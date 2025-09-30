using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Xml;

namespace Geonorge.Download.Models
{
    public class DownloadContext : DbContext
    {
        public DownloadContext(DbContextOptions<DownloadContext> options) : base(options) { }

        public DbSet<Dataset> Capabilities { get; set; }
        public DbSet<File> FileList { get; set; }
        public DbSet<Order> OrderDownloads { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<MachineAccount> MachineAccounts { get; set; }
        
        public DbSet<DownloadUsageEntry> DownloadUsages { get; set; }
        public DbSet<ClipperFile> ClipperFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UseCollation("Danish_Norwegian_CI_AS");            

            modelBuilder.Entity<Order>()
                .HasMany(o => o.orderItem)
                .WithOne(item => item.Order)
                .HasForeignKey(item => item.ReferenceNumber)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .Property(o => o.referenceNumber)
                .UseIdentityColumn(seed: 1000, increment: 1);

            modelBuilder.Entity<Order>()
                .Property(o => o.orderDate)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Order>()
                .Property(o => o.Uuid)
                .HasDefaultValueSql("CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)");

            modelBuilder.Entity<Order>()
            .HasIndex(e => e.Uuid)
            .HasDatabaseName("IDX_Uuid")
            //TODO: postgres: .HasDatabaseName("IDX_OrderUuid")
            .IsUnique(false);

            modelBuilder.Entity<OrderItem>()
            .HasIndex(e => e.Uuid)
            .HasDatabaseName("IDX_Uuid")
            //TODO: postgres: .HasDatabaseName("IDX_OrderItemUuid")
            .IsUnique(false);

            modelBuilder.Entity<OrderItem>()
            .HasIndex(e => e.FileUuid)
            .HasDatabaseName("IDX_FileUuid")
            .IsUnique(false);

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.Uuid)
                .HasDefaultValueSql("CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)");

            modelBuilder.Entity<Dataset>()
            .HasIndex(d => d.MetadataUuid);

            modelBuilder.Entity<Dataset>()
                .Property(o => o.MaxArea)
                .HasDefaultValueSql("0");

            modelBuilder.Entity<File>()
            .HasIndex(f => f.Division);

            modelBuilder.Entity<File>()
            .HasIndex(f => f.DivisionKey);

            modelBuilder.Entity<File>()
            .HasIndex(f => f.Projection);

            modelBuilder.Entity<File>()
            .HasIndex(f => f.Format);

            modelBuilder.Entity<DownloadUsageEntry>()
                .Property(o => o.RequestId)
                .HasDefaultValueSql("CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)");

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                var nonPublicProps = clrType
                    .GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic)
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