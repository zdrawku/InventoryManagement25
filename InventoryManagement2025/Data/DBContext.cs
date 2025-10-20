using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using InventoryManagement2025.Models;

namespace InventoryManagement2025.Data
{
    public class SchoolInventory : IdentityDbContext<AppUser>
    {
        public SchoolInventory(DbContextOptions<SchoolInventory> options)
            : base(options)
        {
        }

        public DbSet<Equipment> Equipment { get; set; }
    public DbSet<InventoryManagement2025.Models.EquipmentRequest> EquipmentRequests { get; set; }
    public DbSet<InventoryManagement2025.Models.ConditionLog> ConditionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ensure Identity entities are configured
            base.OnModelCreating(modelBuilder);
            // Explicitly name the table (optional)
            modelBuilder.Entity<Equipment>().ToTable("Equipment");
            
            // Keep Equipment enums as integers to match existing schema (INTEGER columns)
            // If you prefer strings, create a migration to convert column types to TEXT and move values.

            modelBuilder.Entity<InventoryManagement2025.Models.EquipmentRequest>()
                .Property(r => r.Status)
                .HasConversion<string>();

            // Relationships for EquipmentRequest
            modelBuilder.Entity<InventoryManagement2025.Models.EquipmentRequest>()
                .HasOne(r => r.Requester)
                .WithMany(u => u.Requests)
                .HasForeignKey(r => r.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryManagement2025.Models.EquipmentRequest>()
                .HasOne(r => r.ApprovedBy)
                .WithMany(u => u.ApprovedRequests)
                .HasForeignKey(r => r.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryManagement2025.Models.EquipmentRequest>()
                .HasOne(r => r.Equipment)
                .WithMany()
                .HasForeignKey(r => r.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
