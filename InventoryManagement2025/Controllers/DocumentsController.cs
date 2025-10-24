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

        /// <summary>
        /// Retrieves all documents based on user role permissions.
        /// </summary>
        /// <returns>A list of documents. Admins see all documents, users see only documents visible to their role.</returns>
        /// <response code="200">Returns the list of documents</response>
        /// <response code="401">If the user is not authenticated</response>
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

        /// <summary>
        /// Retrieves a specific document by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the document.</param>
        /// <returns>The document with the specified ID.</returns>
        /// <response code="200">Returns the requested document</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have permission to view this document</response>
        /// <response code="404">If the document with the specified ID is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryDocument>> GetDocument(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();
            if (!User.IsInRole("Admin") && doc.VisibilityRole != "User")
                return Forbid();
            return doc;
        }

        /// <summary>
        /// Creates a new document in the system.
        /// </summary>
        /// <param name="doc">The document object to create. The ID, UploadedById, and UploadedAt fields will be automatically set.</param>
        /// <returns>The created document with assigned ID and metadata.</returns>
        /// <response code="201">Returns the newly created document</response>
        /// <response code="400">If the document data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
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

        /// <summary>
        /// Updates an existing document. Only the document owner or admin can perform this operation.
        /// </summary>
        /// <param name="id">The unique identifier of the document to update.</param>
        /// <param name="dto">The updated document data containing Title, Path, and VisibilityRole.</param>
        /// <returns>The updated document.</returns>
        /// <response code="200">Returns the updated document</response>
        /// <response code="400">If the update data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have permission to update this document</response>
        /// <response code="404">If the document with the specified ID is not found</response>
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

        /// <summary>
        /// Deletes a document from the system. Only the document owner or admin can perform this operation.
        /// </summary>
        /// <param name="id">The unique identifier of the document to delete.</param>
        /// <returns>A confirmation object containing the deleted document ID.</returns>
        /// <response code="200">Returns confirmation of successful deletion with the document ID</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have permission to delete this document</response>
        /// <response code="404">If the document with the specified ID is not found</response>
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
