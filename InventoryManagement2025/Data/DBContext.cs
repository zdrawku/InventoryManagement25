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
            
            // Store enums as strings for readability and compatibility with JSON serialization
            modelBuilder.Entity<Equipment>()
                .Property(e => e.Condition)
                .HasConversion<string>();

            modelBuilder.Entity<Equipment>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<InventoryManagement2025.Models.EquipmentRequest>()
                .Property(r => r.Status)
                .HasConversion<string>();
        }
    }
}
