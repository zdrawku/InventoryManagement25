using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;
using System.Text;

namespace InventoryManagement2025.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [Route("reports")]
    public class ReportsController : ControllerBase
    {
        private readonly SchoolInventory _context;
        public ReportsController(SchoolInventory context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates equipment usage statistics including total equipment count and status breakdown.
        /// </summary>
        /// <returns>Usage statistics with counts for total equipment, available, checked out, pending requests, and approved requests.</returns>
        /// <response code="200">Returns the usage statistics report</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpGet("usage")]
        public async Task<IActionResult> Usage()
        {
            var total = await _context.Equipment.CountAsync();
            var available = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Available);
            var checkedOut = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.CheckedOut);
            var pending = await _context.EquipmentRequests.CountAsync(r => r.Status == RequestStatus.Pending);
            var approved = await _context.EquipmentRequests.CountAsync(r => r.Status == RequestStatus.Approved);
            return Ok(new { total, available, checkedOut, pending, approved });
        }

        /// <summary>
        /// Retrieves recent activity history showing equipment-related actions and events.
        /// </summary>
        /// <returns>The 50 most recent activity log entries ordered by timestamp (most recent first).</returns>
        /// <response code="200">Returns the activity history report</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            var recentLogs = await _context.ActivityLogs.AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .Take(50)
                .ToListAsync();
            return Ok(recentLogs);
        }

        /// <summary>
        /// Exports all equipment data to a CSV file for reporting and backup purposes.
        /// </summary>
        /// <returns>A CSV file containing comprehensive equipment data including all fields and status information.</returns>
        /// <response code="200">Returns the CSV file with complete equipment data</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Export()
        {
            // Simple combined export: Equipment CSV
            var items = await _context.Equipment.AsNoTracking().ToListAsync();
            var csv = new StringBuilder();
            csv.AppendLine("EquipmentId,Name,Type,SerialNumber,Condition,Status,Location,IsSensitive");
            foreach (var e in items)
            {
                csv.AppendLine($"{e.EquipmentId},\"{e.Name}\",\"{e.Type}\",\"{e.SerialNumber}\",{e.Condition},{e.Status},\"{e.Location}\",{e.IsSensitive}");
            }
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "equipment.csv");
        }
    }
}
