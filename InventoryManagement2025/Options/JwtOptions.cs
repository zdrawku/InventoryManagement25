using System.ComponentModel.DataAnnotations;

namespace InventoryManagement2025.Options
{
    public class JwtOptions
    {
        [Required]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(5, 1440)]
        public int AccessTokenMinutes { get; set; } = 60;
    }
}
