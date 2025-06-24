using Geonorge.Download.Models;
using Geonorge.Download.Services.Exceptions;
using Geonorge.Download.Services.Interfaces;
using System.Text;

namespace Geonorge.Download.Services
{
    /// <summary>
    /// This service forward a bundle request to the FME bundling service. 
    /// All files in an order is bundled together in a zip file. 
    /// See Jira #GEOPORTAL-2616 for details.
    /// </summary>
    public class OrderBundleService(ILogger<OrderBundleService> logger, IConfiguration config, IExternalRequestService externalRequestService) : IOrderBundleService
    {

        public void SendToBundling(Order order)
        {
            var bundleRequestUrl = CreateBundleRequestUrl(order);

            logger.LogInformation("Sending order [uuid={Uuid}, referenceNumber={ReferenceNumber}] to bundling.", order.Uuid, order.referenceNumber);

            logger.LogDebug("Bundling request URL: {Url}", bundleRequestUrl);

            try
            {
                var response = externalRequestService.RunRequestAsync(bundleRequestUrl).Result;

                logger.LogInformation("Response from bundle service: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    throw new ExternalRequestException(bundleRequestUrl, response);
                }
            }
            catch (Exception ex) when (ex is not ExternalRequestException)
            {
                logger.LogError(ex, "Exception occurred while sending to bundling service.");
                throw new ExternalRequestException(bundleRequestUrl, ex);
            }
        }

        private string CreateBundleRequestUrl(Order order)
        {
            string orderBundleServiceUrl = config["OrderBundleServiceUrl"];
            string orderBundleServiceUrlToken = config["OrderBundleServiceUrlToken"];
            var urlBuilder = new StringBuilder(orderBundleServiceUrl);
            urlBuilder.Append("?");
            //var fileIds = string.Join(" ", order.CollectFileIdsForBundling());
            //urlBuilder.Append("UUIDS=").Append(fileIds);
            string server = config["DownloadUrl"];
            urlBuilder.Append("UUIDFILE=").Append(System.Web.HttpUtility.UrlEncode($"{server}api/order/uuidfile/{order.Uuid}"));
            urlBuilder.Append("&ORDERID=").Append(order.Uuid);
            urlBuilder.Append("&opt_servicemode=async");
            urlBuilder.Append("&token=").Append(orderBundleServiceUrlToken);
            var bundleRequestUrl = urlBuilder.ToString();
            return bundleRequestUrl;
        }
    }
}