using System;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kartverket.Geonorge.Download.Models;
using log4net;

namespace Kartverket.Geonorge.Download.Services
{
    /// <summary>
    /// This service forward a bundle request to the FME bundling service. 
    /// All files in an order is bundled together in a zip file. 
    /// See Jira #GEOPORTAL-2616 for details.
    /// </summary>
    public class OrderBundleService : IOrderBundleService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private readonly IExternalRequestService _externalRequestService;


        public OrderBundleService(IExternalRequestService externalRequestService)
        {
            _externalRequestService = externalRequestService;
        }

        public void SendToBundling(Order order)
        {
            string bundleRequestUrl = CreateBundleRequestUrl(order);

            Log.Info($"Sending order [uuid={order.Uuid}, referenceNumber={order.referenceNumber}] to bundling.");
            Log.Info("Bundling request: " + bundleRequestUrl);

            HttpResponseMessage httpResponseMessage;
            try
            {
                Task<HttpResponseMessage> httpRequestTask = _externalRequestService.RunRequestAsync(bundleRequestUrl);
                httpResponseMessage = httpRequestTask.Result;

                Log.Info("Response from bundle service: " + httpResponseMessage.StatusCode);
            }
            catch (Exception e)
            {
                throw new ExternalRequestException(bundleRequestUrl, e);
            }
            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new ExternalRequestException(bundleRequestUrl, httpResponseMessage);

        }

        private static string CreateBundleRequestUrl(Order order)
        {
            string orderBundleServiceUrl = ConfigurationManager.AppSettings["OrderBundleServiceUrl"];
            var urlBuilder = new StringBuilder(orderBundleServiceUrl);
            urlBuilder.Append("?");
            //var fileIds = string.Join(" ", order.CollectFileIdsForBundling());
            //urlBuilder.Append("UUIDS=").Append(fileIds);
            string server = ConfigurationManager.AppSettings["DownloadUrl"];
            urlBuilder.Append("UUIDFILE=").Append(System.Web.HttpUtility.UrlEncode($"{server}api/order/uuidfile/{order.Uuid}"));
            urlBuilder.Append("&ORDERID=").Append(order.Uuid);
            urlBuilder.Append("&opt_servicemode=async");
            var bundleRequestUrl = urlBuilder.ToString();
            return bundleRequestUrl;
        }
    }
}