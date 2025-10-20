using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement2025.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
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

        // GET: api/Equipment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipment>> GetEquipment(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);

            if (equipment == null)
            {
                return NotFound();
            }

            return equipment;
        }

        // PUT: api/Equipment/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Technician")]
        public async Task<IActionResult> PutEquipment(int id, [FromBody] Equipment equipment)
        {
            if (id != equipment.EquipmentId)
            {
                return BadRequest("Equipment ID mismatch.");
            }

            _context.Entry(equipment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipmentExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // POST: api/Equipment
        [HttpPost]
        [Authorize(Roles = "Administrator,Technician")]
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
        [Authorize(Roles = "Administrator,Technician")]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }

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
