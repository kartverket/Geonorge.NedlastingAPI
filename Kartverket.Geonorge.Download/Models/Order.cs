namespace Kartverket.Geonorge.Download.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("orderDownload")]
    public partial class Order
    {
        public Order()
        {
            orderItem = new HashSet<OrderItem>();
        }

        [Key]
        public int referenceNumber { get; set; }

        [StringLength(50)]
        public string email { get; set; }

        public DateTime? orderDate { get; set; }

        public virtual ICollection<OrderItem> orderItem { get; set; }
    }
}
