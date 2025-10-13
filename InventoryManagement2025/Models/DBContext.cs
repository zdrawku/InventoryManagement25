using Microsoft.EntityFrameworkCore;
namespace InventoryManagement2025.Models

{
    public class SchoolInventory : DbContext
    {
        public SchoolInventory(DbContextOptions<SchoolInventory> options) : base(options)
        {
        }

        public DbSet<Equipment> Equipments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Equipment>().ToTable("Equipment");
            
        }
    }
}
