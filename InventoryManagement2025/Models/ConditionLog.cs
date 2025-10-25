using System.ComponentModel.DataAnnotations;

namespace InventoryManagement2025.Models
{
    public class ConditionLog
    {
        [Key]
        public int Id { get; set; }

        public int EquipmentId { get; set; }

        public Condition OldCondition { get; set; }

        public Condition NewCondition { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string? ChangedById { get; set; }

        public string? Notes { get; set; }
    }
}
