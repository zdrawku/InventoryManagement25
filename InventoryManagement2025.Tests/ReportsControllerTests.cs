using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Xunit;
using InventoryManagement2025.Controllers;
using InventoryManagement2025.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace InventoryManagement2025.Tests
{
    /// <summary>
    /// Tests for reporting functionality
    /// </summary>
    public class ReportsControllerTests : TestBase
    {
        private readonly ReportsController _controller;

        public ReportsControllerTests() : base()
        {
            _controller = new ReportsController(Context);
            SetupAdminUser();
        }

        private void SetupAdminUser()
        {
            var claims = new List<Claim>
            {
                new Claim("id", "admin-id"),
                new Claim(ClaimTypes.Email, "admin@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task Usage_ReturnsCorrectUsageStatistics()
        {
            // Act
            var result = await _controller.Usage();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var usageStats = okResult.Value;
            Assert.NotNull(usageStats);

            // Verify the structure contains expected properties
            var usageType = usageStats.GetType();
            Assert.NotNull(usageType.GetProperty("total"));
            Assert.NotNull(usageType.GetProperty("available"));
            Assert.NotNull(usageType.GetProperty("checkedOut"));
            Assert.NotNull(usageType.GetProperty("pending"));
            Assert.NotNull(usageType.GetProperty("approved"));

            // Verify reasonable values based on seeded data
            var totalProperty = usageType.GetProperty("total");
            var total = (int)totalProperty.GetValue(usageStats);
            Assert.True(total >= 3); // At least the seeded equipment

            var availableProperty = usageType.GetProperty("available");
            var available = (int)availableProperty.GetValue(usageStats);
            Assert.True(available >= 0);

            var checkedOutProperty = usageType.GetProperty("checkedOut");
            var checkedOut = (int)checkedOutProperty.GetValue(usageStats);
            Assert.True(checkedOut >= 0);

            var pendingProperty = usageType.GetProperty("pending");
            var pending = (int)pendingProperty.GetValue(usageStats);
            Assert.True(pending >= 0);

            var approvedProperty = usageType.GetProperty("approved");
            var approved = (int)approvedProperty.GetValue(usageStats);
            Assert.True(approved >= 0);
        }

        [Fact]
        public async Task History_ReturnsActivityLogs()
        {
            // Arrange - Add some activity logs
            var activityLogs = new List<ActivityLog>
            {
                new ActivityLog
                {
                    UserId = "user-id",
                    EquipmentId = 1,
                    Action = "request:create",
                    Timestamp = DateTime.UtcNow.AddMinutes(-30),
                    Notes = "Test activity 1"
                },
                new ActivityLog
                {
                    UserId = "admin-id",
                    EquipmentId = 1,
                    Action = "request:approve",
                    Timestamp = DateTime.UtcNow.AddMinutes(-15),
                    Notes = "Test activity 2"
                },
                new ActivityLog
                {
                    UserId = "admin-id",
                    EquipmentId = 2,
                    Action = "equipment:update",
                    Timestamp = DateTime.UtcNow.AddMinutes(-5),
                    Notes = "Test activity 3"
                }
            };
            Context.ActivityLogs.AddRange(activityLogs);
            await Context.SaveChangesAsync();

            // Act
            var result = await _controller.History();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var logs = okResult.Value as List<ActivityLog>;
            Assert.NotNull(logs);
            Assert.True(logs.Count >= 3); // At least the added logs
            
            // Verify logs are ordered by timestamp (most recent first)
            for (int i = 0; i < logs.Count - 1; i++)
            {
                Assert.True(logs[i].Timestamp >= logs[i + 1].Timestamp);
            }

            // Verify we don't get more than 50 logs
            Assert.True(logs.Count <= 50);
        }

        [Fact]
        public async Task History_WithManyLogs_ReturnsMaximum50()
        {
            // Arrange - Add more than 50 activity logs
            var activityLogs = new List<ActivityLog>();
            for (int i = 0; i < 60; i++)
            {
                activityLogs.Add(new ActivityLog
                {
                    UserId = "user-id",
                    EquipmentId = 1,
                    Action = "test:action",
                    Timestamp = DateTime.UtcNow.AddMinutes(-i),
                    Notes = $"Test activity {i}"
                });
            }
            Context.ActivityLogs.AddRange(activityLogs);
            await Context.SaveChangesAsync();

            // Act
            var result = await _controller.History();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var logs = okResult.Value as List<ActivityLog>;
            Assert.NotNull(logs);
            Assert.Equal(50, logs.Count); // Should be limited to 50
        }

        [Fact]
        public async Task Export_ReturnsEquipmentCsvFile()
        {
            // Act
            var result = await _controller.Export();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.True(fileResult.FileContents.Length > 0);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal("equipment.csv", fileResult.FileDownloadName);

            // Verify CSV content structure
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            Assert.Contains("EquipmentId,Name,Type,SerialNumber,Condition,Status,Location,IsSensitive", csvContent);
            
            // Should contain at least the seeded equipment data
            Assert.Contains("Test Laptop", csvContent);
            Assert.Contains("Sensitive Equipment", csvContent);
            Assert.Contains("Checked Out Item", csvContent);
        }

        [Fact]
        public async Task Export_EmptyDatabase_ReturnsHeaderOnly()
        {
            // Arrange - Clear equipment data
            Context.Equipment.RemoveRange(Context.Equipment);
            await Context.SaveChangesAsync();

            // Act
            var result = await _controller.Export();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.True(fileResult.FileContents.Length > 0);
            
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            // Should only have header line
            Assert.Single(lines);
            Assert.Contains("EquipmentId,Name,Type,SerialNumber,Condition,Status,Location,IsSensitive", lines[0]);
        }

        [Fact]
        public async Task Export_ValidatesFileFormat()
        {
            // Act
            var result = await _controller.Export();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Verify header format
            var expectedHeader = "EquipmentId,Name,Type,SerialNumber,Condition,Status,Location,IsSensitive";
            Assert.Equal(expectedHeader, lines[0]);
            
            // Verify data lines have proper structure (8 columns)
            if (lines.Length > 1)
            {
                foreach (var line in lines.Skip(1))
                {
                    var columns = line.Split(',');
                    Assert.True(columns.Length >= 8, $"Line should have at least 8 columns: {line}");
                }
            }
        }
    }
}