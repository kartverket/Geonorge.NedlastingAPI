using Geonorge.Download.Helpers;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Xml;

namespace Geonorge.Download.Models
{
    public class DownloadContext : DbContext
    {
        public DownloadContext(DbContextOptions options) : base(options) { }

        public virtual DbSet<Dataset> Capabilities { get; set; }
        public virtual DbSet<File> FileList { get; set; }
        public virtual DbSet<Order> OrderDownloads { get; set; }

        public virtual DbSet<OrderItem> OrderItems { get; set; }

        public virtual DbSet<MachineAccount> MachineAccounts { get; set; }
        
        public virtual DbSet<DownloadUsageEntry> DownloadUsages { get; set; }
        public virtual DbSet<ClipperFile> ClipperFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.orderItem)
                .WithOne(item => item.Order)
                .HasForeignKey(item => item.ReferenceNumber)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
            .HasIndex(e => e.Uuid)
            .HasDatabaseName("IDX_OrderUuid")
            .IsUnique(false);

            modelBuilder.Entity<OrderItem>()
            .HasIndex(e => e.Uuid)
            .HasDatabaseName("IDX_OrderItemUuid")
            .IsUnique(false);

            modelBuilder.Entity<OrderItem>()
            .HasIndex(e => e.FileUuid)
            .HasDatabaseName("IDX_FileUuid")
            .IsUnique(false);

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

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                var nonPublicProps = clrType
                    .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(p => p.GetCustomAttributes(typeof(ColumnAttribute), inherit: true).Any());

                var mappedProps = entityType.GetProperties().Select(p => p.Name).ToHashSet();

                foreach (var prop in nonPublicProps)
                {
                    modelBuilder.Entity(clrType).Property(prop.PropertyType, prop.Name);
                }
            }
        }
    }
}