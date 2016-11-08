using System.Data.Entity;

namespace Kartverket.Geonorge.Download.Models
{
    public class DownloadContext : DbContext
    {
        public DownloadContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Dataset> Capabilities { get; set; }
        public DbSet<filliste> FileList { get; set; }
        public DbSet<Order> OrderDownloads { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.orderItem)
                .WithRequired(item => item.Order)
                .HasForeignKey(item => item.ReferenceNumber)
                .WillCascadeOnDelete(true);
        }
    }
}