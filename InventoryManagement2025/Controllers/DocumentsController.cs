using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;

namespace InventoryManagement2025.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Route("documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly SchoolInventory _context;
        public DocumentsController(SchoolInventory context)
        {
            _context = context;
        }

        // GET: /api/Documents or /documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryDocument>>> GetDocuments()
        {
            var isAdmin = User.IsInRole("Admin");
            if (isAdmin)
                return await _context.Documents.AsNoTracking().OrderByDescending(d => d.UploadedAt).ToListAsync();

            // Non-admin: only see documents visible to your role ("User")
            return await _context.Documents.AsNoTracking()
                .Where(d => d.VisibilityRole == "User")
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        // GET: /api/Documents/{id} or /documents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryDocument>> GetDocument(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();
            if (!User.IsInRole("Admin") && doc.VisibilityRole != "User")
                return Forbid();
            return doc;
        }

        // POST: /api/Documents or /documents
        [HttpPost]
        public async Task<ActionResult<InventoryDocument>> Create([FromBody] InventoryDocument doc)
        {
            var userId = User.FindFirst("id")?.Value;
            doc.Id = 0;
            doc.UploadedById = userId ?? string.Empty;
            doc.UploadedAt = DateTime.UtcNow;
            _context.Documents.Add(doc);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDocument), new { id = doc.Id }, doc);
        }

        // PUT: /api/Documents/{id} or /documents/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InventoryDocument dto)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            var userId = User.FindFirst("id")?.Value;
            var isOwner = !string.IsNullOrEmpty(userId) && doc.UploadedById == userId;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !isOwner) return Forbid();

            doc.Title = dto.Title;
            doc.Path = dto.Path;
            doc.VisibilityRole = dto.VisibilityRole;
            await _context.SaveChangesAsync();
            return Ok(doc);
        }

        // DELETE: /api/Documents/{id} or /documents/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            var userId = User.FindFirst("id")?.Value;
            var isOwner = !string.IsNullOrEmpty(userId) && doc.UploadedById == userId;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !isOwner) return Forbid();

            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();
            return Ok(new { id });
        }
    }
}
