using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagement2025.Models;

namespace InventoryManagement2025.Controllers
{
    /// <summary>
    /// Handles user authentication, registration, and role management operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthController.
        /// </summary>
        /// <param name="userManager">The user manager for handling user operations.</param>
        /// <param name="roleManager">The role manager for handling role operations.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="logger">The logger for recording authentication events.</param>
        public AuthController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user account in the system.
        /// </summary>
        /// <param name="request">The registration details including email and password.</param>
        /// <returns>The created user information with ID and email.</returns>
        /// <response code="200">Returns the newly created user information</response>
        /// <response code="400">If the registration data is invalid or user creation fails</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new AppUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            _logger.LogInformation($"New user registered: {user.Email}");
            return Ok(new { user.Id, user.Email });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token for API access.
        /// </summary>
        /// <param name="request">The login credentials including email and password.</param>
        /// <returns>A JWT token for authenticated API access.</returns>
        /// <response code="200">Returns the JWT token for successful authentication</response>
        /// <response code="401">If the credentials are invalid</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Login attempt failed: User not found for email {request.Email}");
                return Unauthorized();
            }

            var valid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!valid)
            {
                _logger.LogWarning($"Login attempt failed: Invalid password for user {request.Email}");
                return Unauthorized();
            }

            var jwtKey = _configuration["JwtSettings:Key"] ?? "DevSigningKey_MUST_CHANGE_AtLeast32Chars_Long_123456!";
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(jwtKey);
            var roles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new Claim("id", user.Id ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };
            
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation($"User {user.Email} logged in successfully");
            return Ok(new { Token = tokenString });
        }

        /// <summary>
        /// Logs out the authenticated user and records the logout event.
        /// </summary>
        /// <returns>Confirmation of successful logout with timestamp and user information.</returns>
        /// <response code="200">Returns logout confirmation</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var userId = User.FindFirst("id")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            
            // Log the logout event
            _logger.LogInformation($"User {email} (ID: {userId}) logged out at {DateTime.UtcNow}");
            
            // Since JWT tokens are stateless, actual logout happens on the client side
            // by removing the token from storage. This endpoint serves for:
            // 1. Logging purposes
            // 2. Future token blacklisting implementation if needed
            // 3. Server-side cleanup if required
            
            return Ok(new { 
                message = "Logged out successfully", 
                timestamp = DateTime.UtcNow,
                user = new { id = userId, email = email }
            });
        }

        /// <summary>
        /// Retrieves the current authenticated user's information from their token.
        /// </summary>
        /// <returns>The current user's ID, email, and assigned roles.</returns>
        /// <response code="200">Returns the current user information</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var id = User.FindFirst("id")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            
            return Ok(new { id, email, roles });
        }

        /// <summary>
        /// Assigns a role to a user. Admin access required.
        /// </summary>
        /// <param name="request">The role assignment details including user email and role name.</param>
        /// <returns>The updated user information with assigned roles.</returns>
        /// <response code="200">Returns the user with updated role assignments</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        /// <response code="404">If the specified user is not found</response>
        [HttpPost("roles/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest("Email and Role are required.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(request.Role));
            }

            var already = await _userManager.IsInRoleAsync(user, request.Role);
            if (!already)
            {
                var addResult = await _userManager.AddToRoleAsync(user, request.Role);
                if (!addResult.Succeeded)
                {
                    return BadRequest(addResult.Errors);
                }
            }

            var rolesNow = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"Role {request.Role} assigned to user {user.Email} by {User.FindFirst(ClaimTypes.Email)?.Value}");
            
            return Ok(new { user.Id, user.Email, roles = rolesNow });
        }

        /// <summary>
        /// Retrieves all users in the system with their roles. Admin access required.
        /// </summary>
        /// <returns>A list of all users with their ID, email, and assigned roles.</returns>
        /// <response code="200">Returns the list of all users with role information</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    user.Id,
                    user.Email,
                    user.UserName,
                    user.Name,
                    roles = roles.ToArray()
                });
            }

            return Ok(userList);
        }

        /// <summary>
        /// Updates a user's role, replacing all existing roles with the specified role. Admin access required.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose role should be updated.</param>
        /// <param name="request">The role update details including the new role.</param>
        /// <returns>The updated user information with the new role assignment.</returns>
        /// <response code="200">Returns the user with updated role assignment</response>
        /// <response code="400">If the request data is invalid or role update fails</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not an admin</response>
        /// <response code="404">If the specified user is not found</response>
        [HttpPut("users/{userId}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateUserRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest("User ID and Role are required.");
            }

            // Validate that the role is either Admin or User
            if (request.Role != "Admin" && request.Role != "User")
            {
                return BadRequest("Role must be either 'Admin' or 'User'.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Ensure the role exists
            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(request.Role));
            }

            // Get current roles and remove all of them
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return BadRequest(removeResult.Errors);
                }
            }

            // Add the new role
            var addResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!addResult.Succeeded)
            {
                return BadRequest(addResult.Errors);
            }

            var updatedRoles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"User {user.Email} role updated to {request.Role} by {User.FindFirst(ClaimTypes.Email)?.Value}");
            
            return Ok(new { user.Id, user.Email, user.UserName, user.Name, roles = updatedRoles });
        }
    }

    /// <summary>
    /// Request model for assigning roles to users.
    /// </summary>
    public class AssignRoleRequest
    {
        /// <summary>
        /// The email address of the user to assign the role to.
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// The name of the role to assign to the user.
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for updating a user's role.
    /// </summary>
    public class UpdateUserRoleRequest
    {
        /// <summary>
        /// The role to assign to the user (Admin or User).
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}