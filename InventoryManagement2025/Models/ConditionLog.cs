using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement2025.Models
{
    public class ConditionLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConditionLogId { get; set; }

        [Required]
        public ConditionStatus Condition { get; set; } = ConditionStatus.Good;

        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [ForeignKey(nameof(Equipment))]
        public int EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        [ForeignKey(nameof(LoggedBy))]
        public int? LoggedByUserId { get; set; }
        public User? LoggedBy { get; set; }
    }
}
