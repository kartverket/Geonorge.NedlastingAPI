using Geonorge.Download.Services.Interfaces;

namespace Geonorge.Download.Services
{
    public class ExternalRequestService(HttpClient httpClient) : IExternalRequestService
    {
        public async Task<HttpResponseMessage> RunRequestAsync(string url)
        {
            // http://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
            //var httpClient = httpClientFactory.CreateClient();
            return await httpClient.GetAsync(url).ConfigureAwait(false);
        }
    }
}