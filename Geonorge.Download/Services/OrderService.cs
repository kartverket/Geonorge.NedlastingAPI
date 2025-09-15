using Geonorge.AuthLib.Common;
using Geonorge.Download.Models;
using Geonorge.Download.Models.Api.Internal;
using Geonorge.Download.Services.Exceptions;
using Geonorge.Download.Services.Interfaces;
using Geonorge.NedlastingApi.V3;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Geonorge.Download.Services
{
    public class OrderService(
        ILogger<OrderService> logger,
        IConfiguration config,
        IClipperService clipperService,
        IRegisterFetcher registerFetcher,
        IOrderBundleService orderBundleService,
        INotificationService notificationService, 
        IEiendomService eiendomService, 
        IDownloadService downloadService,
        DownloadContext downloadContext) : IOrderService
    {
        public Order CreateOrder(OrderType incomingOrder, ClaimsPrincipal principal)
        {
            var order = new Order
            {
                email = incomingOrder.email,
                UsageGroup = incomingOrder.usageGroup,
                SoftwareClient = incomingOrder.softwareClient,
                SoftwareClientVersion = incomingOrder.softwareClientVersion
            };

            if (principal != null)
                order.username = principal.UsernameForStorage();

            List<Eiendom> eiendoms = null;

            if (principal != null) { 
                if(principal.IsInRole(AuthConfig.DatasetAgriculturalPartyRole))
                    eiendoms = GetEiendomsForUser(principal);
            }

            order.AddOrderItems(GetOrderItemsForPredefinedAreas(incomingOrder, principal));
            List<OrderItem> clippableOrderItems = clipperService.GetClippableOrderItems(incomingOrder, principal, eiendoms);
            order.AddOrderItems(clippableOrderItems);
            
            CheckAccessRestrictions(order, principal);

            CheckRestrictions(clippableOrderItems);

            SaveOrder(order);

            clipperService.SendClippingRequests(clippableOrderItems, order.email);

            if(clippableOrderItems.Count > 0)
                notificationService.SendOrderInfoNotification(order, clippableOrderItems);

            return order;
        }

        private void CheckRestrictions(List<OrderItem> clippableOrderItems)
        {
            if(clippableOrderItems.Count > 0) 
            {
                foreach(OrderItem item in clippableOrderItems.Where(c => !string.IsNullOrEmpty(c.Coordinates) && !string.IsNullOrWhiteSpace(c.Coordinates))) 
                {
                    var capabilities = downloadContext.Capabilities.Where(c => c.MetadataUuid == item.MetadataUuid).FirstOrDefault();
                    if(capabilities != null) 
                    {
                        if (capabilities.SupportsPolygonSelection.HasValue && capabilities.SupportsPolygonSelection.Value == false) 
                        {
                            logger.LogWarning($"Metadata with uuid: {item.MetadataUuid} does not support polygon selection");
                            throw new AccessRestrictionException($"Metadata with uuid: {item.MetadataUuid} does not support polygon selection");
                        }
                    }

                    var canDownload = true;

                    if (config["FmeAreaCheckerEnabled"].Equals("true"))
                        canDownload = downloadService.AreaIsWithinDownloadLimits(item.Coordinates,
                            item.CoordinateSystem, item.MetadataUuid);

                    if (!canDownload) {
                        logger.LogWarning($"Metadata with uuid: {item.MetadataUuid} has coordinates greater than download limit");
                        throw new AccessRestrictionException($"Metadata with uuid: {item.MetadataUuid} has coordinates greater than download limit");
                    }

                }
            }

            
        }

        private List<Eiendom> GetEiendomsForUser(ClaimsPrincipal user)
        {
            List<Eiendom> eiendoms = null;

            eiendoms = eiendomService.GetEiendoms(user);

            return eiendoms;
        }

        // ReSharper disable once UnusedParameter.Local
        public void CheckAccessRestrictions(Order order, ClaimsPrincipal principal)
        {
            var accessRestrictions = GetAccessRestrictionsForOrder(order);
            bool hasAnyRestrictedDatasets = accessRestrictions.Any();

            if (hasAnyRestrictedDatasets && principal == null)
                throw new AccessRestrictionException("Order contains restricted datasets, but no user information is provided.");

            var accessRestrictionsRequiredRole = accessRestrictions.
                Where(a => a.AccessConstraint.RequiredRoles != null);

            if (hasAnyRestrictedDatasets && !principal.IsInRole(GeonorgeRoles.MetadataAdmin) && accessRestrictionsRequiredRole != null && accessRestrictionsRequiredRole.Any())
            {
                foreach(var dataset in accessRestrictionsRequiredRole)
                {
                    bool access = false;
                    foreach (var requiredRole in dataset.AccessConstraint.RequiredRoles) {
                        if(requiredRole == AuthConfig.DatasetOnlyOwnMunicipalityRole 
                            && principal.IsInRole(AuthConfig.DatasetOnlyOwnMunicipalityRole))
                        {
                            if(order.orderItem.Where(i => i.MetadataUuid == dataset.MetadataUuid && i.Area != principal.MunicipalityCode()).Any())
                                throw new AccessRestrictionException("Order contains restricted datasets, but user does not have required role for area for " + dataset.MetadataUuid);
 
                        }
                        if (principal.IsInRole(requiredRole))
                            access = true;
                    }

                    if (!access)
                        throw new AccessRestrictionException("Order contains restricted datasets, but user does not have required role for " + dataset.MetadataUuid);
                }
            }

            var accessRestrictionsRequiredRoleFiles = accessRestrictions.
                Where(a => a.FileAccessConstraints != null);

            if (hasAnyRestrictedDatasets && !principal.IsInRole(GeonorgeRoles.MetadataAdmin) && accessRestrictionsRequiredRoleFiles != null && accessRestrictionsRequiredRoleFiles.Any())
            {
                foreach (var dataset in accessRestrictionsRequiredRoleFiles)
                {
                    bool access = false;
                    FileAccessConstraint fileAccessConstraint = new FileAccessConstraint();

                    if(dataset.FileAccessConstraints != null && dataset.FileAccessConstraints.Count == 0)
                        access = true;

                    foreach (var file in dataset.FileAccessConstraints) { 
                        fileAccessConstraint = file;
                        foreach (var role in file.Roles)
                            if (principal.IsInRole(role))
                                access = true;
                    }
                    if (!access)
                        throw new AccessRestrictionException("Order contains restricted datasets, but user does not have required role for " + fileAccessConstraint.File);
                }
            }

        }

        private List<OrderItem> GetOrderItemsForPredefinedAreas(OrderType order, ClaimsPrincipal principal)
        {
            var orderItems = new List<OrderItem>();

            if (order?.orderLines == null || !order.orderLines.Any())
            {
                return orderItems;
            }

            foreach (var orderLine in order.orderLines)
            {
                var ds = downloadContext.
                    Capabilities
                    .AsNoTracking()
                    .Where(d => d.MetadataUuid == orderLine.metadataUuid)
                    .Select(d => new
                    {
                        d.Id,
                        d.Title,
                        d.AccessConstraintRequiredRole
                    })
                    .FirstOrDefault();

                if (ds == null)
                    continue;

                if (principal != null
                    && principal.IsInRole(AuthConfig.DatasetAgriculturalPartyRole)
                    && !string.IsNullOrEmpty(ds.AccessConstraintRequiredRole)
                    && ds.AccessConstraintRequiredRole.Contains(AuthConfig.DatasetAgriculturalPartyRole))
                {
                    continue;
                }

                IQueryable<Models.File> query =
                    from f in downloadContext.Set<Models.File>().AsNoTracking()
                    where f.DatasetId == ds.Id
                    select f;

                if (orderLine.projections != null && orderLine.projections.Any())
                {
                    var projectionCodes = orderLine.projections
                        .Select(p => p?.code)
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .ToList();

                    if (projectionCodes.Count > 0)
                        query = query.Where(f => projectionCodes.Contains(f.Projection));
                }

                if (orderLine.formats != null && orderLine.formats.Any())
                {
                    var formatNames = orderLine.formats
                        .Select(p => p?.name)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct()
                        .ToList();

                    if (formatNames.Count > 0)
                        query = query.Where(f => formatNames.Contains(f.Format));
                }

                if (orderLine.areas != null && orderLine.areas.Any())
                {
                    var grouped = orderLine.areas
                        .Where(a => !string.IsNullOrWhiteSpace(a?.type) && !string.IsNullOrWhiteSpace(a?.code))
                        .GroupBy(a => a.type)
                        .ToList();

                    if (grouped.Count > 0)
                    {
                        IQueryable<Models.File> union = null;

                        foreach (var g in grouped)
                        {
                            var localType = g.Key;
                            var codes = g.Select(x => x.code).Distinct().ToList();

                            var sub = query.Where(f => f.Division == localType && codes.Contains(f.DivisionKey));
                            union = union == null ? sub : union.Concat(sub);
                        }
                        if (union != null)
                            query = union.Distinct();
                    }
                }

                var files = query.ToList();

                foreach (var item in files)
                {
                    var areaName = registerFetcher.GetArea(item.Division, item.DivisionKey)?.name;
                    var projectionName = registerFetcher.GetProjection(item.Projection)?.name;

                    orderItems.Add(new OrderItem
                    {
                        DownloadUrl = item.Url,
                        FileName = item.Filename,
                        FileUuid = item.Id,
                        Format = item.Format,
                        Area = item.DivisionKey,
                        AreaName = areaName,
                        Projection = item.Projection,
                        ProjectionName = projectionName,
                        MetadataUuid = orderLine.metadataUuid,
                        Status = OrderItemStatus.ReadyForDownload,
                        MetadataName = ds.Title,
                        UsagePurpose = orderLine.usagePurpose
                    });
                }
            }

            return orderItems;
        }

        private void SaveOrder(Order order)
        {
            downloadContext.OrderDownloads.Add(order);
            downloadContext.SaveChanges();
        }

        public void UpdateFileStatus(UpdateFileStatusInformation updateFileStatusInformation)
        {
            OrderItem orderItem = downloadContext.OrderItems.FirstOrDefault(x => x.Uuid == updateFileStatusInformation.FileIdAsGuid);
            if (orderItem == null)
                throw new ArgumentException("Invalid file id - no such file exists.");

            orderItem.DownloadUrl = updateFileStatusInformation.DownloadUrl;
            orderItem.Status = updateFileStatusInformation.Status;
            orderItem.Message = updateFileStatusInformation.Message;

            downloadContext.SaveChanges();

            string logMessage = $"OrderItem [id={orderItem.Id}, fileId={orderItem.Uuid}] has been updated. Status: {orderItem.Status}";

            if (orderItem.Status == OrderItemStatus.Error)
                logger.LogError($"{logMessage}, Message: {orderItem.Message} ");
            else
                logger.LogInformation($"{logMessage}, DownloadUrl: {orderItem.DownloadUrl} ");

            if (orderItem.Order.IsReadyForBundleDownload()) 
                orderBundleService.SendToBundling(orderItem.Order);
        }

        public Order Find(string orderUuid)
        {
            Order order = null;
            Guid referenceNumberAsGuid;
            var parseResult = Guid.TryParse(orderUuid, out referenceNumberAsGuid);
            if (parseResult)
            {
                order = downloadContext.OrderDownloads.Include(o => o.orderItem).FirstOrDefault(o => o.Uuid == referenceNumberAsGuid);
                order?.AddAccessConstraints(GetAccessRestrictionsForOrder(order));
            }
            return order;
        }

        public OrderItem FindOrderItem(string fileId)
        {
            var fileIdGuid = Guid.Parse(fileId);

            return downloadContext.OrderItems.Include(p => p.Order).FirstOrDefault(o => o.Uuid == fileIdGuid);
        }

        private List<DatasetAccessConstraint> GetAccessRestrictionsForOrder(Order order)
        {
            List<string> distinctMetadataUuids = order.orderItem.Select(o => o.MetadataUuid).Distinct().ToList();

            List<DatasetAccessConstraint> accessConstraints = downloadContext.Capabilities
                .Where(d => distinctMetadataUuids.Contains(d.MetadataUuid) && !string.IsNullOrWhiteSpace(d.AccessConstraint))
                .Select(d => new DatasetAccessConstraint() {
                    MetadataUuid = d.MetadataUuid,
                    AccessConstraint = new AccessConstraint()
                    {
                        Constraint = d.AccessConstraint,
                        RequiredRole = d.AccessConstraintRequiredRole
                    }
                    })
                .ToList();

            if(accessConstraints!= null && accessConstraints.Count > 0)
                accessConstraints = SetMultipleAccessConstraintRequiredRole(accessConstraints);

            List<string> distinctFileNames = order.orderItem.Select(o => o.FileName).Distinct().ToList();
            List<FileAccessConstraint> accessConstraintsFiles = null;
            if(distinctFileNames != null && distinctFileNames.Count > 0)
            accessConstraintsFiles = downloadContext.FileList.Where(f => distinctFileNames.Contains(f.Filename) && !string.IsNullOrWhiteSpace(f.AccessConstraintRequiredRole))
                .Select(a => new FileAccessConstraint
                { MetadataUuid = a.Dataset.MetadataUuid, File = a.Filename, Role = a.AccessConstraintRequiredRole})
                .ToList();

            if(accessConstraintsFiles != null && accessConstraints != null)
            { 
                for (int a = 0; a < accessConstraints.Count; a++)
                {
                    var accessConstraintFilesForUuid = accessConstraintsFiles.Where(f => f.MetadataUuid == accessConstraints[a].MetadataUuid).ToList();
                    if(accessConstraintFilesForUuid != null)
                    {
                        accessConstraints[a].FileAccessConstraints = accessConstraintFilesForUuid;
                        for (int b = 0; b < accessConstraints[a].FileAccessConstraints.Count; b++)
                        {
                            accessConstraints[a].FileAccessConstraints[b].Roles = accessConstraints[a].FileAccessConstraints[b].Role.Split(',').Select(r => r.Trim()).ToList();
                        }
                    }
                }
            }

            return accessConstraints;
        }

        private List<DatasetAccessConstraint> SetMultipleAccessConstraintRequiredRole(List<DatasetAccessConstraint> accessConstraints)
        {
            for (int j = 0; j < accessConstraints.Count;j++)
            {
                var accessConstraint = accessConstraints[j];

                if (!string.IsNullOrEmpty(accessConstraint?.AccessConstraint?.RequiredRole))
                {
                    var requiredRoles = accessConstraint.AccessConstraint.RequiredRole.Split(',').Select(r => r.Trim()).ToList();
                    accessConstraints[j].AccessConstraint.RequiredRoles = requiredRoles;
                }
            }

            return accessConstraints;
        }

        /// <summary>
        /// Updates an order. Currently only these fields are updated:
        /// * email
        /// * downloadAsBundle
        /// </summary>
        /// <param name="order"></param>
        /// <param name="incomingOrder"></param>
        public void UpdateOrder(Order order, OrderType incomingOrder)
        {
            order.DownloadAsBundle = incomingOrder.downloadAsBundle;
            order.email = incomingOrder.email;
            downloadContext.SaveChanges();

            if (order.IsReadyForBundleDownload())
            {
                orderBundleService.SendToBundling(order);
            }
        }

        public void UpdateOrderStatus(UpdateOrderStatusRequest orderStatus)
        {
            Order order = Find(orderStatus.OrderUuid);
            order.DownloadBundleUrl = orderStatus.DownloadUrl;
            downloadContext.SaveChanges();

            if (orderStatus.Status == "ReadyForDownload" && !order.DownloadBundleNotificationSent.HasValue)
            {
                notificationService.SendReadyForDownloadBundleNotification(order);
                order.DownloadBundleNotificationSent = DateTime.UtcNow;
            }

            downloadContext.SaveChanges();
        }

        public void AddOrderUsage(DownloadUsage usage)
        {
            downloadContext.DownloadUsages.AddRange(usage.Entries);
            downloadContext.SaveChanges();
        }

        public void SendStatusNotification()
        {
            var orders = downloadContext.OrderItems.Where(s => s.Status == OrderItemStatus.WaitingForProcessing && !(s.Order.email == null || s.Order.email.Trim() == string.Empty)).Select(o => o.Order).Distinct().ToList();

            foreach(var order in orders) 
            {
                notificationService.SendOrderStatusNotification(order);
            }
        }

        public void SendStatusNotificationNotDeliverable()
        {
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            var orders = downloadContext.OrderItems.Where(s => s.Status == OrderItemStatus.WaitingForProcessing && !(s.Order.email == null || s.Order.email.Trim() == string.Empty) && s.Order.orderDate <= sevenDaysAgo).Select(o => o.Order).Distinct().ToList();

            foreach (var order in orders)
            {
                notificationService.SendOrderStatusNotificationNotDeliverable(order);
            }

            DeleteOldPeronalData();

        }

        protected void DeleteOldPeronalData()
        {
            //Remove personal info older than 7 days
            downloadContext.OrderDownloads
                .Where(o => o.email != "" && o.orderDate < DateTime.Now.AddDays(-7))
                .ExecuteUpdate(setters => setters
                    .SetProperty(o => o.email, o => "")
                    .SetProperty(o => o.username, o => ""));
        }

        public void CheckPackageSize(Order order)
        {
            string url = CreatePackageCheckSizeUrl(order);

            string jsonResult;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 240000;
            logger.LogInformation("Check package size request: " + url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
                    jsonResult = reader.ReadToEnd();
                }
                logger.LogInformation("Check package size: " + ((HttpWebResponse)response).StatusCode + " Body: " + jsonResult);
            }
            catch (WebException exception)
            {
                var errorResponse = exception.Response;

                using (var responseStream = errorResponse.GetResponseStream())
                {
                    var reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    var errorText = reader.ReadToEnd();
                    logger.LogError(errorText, exception);
                }
                throw;
            }

            jsonResult = jsonResult.Trim('[', ']'); // [{"allowed":true}] -> {"allowed":true}

            var result = JObject.Parse(jsonResult);

            bool allowed = result.Value<bool>("allowed");

            if(!allowed)
                throw new FileSizeException("Filene er for store å pakke");
        }

        private string CreatePackageCheckSizeUrl(Order order)
        {
            string orderPackageCheckUrl = config["FmeCheckPackageSizeUrl"];
            string orderPackageCheckToken = config["FmeCheckPackageSizeToken"];
            var urlBuilder = new StringBuilder(orderPackageCheckUrl);
            urlBuilder.Append("?");
            string server = config["DownloadUrl"];
            urlBuilder.Append("UUIDFILE=").Append(System.Web.HttpUtility.UrlEncode($"{server}api/order/uuidfile/{order.Uuid}"));
            urlBuilder.Append("&token=").Append(orderPackageCheckToken);
            var packageCheckRequestUrl = urlBuilder.ToString();
            return packageCheckRequestUrl;
        }
    }
}
