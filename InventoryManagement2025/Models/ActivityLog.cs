using System.ComponentModel.DataAnnotations;

namespace InventoryManagement2025.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }
        public AppUser? User { get; set; }

        public int? EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }
    }
}
