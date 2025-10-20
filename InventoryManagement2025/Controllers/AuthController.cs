using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InventoryManagement2025.Data;
using InventoryManagement2025.Models;
using InventoryManagement2025.Models.Auth;
using InventoryManagement2025.Services;
using InventoryManagement2025.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement2025.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly UserRole[] SelfServiceRoles = new[]
        {
            UserRole.Staff,
            UserRole.Student
        };

        private readonly SchoolInventory _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthController(
            SchoolInventory context,
            IPasswordHasher<User> passwordHasher,
            ITokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (existingUser != null)
            {
                return Conflict("An account with this email already exists.");
            }

            var role = request.Role ?? UserRole.Staff;
            if (!SelfServiceRoles.Contains(role))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Only staff or student accounts can be self-registered.");
            }

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = normalizedEmail,
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim(),
                Role = role,
                IsActive = true,
                PasswordChangedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateToken(user);

            return Ok(CreateAuthResponse(user, token));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            if (!user.IsActive)
            {
                return Unauthorized("Account has been deactivated. Contact the administrator.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid credentials.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateToken(user);
            return Ok(CreateAuthResponse(user, token));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserProfile>> GetProfile()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(ToProfile(user));
        }

        private static AuthResponse CreateAuthResponse(User user, TokenResult token)
        {
            return new AuthResponse
            {
                AccessToken = token.AccessToken,
                ExpiresAt = token.ExpiresAt,
                User = ToProfile(user)
            };
        }

        private static UserProfile ToProfile(User user) => new()
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            Department = user.Department,
            PhoneNumber = user.PhoneNumber
        };
    }
}
