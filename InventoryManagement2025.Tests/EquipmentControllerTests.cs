using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;
using InventoryManagement2025.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement2025.Tests
{
    public class EquipmentControllerTests
    {
        private SchoolInventory CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SchoolInventory>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new SchoolInventory(options);
        }

    [Fact]
    public async System.Threading.Tasks.Task ReturnFlow_CreatesConditionLog()
        {
            var ctx = CreateInMemoryContext("returnflow_db");
            // Seed equipment
            var eq = new Equipment { Name = "Test", Type = "Laptop", SerialNumber = "S1", Condition = Condition.Good, Status = EquipmentStatus.Available };
            ctx.Equipment.Add(eq);
            await ctx.SaveChangesAsync();

            // Seed request (approved)
            var req = new EquipmentRequest { EquipmentId = eq.EquipmentId, RequesterId = "user1", RequestedAt = DateTime.UtcNow, Start = DateTime.UtcNow, End = DateTime.UtcNow.AddDays(1), Status = RequestStatus.Approved, ApprovedAt = DateTime.UtcNow, ApprovedById = "admin" };
            ctx.EquipmentRequests.Add(req);
            await ctx.SaveChangesAsync();

            var controller = new EquipmentRequestsController(ctx);

            // simulate admin return
            var dto = new ReturnRequest { Condition = Condition.Fair, Status = EquipmentStatus.Available, Notes = "Returned with minor wear" };
            var result = await controller.Return(req.Id, dto);

            // verify
            Assert.IsType<NoContentResult>(result);
            var logs = ctx.ConditionLogs.Where(l => l.EquipmentId == eq.EquipmentId).ToList();
            Assert.NotEmpty(logs);
            var updatedReq = ctx.EquipmentRequests.Find(req.Id);
            Assert.NotNull(updatedReq.ReturnedAt);
            Assert.Equal("Returned with minor wear", updatedReq.ReturnNotes);
        }

    [Fact]
    public async System.Threading.Tasks.Task ExportCsv_ReturnsNonEmptyContent()
        {
            var ctx = CreateInMemoryContext("exportcsv_db");
            var eq = new Equipment { Name = "CSVTest", Type = "Monitor", SerialNumber = "M1", Condition = Condition.Excellent, Status = EquipmentStatus.Available };
            ctx.Equipment.Add(eq);
            await ctx.SaveChangesAsync();

            var equipmentController = new EquipmentController(ctx);
            var fileResult = await equipmentController.ExportCsv() as FileContentResult;
            Assert.NotNull(fileResult);
            Assert.True(fileResult.FileContents.Length > 0);

            // seed request and test requests csv
            var req = new EquipmentRequest { EquipmentId = eq.EquipmentId, RequesterId = "userA", RequestedAt = DateTime.UtcNow, Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(2), Status = RequestStatus.Approved };
            ctx.EquipmentRequests.Add(req);
            await ctx.SaveChangesAsync();

            var reqFile = await equipmentController.ExportRequestsCsv() as FileContentResult;
            Assert.NotNull(reqFile);
            Assert.True(reqFile.FileContents.Length > 0);
        }
    }
}
