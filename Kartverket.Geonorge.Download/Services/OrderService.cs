using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Models.Api.Internal;
using log4net;
using LinqKit;
using System.Data.Entity;
using Geonorge.AuthLib.Common;

namespace Kartverket.Geonorge.Download.Services
{
    public class OrderService : IOrderService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IClipperService _clipperService;
        private readonly DownloadContext _dbContext;
        private readonly IRegisterFetcher _registerFetcher;
        private readonly IOrderBundleService _orderBundleService;
        private readonly INotificationService _notificationService;

        public OrderService(DownloadContext dbContext, 
            IClipperService clipperService, 
            IRegisterFetcher registerFetcherFetcher, 
            IOrderBundleService orderBundleService,
            INotificationService notificationService)
        {
            _dbContext = dbContext;
            _clipperService = clipperService;
            _registerFetcher = registerFetcherFetcher;
            _orderBundleService = orderBundleService;
            _notificationService = notificationService;
        }

        public Order CreateOrder(OrderType incomingOrder, AuthenticatedUser authenticatedUser)
        {
            var order = new Order
            {
                email = incomingOrder.email,
                UsageGroup = incomingOrder.usageGroup,
                SoftwareClient = incomingOrder.softwareClient,
                SoftwareClientVersion = incomingOrder.softwareClientVersion
            };

            if (authenticatedUser != null)
                order.username = authenticatedUser.UsernameForStorage();

            order.AddOrderItems(GetOrderItemsForPredefinedAreas(incomingOrder));
            List<OrderItem> clippableOrderItems = _clipperService.GetClippableOrderItems(incomingOrder);
            order.AddOrderItems(clippableOrderItems);
            
            CheckAccessRestrictions(order, authenticatedUser);

            SaveOrder(order);

            _clipperService.SendClippingRequests(clippableOrderItems, order.email);

            return order;
        }

        // ReSharper disable once UnusedParameter.Local
        public void CheckAccessRestrictions(Order order, AuthenticatedUser authenticatedUser)
        {
            var accessRestrictions = GetAccessRestrictionsForOrder(order);
            bool hasAnyRestrictedDatasets = accessRestrictions.Any();

            if (hasAnyRestrictedDatasets && authenticatedUser == null)
                throw new AccessRestrictionException("Order contains restricted datasets, but no user information is provided.");

            var accessRestrictionsRequiredRole = accessRestrictions.
                Where(a => a.AccessConstraint.RequiredRoles.Any());

            if (hasAnyRestrictedDatasets && !authenticatedUser.HasRole(GeonorgeRoles.MetadataAdmin) && accessRestrictionsRequiredRole.Any())
            {
                foreach(var dataset in accessRestrictionsRequiredRole)
                {
                    foreach(var requiredRole in dataset.AccessConstraint.RequiredRoles)
                    if (!authenticatedUser.HasRole(requiredRole))
                        throw new AccessRestrictionException("Order contains restricted datasets, but user does not have required role");
                }
            }
        }

        private List<OrderItem> GetOrderItemsForPredefinedAreas(OrderType order)
        {
            var orderItems = new List<OrderItem>();

            foreach (var orderLine in order.orderLines)
            {
                var query = _dbContext.FileList.AsExpandable();
                query = query.Where(f => f.Dataset.MetadataUuid == orderLine.metadataUuid);

                if (orderLine.projections != null && orderLine.projections.Any())
                {
                    var projections = orderLine.projections.Select(p => p.code).ToList();
                    query = query.Where(p => projections.Contains(p.Projection));
                }

                if (orderLine.formats != null && orderLine.formats.Any())
                {
                    var formats = orderLine.formats.Select(p => p.name).ToList();
                    query = query.Where(f => formats.Contains(f.Format));
                }

                if (orderLine.areas != null && orderLine.areas.Any())
                {
                    var areas = orderLine.areas.Select(a => new {a.code, a.type});

                    var predicate = PredicateBuilder.False<File>();
                    areas = areas.ToList();

                    foreach (var area in areas)
                    {
                        predicate = predicate.Or(a => a.Division == area.type && a.DivisionKey == area.code);
                    }

                    query = query.Where(predicate);
                }

                List<File> files = query.ToList();

                foreach (File item in files)
                {
                    orderItems.Add(new OrderItem
                    {
                        DownloadUrl = item.Url,
                        FileName = item.Filename,
                        FileUuid = item.Id,
                        Format = item.Format,
                        Area = item.DivisionKey,
                        AreaName = _registerFetcher.GetArea(item.Division, item.DivisionKey).name,
                        Projection = item.Projection,
                        ProjectionName =  _registerFetcher.GetProjection(item.Projection).name,
                        MetadataUuid = orderLine.metadataUuid,
                        Status = OrderItemStatus.ReadyForDownload,
                        MetadataName = item.Dataset.Title,
                        UsagePurpose = orderLine.usagePurpose
                    });
                }
            }
            return orderItems;
        }

