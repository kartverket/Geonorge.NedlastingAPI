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
            //Create a stream for the file
            Stream stream = null;

            //This controls how many bytes to read at a time and send to the client
            var bytesToRead = 10000;

            // Buffer to read bytes in chunk size specified above
            var buffer = new byte[bytesToRead];

            // The number of bytes read
            try
            {
                //Create a WebRequest to get the file
                var fileReq = (HttpWebRequest) WebRequest.Create(url);

                //Create a response for this request
                var fileResp = (HttpWebResponse) fileReq.GetResponse();

                if (fileReq.ContentLength > 0)
                    fileResp.ContentLength = fileReq.ContentLength;

                //Get the Stream returned from the response
                stream = fileResp.GetResponseStream();

                // prepare the response to the client. resp is the client Response
                var response = HttpContext.Current.Response;

                //Indicate the type of data being sent
                response.ContentType = "application/octet-stream";

                //Name the file 

                var fileName = url.Substring(url.LastIndexOf('/') + 1);

                response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
                response.AddHeader("Content-Length", fileResp.ContentLength.ToString());

                int length;
                do
                {
                    // Verify that the client is connected.
                    if (response.IsClientConnected)
                    {
                        // Read data into the buffer.
                        length = stream.Read(buffer, 0, bytesToRead);

                        // and write it out to the response's output stream
                        response.OutputStream.Write(buffer, 0, length);

                        // Flush the data
                        response.Flush();

                        //Clear the buffer
                        buffer = new byte[bytesToRead];
                    }
                    else
                    {
                        // cancel the download if client has disconnected
                        length = -1;
                    }
                } while (length > 0); //Repeat until no data is read

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Error serving file from url: " + url, e);
                throw;
            }
            finally
            {
                //Close the input stream
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

            var urlBuilder = new StringBuilder(areaCheckerUrl);

            urlBuilder.Append("CLIPPERCOORDS=").Append(coordinates);
            urlBuilder.Append("&CLIPPERCOORDSYS=").Append(coordinateSystem);
            urlBuilder.Append("&UUID=").Append(metadataUuid);

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
