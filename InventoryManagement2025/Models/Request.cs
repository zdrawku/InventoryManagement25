using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement2025.Models
{
    public class Request
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Required]
        public RequestType Type { get; set; } = RequestType.Checkout;

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? NeededBy { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [ForeignKey(nameof(Equipment))]
        public int EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }
    }

    public enum RequestType
    {
        Checkout = 1,
        Return = 2,
        Maintenance = 3
    }

    public enum RequestStatus
    {
        Pending = 1,
        Approved = 2,
        Denied = 3,
        Completed = 4,
        Cancelled = 5
    }
}