        private void SaveOrder(Order order)
        {
            _dbContext.OrderDownloads.Add(order);
            _dbContext.SaveChanges();
        }

        public void UpdateFileStatus(UpdateFileStatusInformation updateFileStatusInformation)
        {
            OrderItem orderItem = _dbContext.OrderItems.FirstOrDefault(x => x.Uuid == updateFileStatusInformation.FileIdAsGuid);
            if (orderItem == null)
                throw new ArgumentException("Invalid file id - no such file exists.");

            orderItem.DownloadUrl = updateFileStatusInformation.DownloadUrl;
            orderItem.Status = updateFileStatusInformation.Status;
            orderItem.Message = updateFileStatusInformation.Message;

            _dbContext.SaveChanges();

            string logMessage = $"OrderItem [id={orderItem.Id}, fileId={orderItem.Uuid}] has been updated. Status: {orderItem.Status}";

            if (orderItem.Status == OrderItemStatus.Error)
                Log.Error($"{logMessage}, Message: {orderItem.Message} ");
            else
                Log.Info($"{logMessage}, DownloadUrl: {orderItem.DownloadUrl} ");

            if (orderItem.Order.IsReadyForBundleDownload()) 
                _orderBundleService.SendToBundling(orderItem.Order);
        }

        public Order Find(string orderUuid)
        {
            Order order = null;
            Guid referenceNumberAsGuid;
            var parseResult = Guid.TryParse(orderUuid, out referenceNumberAsGuid);
            if (parseResult)
            {
                order = _dbContext.OrderDownloads.FirstOrDefault(o => o.Uuid == referenceNumberAsGuid);
                order?.AddAccessConstraints(GetAccessRestrictionsForOrder(order));
            }
            return order;
        }

        public OrderItem FindOrderItem(string fileId)
        {
            var fileIdGuid = Guid.Parse(fileId);

            return _dbContext.OrderItems.Include(p => p.Order).FirstOrDefault(o => o.Uuid == fileIdGuid);
        }

        private List<DatasetAccessConstraint> GetAccessRestrictionsForOrder(Order order)
        {
            List<string> distinctMetadataUuids = order.orderItem.Select(o => o.MetadataUuid).Distinct().ToList();

            List<DatasetAccessConstraint> accessConstraints = _dbContext.Capabilities
                .Where(d => distinctMetadataUuids.Contains(d.MetadataUuid) && d.AccessConstraint != null)
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
            _dbContext.SaveChanges();

            if (order.IsReadyForBundleDownload())
            {
                _orderBundleService.SendToBundling(order);
            }
        }

        public void UpdateOrderStatus(UpdateOrderStatusRequest orderStatus)
        {
            Order order = Find(orderStatus.OrderUuid);
            order.DownloadBundleUrl = orderStatus.DownloadUrl;
            _dbContext.SaveChanges();

            if (orderStatus.Status == "ReadyForDownload" && !order.DownloadBundleNotificationSent.HasValue)
            {
                _notificationService.SendReadyForDownloadBundleNotification(order);
            }

            order.DownloadBundleNotificationSent = DateTime.Now;
            _dbContext.SaveChanges();
        }

        public void AddOrderUsage(DownloadUsage usage)
        {
            _dbContext.DownloadUsages.AddRange(usage.Entries);
            _dbContext.SaveChanges();
        }
    }
}
