using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace Geonorge.Download.Services
{
    public class DownloadService(ILogger<DownloadService> logger, IConfiguration config, IHttpClientFactory httpClientFactory) : IDownloadService
    {
        public async Task StreamRemoteFileToResponseAsync(HttpContext httpContext, string url)
        {
            const int bufferSize = 10_000;

            var cancellationToken = httpContext.RequestAborted;

            try
            {
                var client = httpClientFactory.CreateClient();

                using var remoteResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!remoteResponse.IsSuccessStatusCode || remoteResponse.Content == null)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    await httpContext.Response.WriteAsync("File not found or inaccessible.", cancellationToken);
                    return;
                }

                var remoteStream = await remoteResponse.Content.ReadAsStreamAsync(cancellationToken);

                if (remoteStream == null)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    await httpContext.Response.WriteAsync("File stream was null.", cancellationToken);
                    return;
                }

                var response = httpContext.Response;
                response.ContentType = "application/octet-stream";

                var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
                response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";

                if (remoteResponse.Content.Headers.ContentLength.HasValue)
                    response.Headers["Content-Length"] = remoteResponse.Content.Headers.ContentLength.Value.ToString();

                var buffer = new byte[bufferSize];
                int bytesRead;

                while ((bytesRead = await remoteStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                {
                    await response.Body.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    await response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Client disconnected during streaming of file from: {Url}", url);
                // No rethrow needed — cancellation ends the response
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception while streaming file from: {Url}", url);
                if (!httpContext.Response.HasStarted)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await httpContext.Response.WriteAsync("An error occurred while streaming the file.", cancellationToken);
                }
            }
        }

        public bool AreaIsWithinDownloadLimits(string coordinates, string coordinateSystem, string metadataUuid)
        {
            var areaCheckerApiUrl = MakeAreaCheckerApiUrl(coordinates, coordinateSystem, metadataUuid);

            var areaCheckerResult = CallAreaChecker(areaCheckerApiUrl);

            var areaIsWithinDownloadLimits = areaCheckerResult.Value<bool>("allowed");

            return areaIsWithinDownloadLimits;
        }

        private string MakeAreaCheckerApiUrl(string coordinates, string coordinateSystem, string metadataUuid)
        {
            var areaCheckerUrl = config["FmeAreaChecker"];
            var areaCheckerToken = config["FmeAreaCheckerToken"];

            var urlBuilder = new StringBuilder(areaCheckerUrl);

            urlBuilder.Append("CLIPPERCOORDS=").Append(coordinates);
            urlBuilder.Append("&CLIPPERCOORDSYS=").Append(coordinateSystem);
            urlBuilder.Append("&UUID=").Append(metadataUuid);
            urlBuilder.Append("&token=").Append(areaCheckerToken);

            return urlBuilder.ToString();
        }

        private JObject CallAreaChecker(string url)
        {
            string jsonResult;

            var request = (HttpWebRequest) WebRequest.Create(url);
            logger.LogInformation("Area checker request: " + url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    jsonResult = reader.ReadToEnd();
                }
                logger.LogInformation("Area checker response: " + ((HttpWebResponse)response).StatusCode + " Body: "+jsonResult);
            }
            catch (WebException exception)
            {
                var errorResponse = exception.Response;
                
                using (var responseStream = errorResponse.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    var errorText = reader.ReadToEnd();
                    logger.LogError(errorText, exception);
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
            logger.LogInformation("Clipper file checker request: " + url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    jsonResult = reader.ReadToEnd();
                }
                logger.LogInformation("Clipper file checker response: " + ((HttpWebResponse)response).StatusCode + " Body: " + jsonResult);
            }
            catch (WebException exception)
            {
                var errorResponse = exception.Response;

                using (var responseStream = errorResponse.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    var errorText = reader.ReadToEnd();
                    logger.LogError(errorText, exception);
                }
                throw;
            }

            jsonResult = jsonResult.Trim('[', ']'); // [{"allowed":true}] -> {"allowed":true}

            return JObject.Parse(jsonResult);
        }
    }
}
