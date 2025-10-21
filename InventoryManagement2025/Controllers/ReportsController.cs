using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;
using System.Text;

namespace InventoryManagement2025.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Route("reports")]
    public class ReportsController : ControllerBase
    {
        private readonly SchoolInventory _context;
        public ReportsController(SchoolInventory context)
        {
            _context = context;
        }

        // GET: /reports/usage
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

        // GET: /reports/history
        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            var recentLogs = await _context.ActivityLogs.AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .Take(50)
                .ToListAsync();
            return Ok(recentLogs);
        }

        // GET: /reports/export
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
