using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;
using InventoryManagement2025.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InventoryManagement2025.Tests
{
    /// <summary>
    /// Tests for equipment management functionality
    /// </summary>
    public class EquipmentControllerTests : TestBase
    {
        private readonly EquipmentController _controller;

        public EquipmentControllerTests() : base()
        {
            _controller = new EquipmentController(Context);
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
        public async Task GetEquipments_ReturnsAllEquipment()
        {
            // Act
            var result = await _controller.GetEquipments();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Equipment>>>(result);
            var equipmentList = actionResult.Value as List<Equipment>;
            Assert.NotNull(equipmentList);
            Assert.True(equipmentList.Count >= 3); // At least the seeded equipment
        }

        [Fact]
        public async Task GetEquipment_ExistingId_ReturnsEquipment()
        {
            // Act
            var result = await _controller.GetEquipment(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Equipment>>(result);
            var equipment = actionResult.Value;
            Assert.NotNull(equipment);
            Assert.Equal("Test Laptop", equipment.Name);
        }

        [Fact]
        public async Task GetEquipment_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetEquipment(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostEquipment_ValidEquipment_CreatesEquipment()
        {
            // Arrange
            var newEquipment = new Equipment
            {
                Name = "New Test Equipment",
                Type = "Monitor",
                SerialNumber = "NT001",
                Condition = Condition.Excellent,
                Status = EquipmentStatus.Available,
                Location = "Room 301",
                IsSensitive = false
            };

            // Act
            var result = await _controller.PostEquipment(newEquipment);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Equipment>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdEquipment = Assert.IsType<Equipment>(createdAtActionResult.Value);
            
            Assert.Equal(newEquipment.Name, createdEquipment.Name);
            Assert.True(createdEquipment.EquipmentId > 0);

            // Verify in database
            var equipmentInDb = await Context.Equipment.FindAsync(createdEquipment.EquipmentId);
            Assert.NotNull(equipmentInDb);
            Assert.Equal(newEquipment.Name, equipmentInDb.Name);
        }

        [Fact]
        public async Task PutEquipment_ValidUpdate_UpdatesEquipment()
        {
            // Arrange
            var existingEquipment = await Context.Equipment.FindAsync(1);
            existingEquipment.Name = "Updated Laptop";
            existingEquipment.Location = "Updated Room";

            // Act
            var result = await _controller.PutEquipment(1, existingEquipment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedEquipment = Assert.IsType<Equipment>(okResult.Value);
            Assert.Equal("Updated Laptop", updatedEquipment.Name);
            Assert.Equal("Updated Room", updatedEquipment.Location);

            // Verify in database
            var equipmentInDb = await Context.Equipment.FindAsync(1);
            Assert.Equal("Updated Laptop", equipmentInDb.Name);
        }

        [Fact]
        public async Task PutEquipment_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var equipment = new Equipment { EquipmentId = 2, Name = "Test" };

            // Act
            var result = await _controller.PutEquipment(1, equipment);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Equipment ID mismatch.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteEquipment_ExistingId_DeletesEquipment()
        {
            // Arrange
            var equipmentToDelete = new Equipment
            {
                Name = "To Delete",
                Type = "Test",
                SerialNumber = "DEL001",
                Condition = Condition.Good,
                Status = EquipmentStatus.Available,
                Location = "Test Room"
            };
            Context.Equipment.Add(equipmentToDelete);
            await Context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteEquipment(equipmentToDelete.EquipmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            // Verify deletion in database
            var deletedEquipment = await Context.Equipment.FindAsync(equipmentToDelete.EquipmentId);
            Assert.Null(deletedEquipment);
        }

        [Fact]
        public async Task DeleteEquipment_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteEquipment(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Search_ByName_ReturnsMatchingEquipment()
        {
            // Act
            var result = await _controller.Search(null, "Laptop", null, null, null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Equipment>>>(result);
            var equipmentList = actionResult.Value as List<Equipment>;
            Assert.NotNull(equipmentList);
            Assert.All(equipmentList, eq => Assert.Contains("Laptop", eq.Name, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Search_ByStatus_ReturnsMatchingEquipment()
        {
            // Act
            var result = await _controller.Search(null, null, null, EquipmentStatus.Available, null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Equipment>>>(result);
            var equipmentList = actionResult.Value as List<Equipment>;
            Assert.NotNull(equipmentList);
            Assert.All(equipmentList, eq => Assert.Equal(EquipmentStatus.Available, eq.Status));
        }

        [Fact]
        public async Task Search_ByCondition_ReturnsMatchingEquipment()
        {
            // Act
            var result = await _controller.Search(null, null, null, null, Condition.Excellent);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Equipment>>>(result);
            var equipmentList = actionResult.Value as List<Equipment>;
            Assert.NotNull(equipmentList);
            Assert.All(equipmentList, eq => Assert.Equal(Condition.Excellent, eq.Condition));
        }

        [Fact]
        public async Task Search_GeneralText_ReturnsMatchingEquipment()
        {
            // Act
            var result = await _controller.Search("Server", null, null, null, null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Equipment>>>(result);
            var equipmentList = actionResult.Value as List<Equipment>;
            Assert.NotNull(equipmentList);
            Assert.NotEmpty(equipmentList);
        }

        [Fact]
        public async Task UpdateStatus_ValidRequest_UpdatesStatus()
        {
            // Act
            var result = await _controller.UpdateStatus(1, EquipmentStatus.UnderRepair);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedEquipment = Assert.IsType<Equipment>(okResult.Value);
            Assert.Equal(EquipmentStatus.UnderRepair, updatedEquipment.Status);

            // Verify in database
            var equipmentInDb = await Context.Equipment.FindAsync(1);
            Assert.Equal(EquipmentStatus.UnderRepair, equipmentInDb.Status);
        }

        [Fact]
        public async Task UpdateStatus_NonExistingEquipment_ReturnsNotFound()
        {
            // Act
            var result = await _controller.UpdateStatus(999, EquipmentStatus.Available);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExportCsv_ReturnsNonEmptyContent()
        {
            // Act
            var result = await _controller.ExportCsv();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.True(fileResult.FileContents.Length > 0);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal("equipment.csv", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportRequestsCsv_ReturnsNonEmptyContent()
        {
            // Act
            var result = await _controller.ExportRequestsCsv();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.True(fileResult.FileContents.Length > 0);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal("requests.csv", fileResult.FileDownloadName);
        }
    }
}
