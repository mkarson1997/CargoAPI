using CargoAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CargoAPI.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Carrier> Carriers { get; set; } = null!;
        public DbSet<CarrierConfiguration> CarrierConfigurations { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<CarrierReport> CarrierReports { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Carrier
            modelBuilder.Entity<Carrier>(entity =>
            {
                entity.HasKey(c => c.CarrierId);
                entity.Property(c => c.CarrierName).IsRequired();
                entity.Property(c => c.CarrierIsActive).IsRequired();
                entity.Property(c => c.CarrierPlusDesiCost).IsRequired();
                entity.Property(c => c.CarrierConfigurationId).IsRequired();

                entity.HasMany(c => c.CarrierConfigurations)
                      .WithOne(cc => cc.Carrier)
                      .HasForeignKey(cc => cc.CarrierId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CarrierConfiguration
            modelBuilder.Entity<CarrierConfiguration>(entity =>
            {
                entity.HasKey(cc => cc.CarrierConfigurationId);
                entity.Property(cc => cc.CarrierId).IsRequired();
                entity.Property(cc => cc.CarrierMaxDesi).IsRequired();
                entity.Property(cc => cc.CarrierMinDesi).IsRequired();
                entity.Property(cc => cc.CarrierCost).HasColumnType("decimal(18,2)").IsRequired();
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.OrderDesi).IsRequired();
                entity.Property(o => o.OrderDate).IsRequired();
                entity.Property(o => o.OrderCarrierCost).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(o => o.CarrierId).IsRequired();

                entity.HasOne(o => o.Carrier)
                      .WithMany()
                      .HasForeignKey(o => o.CarrierId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CarrierReport
            modelBuilder.Entity<CarrierReport>(entity =>
            {
                entity.HasKey(cr => cr.CarrierReportId);
                entity.Property(cr => cr.CarrierId).IsRequired();
                entity.Property(cr => cr.CarrierCost).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(cr => cr.CarrierReportDate).IsRequired();

                // Unique index to prevent duplicate reports for same carrier and date
                entity.HasIndex(cr => new { cr.CarrierId, cr.CarrierReportDate }).IsUnique();
            });
        }
    }
}
