using System;

namespace InventoryManagement2025.Models.Auth
{
    public class AuthResponse
    {
        public required string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public required UserProfile User { get; set; }
    }

    public class UserProfile
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
