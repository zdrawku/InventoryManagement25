using System.ComponentModel.DataAnnotations;

namespace InventoryManagement2025.Models
{
    /// <summary>
    /// Represents an equipment item in the inventory.
    /// </summary>
    public class Equipment
    {
        [Key]
        public int EquipmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        public Condition Condition { get; set; }

        [Required]
        public EquipmentStatus Status { get; set; }

        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        public string PhotoUrl { get; set; } = string.Empty;
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
        Unavailable = 2,
        UnderRepair = 3
    }
}
