using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement2025.Models
{
    /// <summary>
    /// Represents an equipment item in the inventory.
    /// </summary>
    public class Equipment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EquipmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        public Condition Condition { get; set; } = Condition.Good;

        [Required]
        public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;

        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        public string PhotoUrl { get; set; } = string.Empty;

        // If true the item is considered sensitive and requires admin approval
        public bool IsSensitive { get; set; } = false;
    }

    /// <summary>
    /// Represents the physical condition of the equipment.
    /// </summary>
    public enum Condition
    {
        Excellent = 1,
        Good = 2,
        Fair = 3,
        Damaged = 4
    }

    /// <summary>
    /// Represents the operational or availability status of the equipment.
    /// </summary>
    public enum EquipmentStatus
    {
        Available = 1,
        CheckedOut = 2,
        UnderRepair = 3,
        Retired = 4,
        Unavailable = 5
    }
}
