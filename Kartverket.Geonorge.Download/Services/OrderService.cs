using Kartverket.Geonorge.Download.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            orderReceipt.referenceNumber = SaveOrder(orderReceipt, order.email);

            return orderReceipt;
        }


        private FileType[] GetFiles(OrderType o)
        {
            List<FileType> fileList = new List<FileType>();

            foreach (var orderLine in o.orderLines)
            {

                IQueryable<filliste> query = db.FileList.AsExpandable();
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

                if (orderLine.areas != null) { 
                    var areas = orderLine.areas.Select(a => new { code = a.code, type = a.type });
                    
                    var predicate = PredicateBuilder.False<filliste>();
                    areas = areas.ToList();

                    foreach (var area in areas)
                    {
                        predicate = predicate.Or(a => a.inndeling == area.type && a.inndelingsverdi == area.code);
                    }

                    query = query.Where(predicate);

                }

                var files = query.ToList();
            
                foreach(var file in files)
                {
                    FileType ft = new FileType();
                    ft.downloadUrl = file.url;
                    ft.name = file.filnavn;
                    fileList.Add(ft);
                }
            }

            return fileList.ToArray();
        }

        private string SaveOrder(OrderReceiptType receipt, string email)
        {
            orderDownload o = new orderDownload();
            o.email = email;
            o.orderDate = DateTime.Now;
            db.OrderDownloads.Add(o);
            db.SaveChanges();

            foreach (var oItem in receipt.files) 
            {
                orderItem downloadedItem = new orderItem();
                downloadedItem.downloadUrl = oItem.downloadUrl;
                downloadedItem.fileName = oItem.name;
                downloadedItem.referenceNumber = o.referenceNumber;
                o.orderItem.Add(downloadedItem);
            }
            db.Entry(o).State = EntityState.Modified;
            db.SaveChanges();

            return o.referenceNumber.ToString();
        }

    }
}