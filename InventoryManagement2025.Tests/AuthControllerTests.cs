using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using InventoryManagement2025.Controllers;
using InventoryManagement2025.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace InventoryManagement2025.Tests
{
    /// <summary>
    /// Tests for authentication and user management functionality
    /// </summary>
    public class AuthControllerTests : TestBase
    {
        private readonly AuthController _controller;
        private readonly Mock<ILogger<AuthController>> _mockLogger;

        public AuthControllerTests() : base()
        {
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(UserManager, RoleManager, Configuration, _mockLogger.Object);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOkWithUserInfo()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser@test.com",
                Password = "NewUser123!"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);
            
            // Verify user was created
            var createdUser = await UserManager.FindByEmailAsync(request.Email);
            Assert.NotNull(createdUser);
            Assert.Equal(request.Email, createdUser.Email);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "admin@test.com", // This user already exists in test data
                Password = "Password123!"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsTokenForAdmin()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "Admin123!"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);
            
            // Verify token is returned
            var tokenProperty = response.GetType().GetProperty("Token");
            Assert.NotNull(tokenProperty);
            var token = tokenProperty.GetValue(response) as string;
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "WrongPassword"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Login_NonExistentUser_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "nonexistent@test.com",
                Password = "Password123!"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void GetCurrentUser_AuthenticatedUser_ReturnsUserInfo()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("id", "test-user-id"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public void Logout_AuthenticatedUser_ReturnsSuccessMessage()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("id", "test-user-id"),
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetAllUsers_AdminUser_ReturnsAllUsers()
        {
            // Arrange
            var adminClaims = new List<Claim>
            {
                new Claim("id", "admin-id"),
                new Claim(ClaimTypes.Email, "admin@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(adminClaims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var users = okResult.Value as List<object>;
            Assert.NotNull(users);
            Assert.NotEmpty(users);
        }

        [Fact]
        public async Task AssignRole_ValidRequest_AssignsRoleSuccessfully()
        {
            // Arrange
            var adminClaims = new List<Claim>
            {
                new Claim("id", "admin-id"),
                new Claim(ClaimTypes.Email, "admin@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(adminClaims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var request = new AssignRoleRequest
            {
                Email = "user@test.com",
                Role = "Admin"
            };

            // Act
            var result = await _controller.AssignRole(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);

            // Verify role was assigned
            var user = await UserManager.FindByEmailAsync("user@test.com");
            var roles = await UserManager.GetRolesAsync(user);
            Assert.Contains("Admin", roles);
        }

        [Fact]
        public async Task UpdateUserRole_ValidRequest_UpdatesRoleSuccessfully()
        {
            // Arrange
            var adminClaims = new List<Claim>
            {
                new Claim("id", "admin-id"),
                new Claim(ClaimTypes.Email, "admin@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(adminClaims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var user = await UserManager.FindByEmailAsync("user@test.com");
            var request = new UpdateUserRoleRequest { Role = "Admin" };

            // Act
            var result = await _controller.UpdateUserRole(user.Id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);

            // Verify role was updated
            var updatedUser = await UserManager.FindByIdAsync(user.Id);
            var roles = await UserManager.GetRolesAsync(updatedUser);
            Assert.Contains("Admin", roles);
            Assert.DoesNotContain("User", roles); // Old role should be removed
        }

        [Fact]
        public async Task UpdateUserRole_InvalidRole_ReturnsBadRequest()
        {
            // Arrange
            var adminClaims = new List<Claim>
            {
                new Claim("id", "admin-id"),
                new Claim(ClaimTypes.Email, "admin@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(adminClaims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var request = new UpdateUserRoleRequest { Role = "InvalidRole" };

            // Act
            var result = await _controller.UpdateUserRole("user-id", request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}