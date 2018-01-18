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
        public virtual DbSet<filliste> FileList { get; set; }
        public virtual DbSet<Order> OrderDownloads { get; set; }

        public virtual DbSet<OrderItem> OrderItems { get; set; }

        public virtual DbSet<MachineAccount> MachineAccounts { get; set; }

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