using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class DownloadContext: DbContext
    {
        public DbSet<CapabilitiesModel> Capabilities { get; set; } 

        public DownloadContext()
            : base("DefaultConnection")
        {
            //prevents EF from attempting to create the database or generate migrations
            Database.SetInitializer<DownloadContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CapabilitiesModel>().ToTable("Dataset");
            modelBuilder.Entity<CapabilitiesModel>().Property(t => t.Capabilities.supportsAreaSelection).HasColumnName("supportsAreaSelection");
            modelBuilder.Entity<CapabilitiesModel>().Property(t => t.Capabilities.supportsFormatSelection).HasColumnName("supportsFormatSelection");
            modelBuilder.Entity<CapabilitiesModel>().Property(t => t.Capabilities.supportsPolygonSelection).HasColumnName("supportsPolygonSelection");
            modelBuilder.Entity<CapabilitiesModel>().Property(t => t.Capabilities.supportsProjectionSelection).HasColumnName("supportsProjectionSelection");
        }
    }
}