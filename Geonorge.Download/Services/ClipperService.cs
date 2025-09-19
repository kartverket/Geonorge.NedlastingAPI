using Geonorge.Download.Models;
using Geonorge.Download.Services.Interfaces;
using Geonorge.NedlastingApi.V3;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace Geonorge.Download.Services
{
    public class ClipperService(ILogger<ClipperService> logger, IConfiguration config, IRegisterFetcher registerFetcher, DownloadContext downloadContext) : IClipperService
    {
        private const string DefaultEpsgCode = "25833";
         
        public List<OrderItem> GetClippableOrderItems(OrderType incomingOrder, ClaimsPrincipal principal = null, List<Eiendom> eiendoms = null)
        {
            var orderItems = new List<OrderItem>();

            foreach (var orderLine in incomingOrder.orderLines)
            {
                if (!string.IsNullOrWhiteSpace(orderLine.coordinates) && !string.IsNullOrWhiteSpace(incomingOrder.email) && orderLine.projections != null)
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
                                ProjectionName = registerFetcher.GetProjection(projection.code).name,
                                MetadataUuid = orderLine.metadataUuid,
                                MetadataName = GetMetadataName(orderLine.metadataUuid)
                            };

                            orderItems.Add(orderItem);
                        }
                    }
                }

                if (eiendoms != null & string.IsNullOrWhiteSpace(orderLine.coordinates) && orderLine.areas != null && !string.IsNullOrWhiteSpace(incomingOrder.email))
                {
                    var sqlDataset = "select AccessConstraintRequiredRole from Dataset where metadataUuid = @p0";
                    var accessConstraintRequiredRole = downloadContext.Database.SqlQueryRaw<string>(sqlDataset, orderLine.metadataUuid).FirstOrDefault();

                    if ( !(principal != null && principal.IsInRole(AuthConfig.DatasetAgriculturalPartyRole) &&
                        !string.IsNullOrEmpty(accessConstraintRequiredRole) && accessConstraintRequiredRole.Contains(AuthConfig.DatasetAgriculturalPartyRole)))
                        continue;

                    var matrikkelEiendomAreas = orderLine.areas.ToList();

                    if(matrikkelEiendomAreas.Any())
                    {
                        var areaCodes = matrikkelEiendomAreas.Select(e => e.code).Distinct().ToArray();

                        if (!matrikkelEiendomAreas.Where(l => l.code == "0000").Any())
                        {
                            var eiendomsQuery = eiendoms.Where(e => areaCodes.Contains(e.kommunenr + "/" + e.gaardsnr + "/" + e.bruksnr + "/" + e.festenr));
                            var municipalityQuery = eiendoms.Where(e => areaCodes.Contains(e.kommunenr));
                            eiendoms = eiendomsQuery.Concat(municipalityQuery).ToList();
                        }

                        var parcels = eiendoms.Select(eiendom => eiendom).ToArray();
                        int itemsPerBatch = 100;
                        for (int i = 0; i < parcels.Length; i += itemsPerBatch)
                        {
                            var parcelsBatched = parcels.Skip(i).Take(itemsPerBatch);

                            var areaNamesBatched = parcelsBatched.Select(e => registerFetcher.GetArea("kommune", e.kommunenr).name).Distinct().ToArray();

                            foreach (var projection in orderLine.projections)
                            {
                                foreach (var format in orderLine.formats)
                                {
                                    var orderItem = new OrderItem
                                    {
                                        Area = String.Join(" ", parcelsBatched.Select(eiendom => $"{eiendom.kommunenr}/{eiendom.gaardsnr}/{eiendom.bruksnr}/{eiendom.festenr}")),
                                        AreaName = String.Join(", ", areaNamesBatched.Select(s => s)),
                                        Format = format.name,
                                        Projection = projection.code,
                                        ProjectionName = registerFetcher.GetProjection(projection.code).name,
                                        MetadataUuid = orderLine.metadataUuid,
                                        MetadataName = GetMetadataName(orderLine.metadataUuid)
                                    };

                                    orderItems.Add(orderItem);
                                }
                            }
                        }
                    }
                }


                if (!string.IsNullOrWhiteSpace(orderLine.clipperFile) && !string.IsNullOrWhiteSpace(incomingOrder.email) && orderLine.projections != null)
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
                                ProjectionName = registerFetcher.GetProjection(projection.code).name,
                                MetadataUuid = orderLine.metadataUuid,
                                MetadataName = GetMetadataName(orderLine.metadataUuid)
                            };

                            orderItems.Add(orderItem);
                        }
                    }
                }


            }
            return orderItems.Where(m => !string.IsNullOrEmpty(m.MetadataName)).ToList();
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
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(email))
            {
                logger.LogError("Email is not defined for metadata uuid: " + orderItem.MetadataUuid +
                          ". Cannot process clipping request for orderItem: " + orderItem.Uuid);
                return;
            }

            if (string.IsNullOrWhiteSpace(clipperUrl))
            {
                logger.LogError("ClipperUrl is not defined for metadata uuid: " + orderItem.MetadataUuid +
                          ". Cannot process clipping request for orderItem: " + orderItem.Uuid);
                return;
            }

            if (string.IsNullOrEmpty(orderItem.Coordinates) && string.IsNullOrEmpty(orderItem.Area))
            {
                logger.LogError("Coordinates or Area for metadata uuid: " + orderItem.MetadataUuid +
                          ". Cannot process clipping request for orderItem: " + orderItem.Uuid);
                return;
            }

            var fmeToken = config["FmeToken"];

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

            if(!string.IsNullOrEmpty(fmeToken))
                urlBuilder.Append("&token=").Append(fmeToken);

            // TODO - remove this when FME has implemented callback method
            urlBuilder.Append("&EPOST=").Append(email);

            using (var client = new HttpClient())
            {
                var url = urlBuilder.ToString();
                logger.LogInformation($"Sending clipping request for orderItem [{orderItem.Uuid}]: {url}");
                using (var response = await client.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        logger.LogDebug($"Result from ClippingService: {result}");
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
            return downloadContext.Capabilities.FirstOrDefault(d => d.MetadataUuid == metadataUuid);
        }
    }
}