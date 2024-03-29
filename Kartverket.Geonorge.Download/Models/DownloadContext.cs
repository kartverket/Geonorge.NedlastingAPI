﻿using Kartverket.Geonorge.Download.Helpers;
using System.Data.Entity;

namespace Kartverket.Geonorge.Download.Models
{
    public class DownloadContext : DbContext
    {
        public DownloadContext()
            : base("DefaultConnection")
        {
        }

        public virtual DbSet<Dataset> Capabilities { get; set; }
        public virtual DbSet<File> FileList { get; set; }
        public virtual DbSet<Order> OrderDownloads { get; set; }

        public virtual DbSet<OrderItem> OrderItems { get; set; }

        public virtual DbSet<MachineAccount> MachineAccounts { get; set; }
        
        public virtual DbSet<DownloadUsageEntry> DownloadUsages { get; set; }
        public virtual DbSet<ClipperFile> ClipperFiles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.orderItem)
                .WithRequired(item => item.Order)
                .HasForeignKey(item => item.ReferenceNumber)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Dataset>()
            .HasIndex(d => d.MetadataUuid);

            modelBuilder.Entity<File>()
            .HasIndex(f => f.Division);

            modelBuilder.Entity<File>()
            .HasIndex(f => f.DivisionKey);

            modelBuilder.Entity<File>()
            .HasIndex(f => f.Projection);

            modelBuilder.Entity<File>()
            .HasIndex(f => f.Format);


            modelBuilder.Conventions.Add(new NonPublicColumnAttributeConvention());
        }
    }
}