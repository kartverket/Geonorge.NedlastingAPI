using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;
using log4net;

namespace Kartverket.Geonorge.Download.Services
{
    public class ClipperService : IClipperService
    {
        private const string DefaultEpsgCode = "25833";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DownloadContext _dbContext;
        private readonly IRegisterFetcher _registerFetcher;

        public ClipperService(DownloadContext dbContext, IRegisterFetcher registerFetcherFetcher)
        {
            _dbContext = dbContext;
            _registerFetcher = registerFetcherFetcher;
        }

        public List<OrderItem> GetClippableOrderItems(OrderType incomingOrder)
        {
            var orderItems = new List<OrderItem>();

            foreach (var orderLine in incomingOrder.orderLines)
            {
                if (!string.IsNullOrWhiteSpace(orderLine.coordinates) && incomingOrder.email != null && orderLine.projections != null)
                {
                    var epsgCode = !string.IsNullOrEmpty(orderLine.coordinatesystem) ? orderLine.coordinatesystem : DefaultEpsgCode;

                    foreach (var projection in orderLine.projections)
                    {
                        foreach (var format in orderLine.formats)
                        {
                            var orderItem = new OrderItem
                            {
                                Coordinates = orderLine.coordinates,
                                CoordinateSystem = epsgCode,
                                Format = format.name,
                                Projection = projection.code,
                                ProjectionName = _registerFetcher.GetProjection(projection.code).name,
                                MetadataUuid = orderLine.metadataUuid,
                                MetadataName = GetMetadataName(orderLine.metadataUuid)
                            };

                            orderItems.Add(orderItem);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(orderLine.coordinates) && orderLine.areas != null && incomingOrder.email != null)
                {
                    var matrikkelEiendomAreas = orderLine.areas.Where(a => a.type == Constants.MatrikkelEiendomAreaType).ToList();

                    if(matrikkelEiendomAreas.Any())
                    {
                        foreach (var projection in orderLine.projections)
                        {
                            foreach (var format in orderLine.formats)
                            {
                                var orderItem = new OrderItem
                                {
                                    Area = String.Join(" ", matrikkelEiendomAreas.Select(s => s.code)), 
                                    AreaName = String.Join(" ", matrikkelEiendomAreas.Select(s => s.name)),
                                    Format = format.name,
                                    Projection = projection.code,
                                    ProjectionName = _registerFetcher.GetProjection(projection.code).name,
                                    MetadataUuid = orderLine.metadataUuid,
                                    MetadataName = GetMetadataName(orderLine.metadataUuid)
                                };

                                orderItems.Add(orderItem);
                            }
                        }
                    }
                }
            }
            return orderItems;
        }


        public void SendClippingRequests(List<OrderItem> orderItems, string email)
        {
            foreach (var orderItem in orderItems)
            {
                var clipperUrl = GetClipperServiceUrl(orderItem.MetadataUuid);
                Task.Run(() => { SendClippingRequestAsync(orderItem, email, clipperUrl); });
            }
        }


        private async void SendClippingRequestAsync(OrderItem orderItem, string email, string clipperUrl)
        {
            if (string.IsNullOrWhiteSpace(clipperUrl))
            {
                Log.Error("ClipperUrl is not defined for metadata uuid: " + orderItem.MetadataUuid +
                          ". Cannot process clipping request for orderItem: " + orderItem.Uuid);
                return;
            }

            var urlBuilder = new StringBuilder(clipperUrl);
            if (string.IsNullOrEmpty(orderItem.Coordinates))
            {
                urlBuilder.Append("PARCELIDS=").Append(orderItem.Area);
            }
            else
            { 
                urlBuilder.Append("CLIPPERCOORDS=").Append(orderItem.Coordinates);
                urlBuilder.Append("&CLIPPER_EPSG_CODE=").Append(orderItem.CoordinateSystem);
            }
            urlBuilder.Append("&OUTPUT_EPSG_CODE=").Append(orderItem.Projection);
            urlBuilder.Append("&opt_servicemode=async");
            urlBuilder.Append("&FORMAT=").Append(orderItem.Format);
            urlBuilder.Append("&FILEID=").Append(orderItem.Uuid);
            urlBuilder.Append("&UUID=").Append(orderItem.MetadataUuid);

            // TODO - remove this when FME has implemented callback method
            urlBuilder.Append("&EPOST=").Append(email);

            using (var client = new HttpClient())
            {
                var url = urlBuilder.ToString();
                Log.Info($"Sending clipping request for orderItem [{orderItem.Uuid}]: {url}");
                using (var response = await client.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        Log.Debug($"Result from ClippingService: {result}");
                    }
                }
            }
        }

        private string GetClipperServiceUrl(string metadataUuid)
        {
            return GetDataset(metadataUuid)?.FmeClippingUrl;
        }

        private string GetMetadataName(string metadataUuid)
        {
            return GetDataset(metadataUuid)?.Title;
        }

        private Dataset GetDataset(string metadataUuid)
        {
            return _dbContext.Capabilities.FirstOrDefault(d => d.MetadataUuid == metadataUuid);
        }
    }
}