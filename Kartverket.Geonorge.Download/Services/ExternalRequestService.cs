using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kartverket.Geonorge.Download.Services
{
    internal class ExternalRequestService : IExternalRequestService
    {
        private static readonly HttpClient HttpClient;

        static ExternalRequestService()
        {
            HttpClient = new HttpClient();
            HttpClient.Timeout = TimeSpan.FromSeconds(20);
        }

        public async Task<HttpResponseMessage> RunRequestAsync(string url)
        {
            // http://blog.stephencleary.com/2012/07/dont-block-on-async-code.html

            return await HttpClient.GetAsync(url).ConfigureAwait(false);
        }
    }
}