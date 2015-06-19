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

        public DownloadContext()
            : base("DefaultConnection")
        {
            //prevents EF from attempting to create the database or generate migrations
            Database.SetInitializer<DownloadContext>(null);
        }
    }
}