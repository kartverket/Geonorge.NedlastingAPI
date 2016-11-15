using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Geonorge.NedlastingApi.V2;
using Kartverket.Geonorge.Download.Models;
using log4net;
using LinqKit;

namespace Kartverket.Geonorge.Download.Services
{
    public class OrderService : IOrderService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IClipperService _clipperService;
        private readonly DownloadContext _dbContext;

        public OrderService(DownloadContext dbContext, IClipperService clipperService)
        {
            _dbContext = dbContext;
            _clipperService = clipperService;
        }

        public Order CreateOrder(OrderType incomingOrder, string username)
        {
            var order = new Order
            {
                email = incomingOrder.email,
                username = username
            };
            order.AddOrderItems(GetOrderItemsForPredefinedAreas(incomingOrder));
            order.AddOrderItems(_clipperService.GetClippableOrderItems(incomingOrder));
            SaveOrder(order);
            return order;
        }

        private List<OrderItem> GetOrderItemsForPredefinedAreas(OrderType order)
        {
            var orderItems = new List<OrderItem>();

            foreach (var orderLine in order.orderLines)
            {
                var query = _dbContext.FileList.AsExpandable();
                query = query.Where(f => f.Dataset1.metadataUuid == orderLine.metadataUuid);

                if (orderLine.projections != null)
                {
                    var projections = orderLine.projections.Select(p => p.code).ToList();
                    query = query.Where(p => projections.Contains(p.projeksjon));
                }

                if (orderLine.formats != null)
                {
                    var formats = orderLine.formats.Select(p => p.name).ToList();
                    query = query.Where(f => formats.Contains(f.format));
                }

                if (orderLine.areas != null)
                {
                    var areas = orderLine.areas.Select(a => new {a.code, a.type});

                    var predicate = PredicateBuilder.False<filliste>();
                    areas = areas.ToList();

                    foreach (var area in areas)
                    {
                        predicate = predicate.Or(a => a.inndeling == area.type && a.inndelingsverdi == area.code);
                    }

                    query = query.Where(predicate);
                }

                var files = query.ToList();

                foreach (var item in files)
                {
                    orderItems.Add(new OrderItem
                    {
                        DownloadUrl = item.url,
                        FileName = item.filnavn,
                        Format = item.format,
                        Area = item.inndelingsverdi,
                        Projection = item.projeksjon,
                        MetadataUuid = orderLine.metadataUuid,
                        Status = OrderItemStatus.ReadyForDownload
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
            var orderItem = _dbContext.OrderItems.FirstOrDefault(x => x.FileId == updateFileStatusInformation.FileIdAsGuid);
            if (orderItem == null)
                throw new ArgumentException("Invalid file id - no such file exists.");

            orderItem.DownloadUrl = updateFileStatusInformation.DownloadUrl;
            orderItem.Status = updateFileStatusInformation.Status;
            orderItem.Message = updateFileStatusInformation.Message;

            _dbContext.Entry(orderItem).State = EntityState.Modified;
            _dbContext.SaveChanges();

            string logMessage = $"OrderItem [id={orderItem.Id}, fileId={orderItem.FileId}] has been updated. Status: {orderItem.Status}";

            if (orderItem.Status == OrderItemStatus.Error)
                Log.Error($"{logMessage}, Message: {orderItem.Message} ");
            else
                Log.Info($"{logMessage}, DownloadUrl: {orderItem.DownloadUrl} ");
        }

        public OrderReceiptType Find(int referenceNumber)
        {
            var order = _dbContext.OrderDownloads.Find(referenceNumber);

            return order != null
                ? new OrderReceiptType
                {
                    referenceNumber = order.referenceNumber.ToString(),
                    files = GetFiles(order)
                }
                : null;
        }

        private static FileType[] GetFiles(Order orderDownload)
        {
            return orderDownload.orderItem.Select(orderItem => new FileType
            {
                name = orderItem.FileName,
                downloadUrl = orderItem.DownloadUrl,
                status = orderItem.Status.ToString()
            }).ToArray();
        }
    }
}