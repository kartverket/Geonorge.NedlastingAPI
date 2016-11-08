using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Geonorge.Download.Models
{
    [Table("orderDownload")]
    public class Order
    {
        public Order()
        {
            orderItem = new List<OrderItem>();
            orderDate = DateTime.Now;
        }

        [Key]
        public int referenceNumber { get; set; }

        [StringLength(50)]
        public string email { get; set; }

        public DateTime? orderDate { get; set; }

        public string username { get; set; }

        public virtual List<OrderItem> orderItem { get; set; }

        public void AddOrderItems(List<OrderItem> items)
        {
            orderItem.AddRange(items);
        }
    }
}