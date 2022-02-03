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
         
        public List<OrderItem> GetClippableOrderItems(OrderType incomingOrder, AuthenticatedUser authenticatedUser = null, List<Eiendom> eiendoms = null)
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

                if (eiendoms != null & string.IsNullOrWhiteSpace(orderLine.coordinates) && orderLine.areas != null && incomingOrder.email != null)
                {
                    var sqlDataset = "select AccessConstraintRequiredRole from Dataset where metadataUuid = @p0";
                    var accessConstraintRequiredRole = _dbContext.Database.SqlQuery<string>(sqlDataset, orderLine.metadataUuid).FirstOrDefault();

                    if ( !(authenticatedUser != null && authenticatedUser.HasRole(AuthConfig.DatasetAgriculturalPartyRole) &&
                        !string.IsNullOrEmpty(accessConstraintRequiredRole) && accessConstraintRequiredRole.Contains(AuthConfig.DatasetAgriculturalPartyRole)))
                        continue;

                    var matrikkelEiendomAreas = orderLine.areas.ToList();

                    if(matrikkelEiendomAreas.Any())
                    {
                        var areaCodes = matrikkelEiendomAreas.Select(e => e.code).Distinct().ToArray();

                        if (!matrikkelEiendomAreas.Where(l => l.code == "0000").Any())
                        {
                            var eiendomsQuery = eiendoms.Where(e => areaCodes.Contains(e.kommunenr + "/" + e.gaardsnr + "/" + e.bruksnr + "/" + e.festenr));
                            eiendoms = eiendomsQuery.ToList();
                        }

                        foreach (var projection in orderLine.projections)
                        {
                            foreach (var format in orderLine.formats)
                            {
                                var orderItem = new OrderItem
                                {
                                    Area = String.Join(" ", eiendoms.Select(eiendom => $"{eiendom.kommunenr}/{eiendom.gaardsnr}/{eiendom.bruksnr}/{eiendom.festenr}")),
                                    AreaName = String.Join(", ", matrikkelEiendomAreas.Select(s => s.name)),
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


                if (!string.IsNullOrWhiteSpace(orderLine.clipperFile) && incomingOrder.email != null && orderLine.projections != null)
                {

                    foreach (var projection in orderLine.projections)
                    {
                        foreach (var format in orderLine.formats)
                        {
                            var orderItem = new OrderItem
                            {
                                Area = "Klippe-fil",
                                AreaName = "Klippe-fil",
                                ClipperFile = orderLine.clipperFile,
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

            if (string.IsNullOrEmpty(orderItem.Coordinates) && string.IsNullOrEmpty(orderItem.Area))
            {
                Log.Error("Coordinates or Area for metadata uuid: " + orderItem.MetadataUuid +
                          ". Cannot process clipping request for orderItem: " + orderItem.Uuid);
                return;
            }

            var urlBuilder = new StringBuilder(clipperUrl);
            if (!string.IsNullOrEmpty(orderItem.ClipperFile))
            {
                urlBuilder.Append("CLIPPER_FILE=").Append(System.Web.HttpUtility.UrlEncode(orderItem.ClipperFile));
            }
            else if (string.IsNullOrEmpty(orderItem.Coordinates))
            {
                urlBuilder.Append("PARCELIDS=").Append(System.Web.HttpUtility.UrlEncode(orderItem.Area));
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