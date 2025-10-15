using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Models;
using InventoryManagement2025.Data;

namespace InventoryManagement2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class EquipmentController : ControllerBase
    {
        private readonly SchoolInventory _context;

        public EquipmentController(SchoolInventory context)
        {
            _context = context;
        }

        // GET: api/Equipment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetEquipments()
        {
            return await _context.Equipment.AsNoTracking().ToListAsync();
        }

        // GET: api/Equipment/search?name=...&type=...&status=Available
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Equipment>>> Search([FromQuery] string? name, [FromQuery] string? type, [FromQuery] EquipmentStatus? status, [FromQuery] Condition? condition)
        {
            var q = _context.Equipment.AsQueryable();
            if (!string.IsNullOrWhiteSpace(name)) q = q.Where(e => e.Name.Contains(name));
            if (!string.IsNullOrWhiteSpace(type)) q = q.Where(e => e.Type.Contains(type));
            if (status != null) q = q.Where(e => e.Status == status);
            if (condition != null) q = q.Where(e => e.Condition == condition);

            return await q.AsNoTracking().ToListAsync();
        }

        // GET: api/Equipment/export/csv
        [HttpGet("export/csv")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCsv()
        {
            var items = await _context.Equipment.AsNoTracking().ToListAsync();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("EquipmentId,Name,Type,SerialNumber,Condition,Status,Location,IsSensitive");
            foreach (var e in items)
            {
                csv.AppendLine($"{e.EquipmentId},\"{e.Name}\",\"{e.Type}\",\"{e.SerialNumber}\",{e.Condition},{e.Status},\"{e.Location}\",{e.IsSensitive}");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "equipment.csv");
        }

        // GET: api/Equipment/export/requests/csv
        [HttpGet("export/requests/csv")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportRequestsCsv()
        {
            var reqs = await _context.EquipmentRequests.AsNoTracking().ToListAsync();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Id,EquipmentId,RequesterId,RequestedAt,Start,End,Status,ApprovedById,ApprovedAt,ReturnedAt,ReturnNotes");
            foreach (var r in reqs)
            {
                csv.AppendLine($"{r.Id},{r.EquipmentId},\"{r.RequesterId}\",{r.RequestedAt:o},{r.Start:o},{r.End:o},{r.Status},\"{r.ApprovedById}\",{r.ApprovedAt:o},{r.ReturnedAt:o},\"{r.ReturnNotes}\"");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "requests.csv");
        }

        // GET: api/Equipment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipment>> GetEquipment(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);

            if (equipment == null)
                return NotFound();

            return equipment;
        }

        // PUT: api/Equipment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEquipment(int id, [FromBody] Equipment equipment)
        {
            if (id != equipment.EquipmentId)
                return BadRequest("Equipment ID mismatch.");

            _context.Entry(equipment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipmentExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Equipment
        [HttpPost]
        public async Task<ActionResult<Equipment>> PostEquipment([FromBody] Equipment equipment)
        {
            // Prevent duplicates if ID manually provided
            if (equipment.EquipmentId != 0 && EquipmentExists(equipment.EquipmentId))
            {
                return Conflict("An equipment with this ID already exists.");
            }
            equipment.EquipmentId = 0;
            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEquipment), new { id = equipment.EquipmentId }, equipment);
        }

        // DELETE: api/Equipment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
                return NotFound();

            _context.Equipment.Remove(equipment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipment.Any(e => e.EquipmentId == id);
        }
    }
}
