using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FleetCarePro.Models;

namespace FleetCarePro.Data
{
    public class FleetContext : IdentityDbContext<ApplicationUser>
    {
        public FleetContext(DbContextOptions<FleetContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ServiceCenter> ServiceCenters { get; set; }
        public DbSet<ServiceRecord> ServiceRecords { get; set; }
        public DbSet<ServiceLineItem> ServiceLineItems { get; set; }
        public DbSet<VendorService> VendorServices { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Vehicle -> Driver (1-to-N)
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Driver)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            // ServiceRecord -> Vehicle (1-to-N)
            modelBuilder.Entity<ServiceRecord>()
                .HasOne(sr => sr.Vehicle)
                .WithMany(v => v.ServiceRecords)
                .HasForeignKey(sr => sr.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRecord -> ServiceCenter (1-to-N)
            modelBuilder.Entity<ServiceRecord>()
                .HasOne(sr => sr.ServiceCenter)
                .WithMany(sc => sc.ServiceRecords)
                .HasForeignKey(sr => sr.ServiceCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRecord -> CreatedByUser
            modelBuilder.Entity<ServiceRecord>()
                .HasOne(sr => sr.CreatedByUser)
                .WithMany()
                .HasForeignKey(sr => sr.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRecord -> ServiceLineItems (Master-Detail)
            modelBuilder.Entity<ServiceLineItem>()
                .HasOne(sli => sli.ServiceRecord)
                .WithMany(sr => sr.ServiceLineItems)
                .HasForeignKey(sli => sli.ServiceRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceLineItem -> ServiceCategory
            modelBuilder.Entity<ServiceLineItem>()
                .HasOne(sli => sli.ServiceCategory)
                .WithMany(sc => sc.ServiceLineItems)
                .HasForeignKey(sli => sli.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceCenter <-> ServiceCategory (N-to-N)
            modelBuilder.Entity<VendorService>()
                .HasKey(vs => new
                {
                    vs.ServiceCenterId,
                    vs.ServiceCategoryId
                });

            modelBuilder.Entity<VendorService>()
                .HasOne(vs => vs.ServiceCenter)
                .WithMany(sc => sc.VendorServices)
                .HasForeignKey(vs => vs.ServiceCenterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VendorService>()
                .HasOne(vs => vs.ServiceCategory)
                .WithMany(sc => sc.VendorServices)
                .HasForeignKey(vs => vs.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique VIN
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.VIN)
                .IsUnique();

            // Decimal precision
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.PurchasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ServiceRecord>()
                .Property(sr => sr.TotalCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ServiceLineItem>()
                .Property(sli => sli.Cost)
                .HasColumnType("decimal(18,2)");
        }
    }
}