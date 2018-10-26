using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Microsoft.Web.Http;

namespace Kartverket.Geonorge.Download.Controllers.Api.Internal
{
    [ApiVersion("3.0")]
    [EnableCors("*", "*", "*", SupportsCredentials = true)]
    [HandleError]
    public class DownloadUsageController : ApiController
    {
        private IOrderService _orderService;

        public DownloadUsageController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Register download usage for statistical purposes. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("api/internal/download-usage")]
        public IHttpActionResult DownloadUsage([FromBody] DownloadUsageRequestType request)
        {
            _orderService.AddOrderUsage(ConvertToDownloadUsage(request));
            return Ok();
        }

        private DownloadUsage ConvertToDownloadUsage(DownloadUsageRequestType request)
        {
            var downloadUsage = new DownloadUsage();
            if (request.entries != null)
            {
                foreach (var entry in request.entries)
                {
                    downloadUsage.AddEntry(new DownloadUsageEntry
                    {
                        Uuid = entry.metadataUuid,
                        AreaCode = entry.areaCode,
                        AreaName = entry.areaName,
                        Format = entry.format,
                        Projection = entry.projection,
                        Group = request.@group,
                        Purpose = request.purpose,
                        SoftwareClient = request.softwareClient,
                        SoftwareClientVersion = request.softwareClientVersion
                    });
                }
            }
            return downloadUsage;
        }
    }
}