namespace Kartverket.Geonorge.Download.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("orderItem")]
    public partial class orderItem
    {
        [Key]
        public int id { get; set; }

        public int referenceNumber { get; set; }

        public string downloadUrl { get; set; }

        public string fileName { get; set; }

        public virtual orderDownload orderDownload { get; set; }
    }
}
