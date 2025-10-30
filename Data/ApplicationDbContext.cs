using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HarvestHub.WebApp.Models;


namespace Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Farmer> Farmers { get; set; }
        public DbSet<Crop> Crops { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<VerificationDocument> VerificationDocument { get; set; }
        public DbSet<Laboratory> Laboratories { get; set; }
        public DbSet<LabReport> LabReports { get; set; }
        public DbSet<CropImage> CropImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
       
        public DbSet<MarketRate> MarketRates { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Order → Crop: disable cascade delete
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Crop)
                .WithMany()
                .HasForeignKey(o => o.CropId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Order → Buyer: can keep cascade or restrict as you wish
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Crop → Farmer: disable cascade delete to avoid chain
            modelBuilder.Entity<Crop>()
                .HasOne(c => c.Farmer)
                .WithMany()
                .HasForeignKey(c => c.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ LabReport → Farmer: restrict cascade to avoid cycles
            modelBuilder.Entity<LabReport>()
                .HasOne(r => r.Farmer)
                .WithMany()
                .HasForeignKey(r => r.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ LabReport → Laboratory
            modelBuilder.Entity<LabReport>()
                .HasOne(r => r.Laboratory)
                .WithMany()
                .HasForeignKey(r => r.LaboratoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }


}
