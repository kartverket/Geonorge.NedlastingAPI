using System;
using System.Data.Entity;
using System.Reflection;
using Kartverket.Geonorge.Download.Models;
using log4net;

namespace Kartverket.Geonorge.Download.Services
{
    public class OrderServiceV2 : IOrderService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DownloadContext _dbContext;

        public OrderServiceV2(DownloadContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void UpdateFileStatus(UpdateFileStatusInformation updateFileStatusInformation)
        {
            OrderItem orderItem = _dbContext.OrderItems.Find(updateFileStatusInformation.FileId);
            if (orderItem == null)
                throw new ArgumentException("Invalid file id - no such file exists.");

            orderItem.DownloadUrl = updateFileStatusInformation.DownloadUrl;
            orderItem.Status = updateFileStatusInformation.Status;
            orderItem.Message = updateFileStatusInformation.Message;

            _dbContext.Entry<OrderItem>(orderItem).State = EntityState.Modified;
            _dbContext.SaveChanges();

            string logMessage = $"OrderItem {orderItem.Id} has been updated. Status: {orderItem.Status}";

            if (orderItem.Status == OrderItemStatus.Error)
                Log.Error($"{logMessage}, Message: {orderItem.Message} ");
            else
                Log.Info($"{logMessage}, DownloadUrl: {orderItem.DownloadUrl} ");
        }
    }
}