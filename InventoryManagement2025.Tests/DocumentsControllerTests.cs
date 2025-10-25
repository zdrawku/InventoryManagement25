using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Xunit;
using InventoryManagement2025.Controllers;
using InventoryManagement2025.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace InventoryManagement2025.Tests
{
    /// <summary>
    /// Tests for document management functionality
    /// </summary>
    public class DocumentsControllerTests : TestBase
    {
        private readonly DocumentsController _controller;

        public DocumentsControllerTests() : base()
        {
            _controller = new DocumentsController(Context);
        }

        private void SetupUserContext(string userId = "user-id", string email = "user@test.com", string role = "User")
        {
            var claims = new List<Claim>
            {
                new Claim("id", userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetDocuments_RegularUser_ReturnsUserVisibleDocuments()
        {
            // Arrange
            SetupUserContext();

            // Act
            var result = await _controller.GetDocuments();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<InventoryDocument>>>(result);
            var documents = actionResult.Value as List<InventoryDocument>;
            Assert.NotNull(documents);
            Assert.All(documents, doc => Assert.Equal("User", doc.VisibilityRole));
        }

        [Fact]
        public async Task GetDocuments_AdminUser_ReturnsAllDocuments()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");

            // Act
            var result = await _controller.GetDocuments();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<InventoryDocument>>>(result);
            var documents = actionResult.Value as List<InventoryDocument>;
            Assert.NotNull(documents);
            Assert.True(documents.Count >= 2); // Should include both User and Admin documents
            Assert.Contains(documents, doc => doc.VisibilityRole == "Admin");
            Assert.Contains(documents, doc => doc.VisibilityRole == "User");
        }

        [Fact]
        public async Task GetDocument_ExistingUserDocument_ReturnsDocument()
        {
            // Arrange
            SetupUserContext();

            // Act
            var result = await _controller.GetDocument(1); // User-visible document

            // Assert
            var actionResult = Assert.IsType<ActionResult<InventoryDocument>>(result);
            var document = actionResult.Value;
            Assert.NotNull(document);
            Assert.Equal("Test Document 1", document.Title);
            Assert.Equal("User", document.VisibilityRole);
        }

        [Fact]
        public async Task GetDocument_AdminDocument_RegularUser_ReturnsForbid()
        {
            // Arrange
            SetupUserContext();

            // Act
            var result = await _controller.GetDocument(2); // Admin-only document

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetDocument_AdminDocument_AdminUser_ReturnsDocument()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");

            // Act
            var result = await _controller.GetDocument(2); // Admin-only document

            // Assert
            var actionResult = Assert.IsType<ActionResult<InventoryDocument>>(result);
            var document = actionResult.Value;
            Assert.NotNull(document);
            Assert.Equal("Admin Only Document", document.Title);
            Assert.Equal("Admin", document.VisibilityRole);
        }

        [Fact]
        public async Task GetDocument_NonExistentDocument_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext();

            // Act
            var result = await _controller.GetDocument(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ValidDocument_CreatesDocument()
        {
            // Arrange
            SetupUserContext();
            var newDocument = new InventoryDocument
            {
                Title = "New Test Document",
                Path = "/documents/new-test.pdf",
                VisibilityRole = "User"
            };

            // Act
            var result = await _controller.Create(newDocument);

            // Assert
            var actionResult = Assert.IsType<ActionResult<InventoryDocument>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdDocument = Assert.IsType<InventoryDocument>(createdResult.Value);
            
            Assert.Equal(newDocument.Title, createdDocument.Title);
            Assert.Equal("user-id", createdDocument.UploadedById);
            Assert.True(createdDocument.Id > 0);

            // Verify in database
            var documentInDb = await Context.Documents.FindAsync(createdDocument.Id);
            Assert.NotNull(documentInDb);
            Assert.Equal(newDocument.Title, documentInDb.Title);
        }

        [Fact]
        public async Task Update_OwnDocument_UpdatesSuccessfully()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin"); // Using admin who uploaded the seeded documents
            var updateDto = new InventoryDocument
            {
                Title = "Updated Test Document",
                Path = "/documents/updated-test.pdf",
                VisibilityRole = "Admin"
            };

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedDocument = Assert.IsType<InventoryDocument>(okResult.Value);
            
            Assert.Equal(updateDto.Title, updatedDocument.Title);
            Assert.Equal(updateDto.Path, updatedDocument.Path);
            Assert.Equal(updateDto.VisibilityRole, updatedDocument.VisibilityRole);

            // Verify in database
            var documentInDb = await Context.Documents.FindAsync(1);
            Assert.Equal(updateDto.Title, documentInDb.Title);
        }

        [Fact]
        public async Task Update_NonOwnDocument_RegularUser_ReturnsForbid()
        {
            // Arrange
            SetupUserContext(); // Regular user trying to update admin's document
            var updateDto = new InventoryDocument
            {
                Title = "Unauthorized Update",
                Path = "/documents/unauthorized.pdf",
                VisibilityRole = "User"
            };

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Update_NonExistentDocument_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext();
            var updateDto = new InventoryDocument
            {
                Title = "Update Non-Existent",
                Path = "/documents/non-existent.pdf",
                VisibilityRole = "User"
            };

            // Act
            var result = await _controller.Update(999, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_OwnDocument_DeletesSuccessfully()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin"); // Using admin who uploaded the seeded documents

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            // Verify deletion in database
            var deletedDocument = await Context.Documents.FindAsync(1);
            Assert.Null(deletedDocument);
        }

        [Fact]
        public async Task Delete_NonOwnDocument_RegularUser_ReturnsForbid()
        {
            // Arrange
            SetupUserContext(); // Regular user trying to delete admin's document

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistentDocument_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext();

            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_AnyDocument_AdminUser_DeletesSuccessfully()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");

            // Act
            var result = await _controller.Delete(2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            // Verify deletion in database
            var deletedDocument = await Context.Documents.FindAsync(2);
            Assert.Null(deletedDocument);
        }
    }
}