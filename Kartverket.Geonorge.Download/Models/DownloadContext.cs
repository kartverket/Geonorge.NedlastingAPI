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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<CapabilitiesType>().ToTable("Dataset");

            //modelBuilder.Entity<CapabilitiesType>().Property(t => t.supportsAreaSelection).HasColumnName("supportsAreaSelection");
            //modelBuilder.Entity<CapabilitiesType>().Property(t => t.supportsFormatSelection).HasColumnName("supportsFormatSelection");
            //modelBuilder.Entity<CapabilitiesType>().Property(t => t.supportsPolygonSelection).HasColumnName("supportsPolygonSelection");
            //modelBuilder.Entity<CapabilitiesType>().Property(t => t.supportsProjectionSelection).HasColumnName("supportsProjectionSelection");

            modelBuilder.Entity<ProjectionType>().HasKey(p => p.code);
            modelBuilder.Entity<AreaType>().HasKey(a => a.name);

        }
    }
}