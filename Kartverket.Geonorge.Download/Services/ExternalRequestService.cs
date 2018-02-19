using System;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace Kartverket.Geonorge.Download.Services
{
    internal class ExternalRequestService : IExternalRequestService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly HttpClient HttpClient;

        static ExternalRequestService()
        {
            HttpClient = new HttpClient();
            HttpClient.Timeout = TimeSpan.FromSeconds(GetTimeoutFromConfig());
        }
        private static int GetTimeoutFromConfig()
        {
            string configValue = ConfigurationManager.AppSettings["HttpTimeout"];
            if (!int.TryParse(configValue, out int result))
            {
                Log.Warn("Invalid configuration variable [HttpTimeout]. Unable to parse int from value [" +
                         configValue + "]");
                result = 60;
            }

            return result;
        }

        public async Task<HttpResponseMessage> RunRequestAsync(string url)
        {
            // http://blog.stephencleary.com/2012/07/dont-block-on-async-code.html

            return await HttpClient.GetAsync(url).ConfigureAwait(false);
        }
    }
}