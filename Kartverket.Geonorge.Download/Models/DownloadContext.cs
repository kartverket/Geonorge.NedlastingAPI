using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class DownloadContext: DbContext
    {
        public DbSet<Dataset> Capabilities { get; set; }
        public DbSet<filliste> FileList { get; set; }
        public DbSet<Order> OrderDownloads { get; set; } 

        public DbSet<OrderItem> OrderItems { get; set; }

        public DownloadContext()
            : base("DefaultConnection")
        {
        }

    }
}