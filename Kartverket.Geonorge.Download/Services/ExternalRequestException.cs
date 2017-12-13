using System;
using System.Net.Http;

namespace Kartverket.Geonorge.Download.Services
{
    public class ExternalRequestException : Exception
    {
        public ExternalRequestException(string url, string responseBody)
            : base($"Request to [url={url}] failed. Response from service: {responseBody}")
        {
        }

        public ExternalRequestException(string url, HttpResponseMessage httpResponseMessage)
            : base(
                $"Request to [url={url}] failed. Response from service: " + 
                  $"status={httpResponseMessage.StatusCode}, " + 
                  $"body={httpResponseMessage.Content.ReadAsStringAsync().Result}")
        {
        }

        public ExternalRequestException(string url, Exception innerException)
            : base($"Request to [url={url}] failed.", innerException)
        {
        }
    }
}