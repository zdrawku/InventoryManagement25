using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;

namespace InventoryManagement2025.Controllers
{
    [Route("api/[controller]")]
    [Route("requests")]
    [ApiController]
    [Authorize]
    public class EquipmentRequestsController : ControllerBase
    {
        private readonly SchoolInventory _context;

        public EquipmentRequestsController(SchoolInventory context)
        {
            _context = context;
        }

        // POST: api/EquipmentRequests
    [HttpPost]
    [HttpPost("/request")] // alias per final spec
    public async Task<IActionResult> CreateRequest([FromBody] EquipmentRequest request)
        {
            // Validate equipment exists
            var equipment = await _context.Equipment.FindAsync(request.EquipmentId);
            if (equipment == null) return NotFound("Equipment not found");

            // Require start and end
            if (request.Start == null || request.End == null)
                return BadRequest("Request must include Start and End times.");
            if (request.End <= request.Start)
                return BadRequest("End time must be after Start time.");

            // Basic overlap check: ensure no approved request overlaps for this equipment
            var overlapping = await _context.EquipmentRequests.Where(r => r.EquipmentId == request.EquipmentId && r.Status == RequestStatus.Approved)
                .AnyAsync(r => !(r.End <= request.Start || r.Start >= request.End));
            if (overlapping)
                return Conflict("Equipment already requested for the specified time window.");

            request.RequesterId = User.FindFirst("id")?.Value ?? string.Empty;
            request.RequestedAt = DateTime.UtcNow;

            // If the equipment is sensitive, require admin approval
            if (equipment.IsSensitive)
            {
                request.Status = RequestStatus.Pending;
            }
            else
            {
                // Auto-approve and mark as checked out
                request.Status = RequestStatus.Approved;
                request.ApprovedAt = DateTime.UtcNow;
                request.ApprovedById = request.RequesterId; // self-approved
                equipment.Status = EquipmentStatus.CheckedOut;
            }

            _context.EquipmentRequests.Add(request);
            await _context.SaveChangesAsync();

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = request.RequesterId,
                EquipmentId = request.EquipmentId,
                Action = equipment.IsSensitive ? "request:create" : "request:auto-approve",
                Timestamp = DateTime.UtcNow,
                Notes = request.Notes
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyRequests), new { id = request.Id }, request);
        }

        // GET: api/EquipmentRequests/mine
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<EquipmentRequest>>> GetMyRequests()
        {
            var userId = User.FindFirst("id")?.Value;
            return await _context.EquipmentRequests
                .AsNoTracking()
                .Include(r => r.Equipment)
                .Where(r => r.RequesterId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        // GET: api/EquipmentRequests
        [HttpGet]
    [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EquipmentRequest>>> GetAllRequests()
        {
            return await _context.EquipmentRequests
                .AsNoTracking()
                .Include(r => r.Equipment)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        // GET: api/EquipmentRequests/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EquipmentRequest>>> GetRequestsForUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return BadRequest("userId is required");
            var list = await _context.EquipmentRequests
                .Where(r => r.RequesterId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
            return list;
        }

        // PATCH: api/EquipmentRequests/5/approve
    [HttpPatch("{id}/approve")]
    [HttpPut("/request/{id}/approve")] // alias per final spec
    [HttpPut("/request/{id}/accept")] // extra alias per spec wording
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.EquipmentRequests.FindAsync(id);
            if (req == null) return NotFound();

            if (req.Status != RequestStatus.Pending)
            {
                return Conflict($"Cannot approve a request with status '{req.Status}'.");
            }

            var equipment = await _context.Equipment.FindAsync(req.EquipmentId);
            if (equipment == null) return NotFound("Equipment not found");

            // Check overlap against other approved requests for this equipment
            var overlap = await _context.EquipmentRequests
                .Where(r => r.Id != id && r.EquipmentId == req.EquipmentId && r.Status == RequestStatus.Approved)
                .AnyAsync(r => !(r.End <= req.Start || r.Start >= req.End));
            if (overlap)
            {
                return Conflict("Equipment not available in the requested time window.");
            }

            req.Status = RequestStatus.Approved;
            req.ApprovedById = User.FindFirst("id")?.Value;
            req.ApprovedAt = DateTime.UtcNow;

            equipment.Status = EquipmentStatus.CheckedOut;

            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = req.ApprovedById,
                EquipmentId = req.EquipmentId,
                Action = "request:approve",
                Timestamp = DateTime.UtcNow,
                Notes = req.Notes
            });
            await _context.SaveChangesAsync();
            return Ok(req);
        }

        // PATCH: api/EquipmentRequests/5/reject
    [HttpPatch("{id}/reject")]
    [HttpPut("/request/{id}/reject")] // alias per final spec
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] string? notes)
        {
            var req = await _context.EquipmentRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = RequestStatus.Rejected;
            req.ApprovedById = User.FindFirst("id")?.Value;
            req.ApprovedAt = DateTime.UtcNow;
            req.Notes = notes;

            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = req.ApprovedById,
                EquipmentId = req.EquipmentId,
                Action = "request:reject",
                Timestamp = DateTime.UtcNow,
                Notes = notes
            });
            await _context.SaveChangesAsync();
            return Ok(req);
        }

        // PATCH: api/EquipmentRequests/5/return
    [HttpPatch("{id}/return")]
    [HttpPut("/request/{id}/return")] // alias per final spec
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Return(int id, [FromBody] ReturnRequest dto)
        {
            var req = await _context.EquipmentRequests.FindAsync(id);
            if (req == null) return NotFound();

            var equipment = await _context.Equipment.FindAsync(req.EquipmentId);
            if (equipment == null) return NotFound("Equipment not found");

            // Mark request returned
            req.Status = RequestStatus.Returned;
            req.ReturnedAt = DateTime.UtcNow;
            req.ReturnNotes = dto?.Notes;

            // Determine new status and condition
            var oldCondition = equipment.Condition;
            var newCondition = dto?.Condition ?? equipment.Condition;

            if (dto?.Status != null)
            {
                equipment.Status = dto.Status.Value;
            }
            else
            {
                equipment.Status = EquipmentStatus.Available;
            }

            equipment.Condition = newCondition;

            // Persist a ConditionLog entry when condition changed (or always record a return entry)
            var log = new ConditionLog
            {
                EquipmentId = equipment.EquipmentId,
                OldCondition = oldCondition,
                NewCondition = newCondition,
                ChangedAt = DateTime.UtcNow,
                ChangedById = User?.FindFirst("id")?.Value,
                Notes = dto?.Notes
            };
            _context.Set<ConditionLog>().Add(log);

            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = User?.FindFirst("id")?.Value,
                EquipmentId = equipment.EquipmentId,
                Action = "request:return",
                Timestamp = DateTime.UtcNow,
                Notes = dto?.Notes
            });
            await _context.SaveChangesAsync();
            return Ok(req);
        }
    }

    public class ReturnRequest
    {
        public Condition? Condition { get; set; }
        public EquipmentStatus? Status { get; set; }
        public string? Notes { get; set; }
    }
}
