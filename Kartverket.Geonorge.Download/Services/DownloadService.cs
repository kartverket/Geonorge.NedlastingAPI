using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using Kartverket.Geonorge.Download.Models;
using log4net;
using Newtonsoft.Json.Linq;

namespace Kartverket.Geonorge.Download.Services
{
    public class DownloadService : IDownloadService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public HttpResponse CreateResponseFromRemoteFile(string url)
        {
            HttpWebResponse fileResp = null;
            Stream remoteStream = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowReadStreamBuffering = false;         // stream, don't buffer in memory
                request.Timeout = System.Threading.Timeout.Infinite;
                request.ReadWriteTimeout = System.Threading.Timeout.Infinite;
                request.Proxy = null;                             // avoid proxy lookup overhead if not needed
                request.AutomaticDecompression = DecompressionMethods.None;
                request.KeepAlive = true;

                // Raise per-host connection limit to avoid throttling (default = 2)
                var sp = request.ServicePoint;
                if (sp.ConnectionLimit < 100) sp.ConnectionLimit = 100;

                fileResp = (HttpWebResponse)request.GetResponse();
                remoteStream = fileResp.GetResponseStream();

                var response = HttpContext.Current.Response;
                response.BufferOutput = false;                    // don't buffer whole response in ASP.NET
                response.ContentType = "application/octet-stream";

                // Set download filename (best-effort)
                string fileName;
                try
                {
                    var uri = new Uri(url);
                    fileName = Path.GetFileName(uri.LocalPath);
                    if (string.IsNullOrEmpty(fileName)) fileName = "download.bin";
                }
                catch
                {
                    var slash = url.LastIndexOf('/');
                    fileName = slash >= 0 ? url.Substring(slash + 1) : "download.bin";
                }
                response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");

                if (fileResp.ContentLength > 0)
                    response.AddHeader("Content-Length", fileResp.ContentLength.ToString());

                // Long transfers: extend script timeout
                HttpContext.Current.Server.ScriptTimeout = 24 * 60 * 60; // 24h

                const int bufferSize = 64 * 1024;               // 64KB avoids LOH and is a good throughput size
                var buffer = new byte[bufferSize];              // allocate ONCE and REUSE
                int read;
                while ((read = remoteStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (!response.IsClientConnected) break;     // stop if client disconnected
                    response.OutputStream.Write(buffer, 0, read);
                    // Do NOT Flush() per chunk; let IIS/ASP.NET handle it to avoid head-of-line blocking
                }

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Error serving file from url: " + url, e);
                throw;
            }
            finally
            {
                if (remoteStream != null) remoteStream.Dispose();
                if (fileResp != null) fileResp.Dispose();
            }
        }

        public bool AreaIsWithinDownloadLimits(string coordinates, string coordinateSystem, string metadataUuid)
        {
            var areaCheckerApiUrl = MakeAreaCheckerApiUrl(coordinates, coordinateSystem, metadataUuid);

            var areaCheckerResult = CallAreaChecker(areaCheckerApiUrl);

            var areaIsWithinDownloadLimits = areaCheckerResult.Value<bool>("allowed");

            return areaIsWithinDownloadLimits;
        }

        private static string MakeAreaCheckerApiUrl(string coordinates, string coordinateSystem, string metadataUuid)
        {
            var areaCheckerUrl = ConfigurationManager.AppSettings["FmeAreaChecker"];
            var areaCheckerToken = ConfigurationManager.AppSettings["FmeAreaCheckerToken"];

            var urlBuilder = new StringBuilder(areaCheckerUrl);

            urlBuilder.Append("CLIPPERCOORDS=").Append(coordinates);
            urlBuilder.Append("&CLIPPERCOORDSYS=").Append(coordinateSystem);
            urlBuilder.Append("&UUID=").Append(metadataUuid);
            urlBuilder.Append("&token=").Append(areaCheckerToken);

            return urlBuilder.ToString();
        }

        private static JObject CallAreaChecker(string url)
        {
            string jsonResult;

            var request = (HttpWebRequest) WebRequest.Create(url);
            Log.Info("Area checker request: " + url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    jsonResult = reader.ReadToEnd();
                }
                Log.Info("Area checker response: " + ((HttpWebResponse)response).StatusCode + " Body: "+jsonResult);
            }
            catch (WebException exception)
            {
                var errorResponse = exception.Response;
                
                using (var responseStream = errorResponse.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    var errorText = reader.ReadToEnd();
                    Log.Error(errorText, exception);
                }
                throw;
            }

            jsonResult = jsonResult.Trim('[', ']'); // [{"allowed":true}] -> {"allowed":true}

            return JObject.Parse(jsonResult);
        }

        public JObject CallClipperFileChecker(string url)
        {
            string jsonResult;

            var request = (HttpWebRequest)WebRequest.Create(url);
            Log.Info("Clipper file checker request: " + url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    jsonResult = reader.ReadToEnd();
                }
                Log.Info("Clipper file checker response: " + ((HttpWebResponse)response).StatusCode + " Body: " + jsonResult);
            }
            catch (WebException exception)
            {
                var errorResponse = exception.Response;

                using (var responseStream = errorResponse.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    var errorText = reader.ReadToEnd();
                    Log.Error(errorText, exception);
                }
                throw;
            }

            jsonResult = jsonResult.Trim('[', ']'); // [{"allowed":true}] -> {"allowed":true}

            return JObject.Parse(jsonResult);
        }
    }
}
