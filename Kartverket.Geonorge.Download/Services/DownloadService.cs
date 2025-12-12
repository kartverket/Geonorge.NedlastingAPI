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
            Stream stream = null;
            var bytesToRead = 64 * 1024; // larger chunk helps throughput
            var buffer = new byte[bytesToRead];

            try
            {
                var fileReq = (HttpWebRequest)WebRequest.Create(url);
                fileReq.AllowReadStreamBuffering = false; // stream directly
                fileReq.Timeout = System.Threading.Timeout.Infinite; // beware: ties up the request thread
                fileReq.ReadWriteTimeout = System.Threading.Timeout.Infinite;

                var fileResp = (HttpWebResponse)fileReq.GetResponse();
                stream = fileResp.GetResponseStream();

                var response = HttpContext.Current.Response;
                response.BufferOutput = false; // stream, don’t buffer whole response
                response.ContentType = "application/octet-stream";

                var fileName = url.Substring(url.LastIndexOf('/') + 1);
                response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                if (fileResp.ContentLength > 0)
                    response.AddHeader("Content-Length", fileResp.ContentLength.ToString());

                // Extend server-side script timeout for long transfers
                HttpContext.Current.Server.ScriptTimeout = 24 * 60 * 60; // 24h

                int length;
                do
                {
                    if (!response.IsClientConnected)
                    {
                        length = -1;
                        break;
                    }

                    length = stream.Read(buffer, 0, bytesToRead);
                    if (length > 0)
                        response.OutputStream.Write(buffer, 0, length);

                    // Avoid per-chunk Flush() to prevent slow client stalls and header finalization issues
                } while (length > 0);

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Error serving file from url: " + url, e);
                throw;
            }
            finally
            {
                stream?.Close();
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
