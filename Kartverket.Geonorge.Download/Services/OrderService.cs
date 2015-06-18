using Kartverket.Geonorge.Download.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Services
{
    public class OrderService
    {
        DownloadContext db = new DownloadContext();

        public OrderService() 
        {
        }

        internal OrderReceiptType Order(OrderType order)
        {
            OrderReceiptType orderReceipt = new OrderReceiptType();
            orderReceipt.files = GetFiles(order);
            orderReceipt.referenceNumber = SaveOrder(orderReceipt);
            SendEmailReceipt(orderReceipt, order.email);

            return orderReceipt;
        }


        private FileType[] GetFiles(OrderType o)
        {
            List<FileType> fileList = new List<FileType>();

            foreach (var orderLine in o.orderLines)
            {
                var filesQuery = from f in db.FileList
                             where f.Dataset1.metadataUuid == orderLine.metadataUuid                            
                             select new {url = f.url, name = f.filnavn};

            var files = filesQuery.ToList();
            
                foreach(var file in files)
                {
                    FileType ft = new FileType();
                    ft.downloadUrl = file.url;
                    ft.name = file.name;
                    fileList.Add(ft);
                }
            }

            return fileList.ToArray();
        }

        private string SaveOrder(OrderReceiptType receipt)
        {
            //TODO save to db

            //Fake reference number
            Random rnd = new Random(); string referenceNumber = rnd.Next(1000).ToString();

            return referenceNumber;
        }

        private void SendEmailReceipt(OrderReceiptType orderReceipt, string email)
        {
            //TODO send orderReceipt email 
        }
    }
}