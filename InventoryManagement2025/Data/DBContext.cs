using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Models;

namespace InventoryManagement2025.Data
{
    public class SchoolInventory : DbContext
    {
        public SchoolInventory(DbContextOptions<SchoolInventory> options)
            : base(options)
        {
        }

        public DbSet<Equipment> Equipment { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Explicitly name the table (optional)
            modelBuilder.Entity<Equipment>().ToTable("Equipment");
        }
    }
}
