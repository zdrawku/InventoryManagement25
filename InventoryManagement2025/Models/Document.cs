using System.ComponentModel.DataAnnotations;

namespace InventoryManagement2025.Models
{
    public class InventoryDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Path { get; set; } = string.Empty;

        // e.g., "Admin" or "User"; keep string to match spec
        [Required]
        [MaxLength(50)]
        public string VisibilityRole { get; set; } = "User";

        [Required]
        public string UploadedById { get; set; } = string.Empty;
        public AppUser? UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
