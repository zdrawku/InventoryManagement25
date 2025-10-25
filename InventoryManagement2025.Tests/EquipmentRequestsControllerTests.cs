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
    /// Tests for equipment request management functionality
    /// </summary>
    public class EquipmentRequestsControllerTests : TestBase
    {
        private readonly EquipmentRequestsController _controller;

        public EquipmentRequestsControllerTests() : base()
        {
            _controller = new EquipmentRequestsController(Context);
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
        public async Task CreateRequest_ValidNonSensitiveEquipment_AutoApproves()
        {
            // Arrange
            SetupUserContext();
            var request = new EquipmentRequest
            {
                EquipmentId = 1, // Non-sensitive equipment
                Start = DateTime.UtcNow.AddDays(1),
                End = DateTime.UtcNow.AddDays(3),
                Notes = "Test request for non-sensitive equipment"
            };

            // Act
            var result = await _controller.CreateRequest(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdRequest = Assert.IsType<EquipmentRequest>(createdResult.Value);
            
            Assert.Equal(RequestStatus.Approved, createdRequest.Status);
            Assert.Equal("user-id", createdRequest.RequesterId);
            Assert.NotNull(createdRequest.ApprovedAt);

            // Verify equipment status changed
            var equipment = await Context.Equipment.FindAsync(1);
            Assert.Equal(EquipmentStatus.CheckedOut, equipment.Status);
        }

        [Fact]
        public async Task CreateRequest_ValidSensitiveEquipment_CreatesPendingRequest()
        {
            // Arrange
            SetupUserContext();
            var request = new EquipmentRequest
            {
                EquipmentId = 2, // Sensitive equipment
                Start = DateTime.UtcNow.AddDays(1),
                End = DateTime.UtcNow.AddDays(3),
                Notes = "Test request for sensitive equipment"
            };

            // Act
            var result = await _controller.CreateRequest(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdRequest = Assert.IsType<EquipmentRequest>(createdResult.Value);
            
            Assert.Equal(RequestStatus.Pending, createdRequest.Status);
            Assert.Equal("user-id", createdRequest.RequesterId);
            Assert.Null(createdRequest.ApprovedAt);

            // Verify equipment status unchanged
            var equipment = await Context.Equipment.FindAsync(2);
            Assert.Equal(EquipmentStatus.Available, equipment.Status);
        }

        [Fact]
        public async Task CreateRequest_InvalidTimeRange_ReturnsBadRequest()
        {
            // Arrange
            SetupUserContext();
            var request = new EquipmentRequest
            {
                EquipmentId = 1,
                Start = DateTime.UtcNow.AddDays(2),
                End = DateTime.UtcNow.AddDays(1), // End before start
                Notes = "Invalid time range"
            };

            // Act
            var result = await _controller.CreateRequest(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("End time must be after Start time.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateRequest_NonExistentEquipment_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext();
            var request = new EquipmentRequest
            {
                EquipmentId = 999, // Non-existent equipment
                Start = DateTime.UtcNow.AddDays(1),
                End = DateTime.UtcNow.AddDays(3),
                Notes = "Request for non-existent equipment"
            };

            // Act
            var result = await _controller.CreateRequest(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Equipment not found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetMyRequests_ReturnsUserRequests()
        {
            // Arrange
            SetupUserContext();

            // Act
            var result = await _controller.GetMyRequests();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<EquipmentRequest>>>(result);
            var requests = actionResult.Value as List<EquipmentRequest>;
            Assert.NotNull(requests);
            Assert.All(requests, req => Assert.Equal("user-id", req.RequesterId));
        }

        [Fact]
        public async Task GetAllRequests_AdminUser_ReturnsAllRequests()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");

            // Act
            var result = await _controller.GetAllRequests();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<EquipmentRequest>>>(result);
            var requests = actionResult.Value as List<EquipmentRequest>;
            Assert.NotNull(requests);
            Assert.True(requests.Count >= 2); // Should contain all seeded requests
        }

        [Fact]
        public async Task GetRequestsForUser_AdminUser_ReturnsUserRequests()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");

            // Act
            var result = await _controller.GetRequestsForUser("user-id");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<EquipmentRequest>>>(result);
            var requests = actionResult.Value as List<EquipmentRequest>;
            Assert.NotNull(requests);
            Assert.All(requests, req => Assert.Equal("user-id", req.RequesterId));
        }

        [Fact]
        public async Task GetRequestsForUser_EmptyUserId_ReturnsBadRequest()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");

            // Act
            var result = await _controller.GetRequestsForUser("");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<EquipmentRequest>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("userId is required", badRequestResult.Value);
        }

        [Fact]
        public async Task Approve_PendingRequest_ApprovesSuccessfully()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");

            // Act
            var result = await _controller.Approve(2); // Pending request from seeded data

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var approvedRequest = Assert.IsType<EquipmentRequest>(okResult.Value);
            
            Assert.Equal(RequestStatus.Approved, approvedRequest.Status);
            Assert.Equal("admin-id", approvedRequest.ApprovedById);
            Assert.NotNull(approvedRequest.ApprovedAt);

            // Verify equipment status changed
            var equipment = await Context.Equipment.FindAsync(2);
            Assert.Equal(EquipmentStatus.CheckedOut, equipment.Status);
        }

        [Fact]
        public async Task Approve_NonPendingRequest_ReturnsConflict()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");

            // Act
            var result = await _controller.Approve(1); // Already approved request

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("Cannot approve a request with status", conflictResult.Value.ToString());
        }

        [Fact]
        public async Task Approve_NonExistentRequest_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");

            // Act
            var result = await _controller.Approve(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Reject_ValidRequest_RejectsSuccessfully()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");
            var rejectionNotes = "Request rejected due to policy violation";

            // Act
            var result = await _controller.Reject(2, rejectionNotes);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var rejectedRequest = Assert.IsType<EquipmentRequest>(okResult.Value);
            
            Assert.Equal(RequestStatus.Rejected, rejectedRequest.Status);
            Assert.Equal("admin-id", rejectedRequest.ApprovedById);
            Assert.Equal(rejectionNotes, rejectedRequest.Notes);
        }

        [Fact]
        public async Task Return_ValidRequest_ProcessesReturnSuccessfully()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");
            var returnDto = new ReturnRequest
            {
                Condition = Condition.Good,
                Status = EquipmentStatus.Available,
                Notes = "Equipment returned in good condition"
            };

            // Act
            var result = await _controller.Return(1, returnDto); // Approved request from seeded data

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedRequest = Assert.IsType<EquipmentRequest>(okResult.Value);
            
            Assert.Equal(RequestStatus.Returned, returnedRequest.Status);
            Assert.NotNull(returnedRequest.ReturnedAt);
            Assert.Equal(returnDto.Notes, returnedRequest.ReturnNotes);

            // Verify equipment status and condition updated
            var equipment = await Context.Equipment.FindAsync(1);
            Assert.Equal(EquipmentStatus.Available, equipment.Status);
            Assert.Equal(Condition.Good, equipment.Condition);

            // Verify condition log was created
            var conditionLogs = Context.Set<ConditionLog>()
                .Where(l => l.EquipmentId == 1)
                .ToList();
            Assert.NotEmpty(conditionLogs);
        }

        [Fact]
        public async Task Return_NonExistentRequest_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext("admin-id", "admin@test.com", "Admin");
            var returnDto = new ReturnRequest
            {
                Condition = Condition.Good,
                Status = EquipmentStatus.Available,
                Notes = "Return notes"
            };

            // Act
            var result = await _controller.Return(999, returnDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}