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
        public DbSet<EquipmentRequest> EquipmentRequests { get; set; }
        public DbSet<ConditionLog> ConditionLogs { get; set; }
        public DbSet<InventoryDocument> Documents { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ensure Identity entities are configured
            base.OnModelCreating(modelBuilder);
            // Explicitly name the table (optional)
            modelBuilder.Entity<Equipment>().ToTable("Equipment");
            
            // Keep Equipment enums as integers to match existing schema (INTEGER columns)
            // If you prefer strings, create a migration to convert column types to TEXT and move values.

            modelBuilder.Entity<EquipmentRequest>()
                .Property(r => r.Status)
                .HasConversion<string>();

            // Relationships for EquipmentRequest
            modelBuilder.Entity<EquipmentRequest>()
                .HasOne(r => r.Requester)
                .WithMany(u => u.Requests)
                .HasForeignKey(r => r.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EquipmentRequest>()
                .HasOne(r => r.ApprovedBy)
                .WithMany(u => u.ApprovedRequests)
                .HasForeignKey(r => r.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EquipmentRequest>()
                .HasOne(r => r.Equipment)
                .WithMany()
                .HasForeignKey(r => r.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Documents: uploaded by a user; visibility by role string
            modelBuilder.Entity<InventoryDocument>()
                .HasOne(d => d.UploadedBy)
                .WithMany()
                .HasForeignKey(d => d.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Activity logs (optional user/equipment FKs)
            modelBuilder.Entity<ActivityLog>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ActivityLog>()
                .HasOne(l => l.Equipment)
                .WithMany()
                .HasForeignKey(l => l.EquipmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Helpful indexes
            modelBuilder.Entity<EquipmentRequest>()
                .HasIndex(r => r.RequesterId);
            modelBuilder.Entity<EquipmentRequest>()
                .HasIndex(r => r.ApprovedById);
            modelBuilder.Entity<InventoryDocument>()
                .HasIndex(d => d.VisibilityRole);
        }
    }
}
