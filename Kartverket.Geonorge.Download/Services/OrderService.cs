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
using System.Net.Http;
using System.Configuration;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;

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
        private readonly IEiendomService _eiendomService;

        public OrderService(DownloadContext dbContext, 
            IClipperService clipperService, 
            IRegisterFetcher registerFetcherFetcher, 
            IOrderBundleService orderBundleService,
            INotificationService notificationService, IEiendomService eiendomService)
        {
            _dbContext = dbContext;
            _clipperService = clipperService;
            _registerFetcher = registerFetcherFetcher;
            _orderBundleService = orderBundleService;
            _notificationService = notificationService;
            _eiendomService = eiendomService;
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

            List<Eiendom> eiendoms = null;

            if (authenticatedUser != null) { 
                if(authenticatedUser.HasRole(AuthConfig.DatasetAgriculturalPartyRole))
                    eiendoms = GetEiendomsForUser(authenticatedUser);
            }

            order.AddOrderItems(GetOrderItemsForPredefinedAreas(incomingOrder, authenticatedUser));
            List<OrderItem> clippableOrderItems = _clipperService.GetClippableOrderItems(incomingOrder, authenticatedUser, eiendoms);
            order.AddOrderItems(clippableOrderItems);
            
            CheckAccessRestrictions(order, authenticatedUser);

            SaveOrder(order);

            _clipperService.SendClippingRequests(clippableOrderItems, order.email);

            if(clippableOrderItems.Count > 0)
                _notificationService.SendOrderInfoNotification(order, clippableOrderItems);

            return order;
        }

        private List<Eiendom> GetEiendomsForUser(AuthenticatedUser user)
        {
            List<Eiendom> eiendoms = null;

            eiendoms = _eiendomService.GetEiendoms(user);

            return eiendoms;
        }

        // ReSharper disable once UnusedParameter.Local
        public void CheckAccessRestrictions(Order order, AuthenticatedUser authenticatedUser)
        {
            var accessRestrictions = GetAccessRestrictionsForOrder(order);
            bool hasAnyRestrictedDatasets = accessRestrictions.Any();

            if (hasAnyRestrictedDatasets && authenticatedUser == null)
                throw new AccessRestrictionException("Order contains restricted datasets, but no user information is provided.");

            var accessRestrictionsRequiredRole = accessRestrictions.
                Where(a => a.AccessConstraint.RequiredRoles != null);

            if (hasAnyRestrictedDatasets && !authenticatedUser.HasRole(GeonorgeRoles.MetadataAdmin) && accessRestrictionsRequiredRole != null && accessRestrictionsRequiredRole.Any())
            {
                foreach(var dataset in accessRestrictionsRequiredRole)
                {
                    bool access = false;
                    foreach (var requiredRole in dataset.AccessConstraint.RequiredRoles) {
                        if(requiredRole == AuthConfig.DatasetOnlyOwnMunicipalityRole 
                            && authenticatedUser.HasRole(AuthConfig.DatasetOnlyOwnMunicipalityRole))
                        {
                            if(order.orderItem.Where(i => i.MetadataUuid == dataset.MetadataUuid && i.Area != authenticatedUser.MunicipalityCode).Any())
                                throw new AccessRestrictionException("Order contains restricted datasets, but user does not have required role for area for " + dataset.MetadataUuid);
 
                        }
                        if (authenticatedUser.HasRole(requiredRole))
                            access = true;
                    }

                    if (!access)
                        throw new AccessRestrictionException("Order contains restricted datasets, but user does not have required role for " + dataset.MetadataUuid);
                }
            }

            var accessRestrictionsRequiredRoleFiles = accessRestrictions.
                Where(a => a.FileAccessConstraints != null);

            if (hasAnyRestrictedDatasets && !authenticatedUser.HasRole(GeonorgeRoles.MetadataAdmin) && accessRestrictionsRequiredRoleFiles != null && accessRestrictionsRequiredRoleFiles.Any())
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
                            if (authenticatedUser.HasRole(role))
                                access = true;
                    }
                    if (!access)
                        throw new AccessRestrictionException("Order contains restricted datasets, but user does not have required role for " + fileAccessConstraint.File);
                }
            }

        }

        private List<OrderItem> GetOrderItemsForPredefinedAreas(OrderType order, AuthenticatedUser authenticatedUser)
        {
            var orderItems = new List<OrderItem>();

            foreach (var orderLine in order.orderLines)
            {
                IEnumerable<File> files = new List<File>();

                var sqlDataset = "select Tittel from Dataset where metadataUuid = @p0";
                var datasetTitle = _dbContext.Database.SqlQuery<string>(sqlDataset, orderLine.metadataUuid).FirstOrDefault();
                sqlDataset = "select AccessConstraintRequiredRole from Dataset where metadataUuid = @p0";
                var accessConstraintRequiredRole = _dbContext.Database.SqlQuery<string>(sqlDataset, orderLine.metadataUuid).FirstOrDefault();

                if (authenticatedUser != null && authenticatedUser.HasRole(AuthConfig.DatasetAgriculturalPartyRole) &&
                    !string.IsNullOrEmpty(accessConstraintRequiredRole) && accessConstraintRequiredRole.Contains(AuthConfig.DatasetAgriculturalPartyRole))
                    continue;

                int initialCount = 0;
                int count = 0;

                var sql = "select filliste.[id] as Id, filliste.[filnavn] as Filename ,filliste.[url] as Url,filliste.[kategori] as Category ,filliste.[underkategori] as SubCategory, filliste.[inndeling] as Division,filliste.[inndelingsverdi] as DivisionKey ,filliste.[projeksjon] as Projection ,filliste.[format] as Format ,filliste.[dataset] as DatasetId ,filliste.[AccessConstraintRequiredRole] as AccessConstraintRequiredRole from filliste, Dataset where filliste.dataset = Dataset.id and dataset.metadataUuid = @p" + initialCount++;
                List<string> parameters = new List<string>();
                parameters.Add(orderLine.metadataUuid);

                if (orderLine.projections != null && orderLine.projections.Any())
                {
                    var projections = orderLine.projections.Select(p => p.code).ToList();
                    if (projections.Any())
                        sql = sql + " AND (";

                    var initial = true;

                    foreach (var projection in projections)
                    {
                        if (initial)
                            initial = false;
                        else
                            sql = sql + " OR ";

                        sql = sql + "(projeksjon = @p" + initialCount++ + " )";
                        parameters.Add(projection);

                    }

                    if (projections.Any())
                        sql = sql + " )";

                }

                if (orderLine.formats != null && orderLine.formats.Any())
                {
                    var formats = orderLine.formats.Select(p => p.name).ToList();

                    if (formats.Any())
                        sql = sql + " AND (";

                    var initial = true;

                    foreach (var format in formats)
                    {
                        if (initial)
                            initial = false;
                        else
                            sql = sql + " OR ";

                        sql = sql + "(format =  @p" + initialCount++ + " )";
                        parameters.Add(format);
                    }

                    if (formats.Any())
                        sql = sql + " )";

                }
                List<string> parametersArea = new List<string>();

                if (orderLine.areas != null && orderLine.areas.Any())
                {
                    count = initialCount;
                    var areas = orderLine.areas.Select(a => new { a.code, a.type });

                    areas = areas.ToList();
                    string sqlArea = "";

                    if (areas.Any())
                        sqlArea = sqlArea + " AND (";

                    var initial = true;

                    foreach (var area in areas)
                    {

                        if (initial)
                            initial = false;
                        else
                            sqlArea = sqlArea + " OR ";

                        sqlArea = sqlArea + "(inndeling = @p" + count++ + " AND inndelingsverdi = @p" + count++ + " )" ;
                        parametersArea.Add(area.type);
                        parametersArea.Add(area.code);

                        if (count > 2000)
                        {
                            List<string> param2 = parameters;
                            param2 = param2.Concat(parametersArea).ToList();
                            var sqlStatement = sql + sqlArea + ") ";
                            files = files.Concat(_dbContext.Database.SqlQuery<File>(sqlStatement, param2.ToArray()).ToList());
                            sqlArea = "";
                            count = initialCount;
                            parametersArea = new List<string>();
                            initial = true;
                        }
                    }
                    if(!sqlArea.StartsWith(" AND"))
                        sql = sql + " AND (" + sqlArea + ") ";
                    else
                        sql = sql + " " + sqlArea + " ) ";
                }

                object[] param = parameters.ToArray();
                param = param.Concat(parametersArea).ToList().ToArray();

                files = files.Concat(_dbContext.Database.SqlQuery<File>(sql, param).Distinct().ToList());

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
                        MetadataName = datasetTitle,
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

            List<string> distinctFileNames = order.orderItem.Select(o => o.FileName).Distinct().ToList();
            List<FileAccessConstraint> accessConstraintsFiles = null;
            if(distinctFileNames != null && distinctFileNames.Count > 0)
            accessConstraintsFiles = _dbContext.FileList.Where(f => distinctFileNames.Contains(f.Filename) && f.AccessConstraintRequiredRole != null)
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
                order.DownloadBundleNotificationSent = DateTime.Now;
            }

            _dbContext.SaveChanges();
        }

        public void AddOrderUsage(DownloadUsage usage)
        {
            _dbContext.DownloadUsages.AddRange(usage.Entries);
            _dbContext.SaveChanges();
        }

        public void SendStatusNotification()
        {
            var orders = _dbContext.OrderItems.Where(s => s.Status == OrderItemStatus.WaitingForProcessing && !(s.Order.email == null || s.Order.email.Trim() == string.Empty)).Select(o => o.Order).Distinct().ToList();

            foreach(var order in orders) 
            {
                _notificationService.SendOrderStatusNotification(order);
            }
        }

        public void SendStatusNotificationNotDeliverable()
        {
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            var orders = _dbContext.OrderItems.Where(s => s.Status == OrderItemStatus.WaitingForProcessing && !(s.Order.email == null || s.Order.email.Trim() == string.Empty) && s.Order.orderDate <= sevenDaysAgo).Select(o => o.Order).Distinct().ToList();

            foreach (var order in orders)
            {
                _notificationService.SendOrderStatusNotificationNotDeliverable(order);
            }

            DeleteOldPeronalData();

        }

        protected void DeleteOldPeronalData()
        {
            //Remove personal info older than 7 days
            _dbContext.Database.ExecuteSqlCommand("UPDATE [kartverket_nedlasting].[dbo].[orderDownload] set email = '', username = '' where email<>'' AND orderDate < DATEADD(day, -7, GETDATE())");
        }

        public void CheckPackageSize(Order order)
        {
            string url = CreatePackageCheckSizeUrl(order);

            string jsonResult;

            var request = (HttpWebRequest)WebRequest.Create(url);
            Log.Info("Check package size request: " + url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
                    jsonResult = reader.ReadToEnd();
                }
                Log.Info("Check package size: " + ((HttpWebResponse)response).StatusCode + " Body: " + jsonResult);
            }
            catch (WebException exception)
            {
                var errorResponse = exception.Response;

                using (var responseStream = errorResponse.GetResponseStream())
                {
                    var reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    var errorText = reader.ReadToEnd();
                    Log.Error(errorText, exception);
                }
                throw;
            }

            jsonResult = jsonResult.Trim('[', ']'); // [{"allowed":true}] -> {"allowed":true}

            var result = JObject.Parse(jsonResult);

            bool allowed = result.Value<bool>("allowed");

            if(!allowed)
                throw new FileSizeException("Filene er for store å pakke");
        }

        private static string CreatePackageCheckSizeUrl(Order order)
        {
            string orderPackageCheckUrl = ConfigurationManager.AppSettings["FmeCheckPackageSizeUrl"];
            string orderPackageCheckToken = ConfigurationManager.AppSettings["FmeCheckPackageSizeToken"];
            var urlBuilder = new StringBuilder(orderPackageCheckUrl);
            urlBuilder.Append("?");
            string server = ConfigurationManager.AppSettings["DownloadUrl"];
            urlBuilder.Append("UUIDFILE=").Append(System.Web.HttpUtility.UrlEncode($"{server}api/order/uuidfile/{order.Uuid}"));
            urlBuilder.Append("&token=").Append(orderPackageCheckToken);
            var packageCheckRequestUrl = urlBuilder.ToString();
            return packageCheckRequestUrl;
        }
    }
}
