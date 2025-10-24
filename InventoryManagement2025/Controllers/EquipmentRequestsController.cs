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

        /// <summary>
        /// Creates a new equipment request. Automatically approves non-sensitive equipment or creates pending request for sensitive equipment.
        /// </summary>
        /// <param name="request">The equipment request details including EquipmentId, Start time, End time, and optional Notes.</param>
        /// <returns>The created equipment request with assigned ID and status.</returns>
        /// <response code="201">Returns the newly created equipment request</response>
        /// <response code="400">If the request data is invalid (missing times, invalid time range)</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the specified equipment is not found</response>
        /// <response code="409">If the equipment is already requested for the specified time window</response>
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

        /// <summary>
        /// Retrieves all equipment requests made by the authenticated user.
        /// </summary>
        /// <returns>A list of the user's equipment requests ordered by request date (most recent first), including equipment details.</returns>
        /// <response code="200">Returns the user's equipment requests</response>
        /// <response code="401">If the user is not authenticated</response>
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

        /// <summary>
        /// Retrieves all equipment requests in the system. Admin access required.
        /// </summary>
        /// <returns>A list of all equipment requests ordered by request date (most recent first), including equipment details.</returns>
        /// <response code="200">Returns all equipment requests</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
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

        /// <summary>
        /// Retrieves all equipment requests for a specific user. Admin access required.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose requests to retrieve.</param>
        /// <returns>A list of equipment requests for the specified user ordered by request date (most recent first).</returns>
        /// <response code="200">Returns the user's equipment requests</response>
        /// <response code="400">If the userId parameter is empty or invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
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

        /// <summary>
        /// Approves a pending equipment request and checks out the equipment. Admin access required.
        /// </summary>
        /// <param name="id">The unique identifier of the equipment request to approve.</param>
        /// <returns>The approved equipment request with updated status and approval details.</returns>
        /// <response code="200">Returns the approved equipment request</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        /// <response code="404">If the request or associated equipment is not found</response>
        /// <response code="409">If the request is not in pending status or equipment is unavailable for the requested time</response>
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

        /// <summary>
        /// Rejects an equipment request with optional rejection notes. Admin access required.
        /// </summary>
        /// <param name="id">The unique identifier of the equipment request to reject.</param>
        /// <param name="notes">Optional notes explaining the reason for rejection.</param>
        /// <returns>The rejected equipment request with updated status and rejection details.</returns>
        /// <response code="200">Returns the rejected equipment request</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        /// <response code="404">If the request is not found</response>
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

        /// <summary>
        /// Processes the return of equipment, updating request status and equipment condition/status. Admin access required.
        /// </summary>
        /// <param name="id">The unique identifier of the equipment request to mark as returned.</param>
        /// <param name="dto">The return details including optional new condition, status, and return notes.</param>
        /// <returns>The returned equipment request with updated status and return details.</returns>
        /// <response code="200">Returns the equipment request marked as returned</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        /// <response code="404">If the request or associated equipment is not found</response>
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
