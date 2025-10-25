using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Models;
using InventoryManagement2025.Data;

namespace InventoryManagement2025.Controllers
{
    [Route("api/[controller]")]
    [Route("equipment")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class EquipmentController : ControllerBase
    {
        private readonly SchoolInventory _context;

        public EquipmentController(SchoolInventory context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all equipment items in the inventory.
        /// </summary>
        /// <returns>A list of all equipment items with their details including condition, status, and location.</returns>
        /// <response code="200">Returns the complete list of equipment</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetEquipments()
        {
            return await _context.Equipment.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Searches for equipment based on various criteria using flexible filtering.
        /// </summary>
        /// <param name="qtext">General search text that searches across name, type, serial number, and location fields.</param>
        /// <param name="name">Filter by equipment name (partial match).</param>
        /// <param name="type">Filter by equipment type (partial match).</param>
        /// <param name="status">Filter by equipment status (Available, Unavailable, UnderRepair).</param>
        /// <param name="condition">Filter by equipment condition (Excellent, Good, Fair, Damaged).</param>
        /// <returns>A filtered list of equipment matching the specified criteria.</returns>
        /// <response code="200">Returns the filtered list of equipment</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Equipment>>> Search(
            [FromQuery] string? qtext,
            [FromQuery] string? name,
            [FromQuery] string? type,
            [FromQuery] EquipmentStatus? status,
            [FromQuery] Condition? condition)
        {
            var q = _context.Equipment.AsQueryable();
            if (!string.IsNullOrWhiteSpace(qtext))
            {
                var pattern = $"%{qtext}%";
                q = q.Where(e => EF.Functions.Like(e.Name, pattern)
                              || EF.Functions.Like(e.Type, pattern)
                              || EF.Functions.Like(e.SerialNumber, pattern)
                              || EF.Functions.Like(e.Location, pattern));
            }
            if (!string.IsNullOrWhiteSpace(name)) q = q.Where(e => EF.Functions.Like(e.Name, $"%{name}%"));
            if (!string.IsNullOrWhiteSpace(type)) q = q.Where(e => EF.Functions.Like(e.Type, $"%{type}%"));
            if (status != null) q = q.Where(e => e.Status == status);
            if (condition != null) q = q.Where(e => e.Condition == condition);

            return await q.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Exports all equipment data to a CSV file. Admin access required.
        /// </summary>
        /// <returns>A CSV file containing all equipment data with headers.</returns>
        /// <response code="200">Returns the CSV file with equipment data</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
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

        /// <summary>
        /// Exports all equipment request data to a CSV file. Admin access required.
        /// </summary>
        /// <returns>A CSV file containing all equipment request data with complete request lifecycle information.</returns>
        /// <response code="200">Returns the CSV file with equipment request data</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
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

        /// <summary>
        /// Retrieves a specific equipment item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the equipment item.</param>
        /// <returns>The equipment item with all its details.</returns>
        /// <response code="200">Returns the requested equipment item</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the equipment with the specified ID is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipment>> GetEquipment(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);

            if (equipment == null)
                return NotFound();

            return equipment;
        }

        /// <summary>
        /// Updates an existing equipment item. Admin access required.
        /// </summary>
        /// <param name="id">The unique identifier of the equipment to update.</param>
        /// <param name="equipment">The updated equipment data. The EquipmentId must match the path parameter.</param>
        /// <returns>The updated equipment item.</returns>
        /// <response code="200">Returns the updated equipment item</response>
        /// <response code="400">If the equipment ID in the body doesn't match the path parameter</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        /// <response code="404">If the equipment with the specified ID is not found</response>
        [HttpPut("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
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

            // Return the updated entity for clarity instead of 204 No Content
            return Ok(equipment);
        }

        /// <summary>
        /// Creates a new equipment item in the inventory. Admin access required.
        /// </summary>
        /// <param name="equipment">The equipment data to create. The EquipmentId will be automatically assigned.</param>
        /// <returns>The created equipment item with assigned ID.</returns>
        /// <response code="201">Returns the newly created equipment item</response>
        /// <response code="400">If the equipment data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        /// <response code="409">If an equipment with the provided ID already exists</response>
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Deletes an equipment item from the inventory. Admin access required.
        /// </summary>
        /// <param name="id">The unique identifier of the equipment to delete.</param>
        /// <returns>A confirmation object containing the deleted equipment ID.</returns>
        /// <response code="200">Returns confirmation of successful deletion with the equipment ID</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        /// <response code="404">If the equipment with the specified ID is not found</response>
        [HttpDelete("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
                return NotFound();

            _context.Equipment.Remove(equipment);
            await _context.SaveChangesAsync();

            // Return deleted id for client confirmation
            return Ok(new { id });
        }

        /// <summary>
        /// Updates the status of a specific equipment item. Admin access required.
        /// </summary>
        /// <param name="id">The unique identifier of the equipment to update.</param>
        /// <param name="status">The new status for the equipment (Available, Unavailable, UnderRepair).</param>
        /// <returns>The updated equipment item with the new status.</returns>
        /// <response code="200">Returns the equipment item with updated status</response>
        /// <response code="400">If the status value is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        /// <response code="404">If the equipment with the specified ID is not found</response>
        [HttpPut("{id}/status")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] EquipmentStatus status)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null) return NotFound();
            equipment.Status = status;
            await _context.SaveChangesAsync();
            return Ok(equipment);
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipment.Any(e => e.EquipmentId == id);
        }
    }
}
