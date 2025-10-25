using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace InventoryManagement2025.Data
{
    public class SchoolInventoryDesignTimeFactory : IDesignTimeDbContextFactory<SchoolInventory>
    {
        public SchoolInventory CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            var optionsBuilder = new DbContextOptionsBuilder<SchoolInventory>();
            var conn = config.GetConnectionString("DefaultConnection") ?? "Data Source=school_inventory.db;Cache=Shared";
            optionsBuilder.UseSqlite(conn);

            return new SchoolInventory(optionsBuilder.Options);
        }
    }
}
