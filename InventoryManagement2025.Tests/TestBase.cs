using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;
using System;
using System.Collections.Generic;

namespace InventoryManagement2025.Tests
{
    /// <summary>
    /// Base class for tests providing common setup and utilities
    /// </summary>
    public class TestBase : IDisposable
    {
        protected SchoolInventory Context { get; private set; }
        protected UserManager<AppUser> UserManager { get; private set; }
        protected RoleManager<IdentityRole> RoleManager { get; private set; }
        protected ILogger<TestBase> Logger { get; private set; }
        protected IConfiguration Configuration { get; private set; }

        public TestBase()
        {
            // Create in-memory database
            var options = new DbContextOptionsBuilder<SchoolInventory>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Context = new SchoolInventory(options);

            // Setup services
            var services = new ServiceCollection();
            
            // Add Identity services
            services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SchoolInventory>();

            // Add logging
            services.AddLogging();

            // Add configuration
            var configDict = new Dictionary<string, string>
            {
                {"JwtSettings:Key", "TestSigningKey_MUST_BE_AtLeast32Chars_Long_For_Testing_123456!"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict!)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Add DbContext
            services.AddSingleton(Context);

            var serviceProvider = services.BuildServiceProvider();

            UserManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            Logger = serviceProvider.GetRequiredService<ILogger<TestBase>>();
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Ensure database is created
            Context.Database.EnsureCreated();
            SeedTestData();
        }

        protected virtual void SeedTestData()
        {
            // Create roles
            if (!RoleManager.RoleExistsAsync("Admin").Result)
            {
                RoleManager.CreateAsync(new IdentityRole("Admin")).Wait();
            }
            if (!RoleManager.RoleExistsAsync("User").Result)
            {
                RoleManager.CreateAsync(new IdentityRole("User")).Wait();
            }

            // Create test users
            var adminUser = new AppUser
            {
                Id = "admin-id",
                UserName = "admin@test.com",
                Email = "admin@test.com",
                Name = "Test Admin",
                EmailConfirmed = true
            };

            var regularUser = new AppUser
            {
                Id = "user-id",
                UserName = "user@test.com",
                Email = "user@test.com",
                Name = "Test User",
                EmailConfirmed = true
            };

            // Add users if they don't exist
            if (UserManager.FindByEmailAsync(adminUser.Email).Result == null)
            {
                UserManager.CreateAsync(adminUser, "Admin123!").Wait();
                UserManager.AddToRoleAsync(adminUser, "Admin").Wait();
            }

            if (UserManager.FindByEmailAsync(regularUser.Email).Result == null)
            {
                UserManager.CreateAsync(regularUser, "User123!").Wait();
                UserManager.AddToRoleAsync(regularUser, "User").Wait();
            }

            // Create test equipment
            var equipment = new List<Equipment>
            {
                new Equipment
                {
                    EquipmentId = 1,
                    Name = "Test Laptop",
                    Type = "Laptop",
                    SerialNumber = "TL001",
                    Condition = Condition.Excellent,
                    Status = EquipmentStatus.Available,
                    Location = "Room 101",
                    IsSensitive = false
                },
                new Equipment
                {
                    EquipmentId = 2,
                    Name = "Sensitive Equipment",
                    Type = "Server",
                    SerialNumber = "SE001",
                    Condition = Condition.Good,
                    Status = EquipmentStatus.Available,
                    Location = "Server Room",
                    IsSensitive = true
                },
                new Equipment
                {
                    EquipmentId = 3,
                    Name = "Checked Out Item",
                    Type = "Tablet",
                    SerialNumber = "CO001",
                    Condition = Condition.Fair,
                    Status = EquipmentStatus.CheckedOut,
                    Location = "Room 202",
                    IsSensitive = false
                }
            };

            Context.Equipment.AddRange(equipment);

            // Create test documents
            var documents = new List<InventoryDocument>
            {
                new InventoryDocument
                {
                    Id = 1,
                    Title = "Test Document 1",
                    Path = "/documents/test1.pdf",
                    UploadedById = "admin-id",
                    UploadedAt = DateTime.UtcNow.AddDays(-1),
                    VisibilityRole = "User"
                },
                new InventoryDocument
                {
                    Id = 2,
                    Title = "Admin Only Document",
                    Path = "/documents/admin.pdf",
                    UploadedById = "admin-id",
                    UploadedAt = DateTime.UtcNow,
                    VisibilityRole = "Admin"
                }
            };

            Context.Documents.AddRange(documents);

            // Create test equipment requests
            var requests = new List<EquipmentRequest>
            {
                new EquipmentRequest
                {
                    Id = 1,
                    EquipmentId = 1,
                    RequesterId = "user-id",
                    RequestedAt = DateTime.UtcNow.AddDays(-2),
                    Start = DateTime.UtcNow.AddDays(-1),
                    End = DateTime.UtcNow.AddDays(1),
                    Status = RequestStatus.Approved,
                    ApprovedById = "admin-id",
                    ApprovedAt = DateTime.UtcNow.AddDays(-1),
                    Notes = "Test request"
                },
                new EquipmentRequest
                {
                    Id = 2,
                    EquipmentId = 2,
                    RequesterId = "user-id",
                    RequestedAt = DateTime.UtcNow,
                    Start = DateTime.UtcNow.AddDays(1),
                    End = DateTime.UtcNow.AddDays(3),
                    Status = RequestStatus.Pending,
                    Notes = "Pending sensitive equipment request"
                }
            };

            Context.EquipmentRequests.AddRange(requests);

            Context.SaveChanges();
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}