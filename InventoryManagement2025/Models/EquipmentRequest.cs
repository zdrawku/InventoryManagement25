using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace InventoryManagement2025.Models
{
    public enum RequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Returned = 3
    }

    public class EquipmentRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        // Set server-side from the authenticated user
        [BindNever]
        public string RequesterId { get; set; } = string.Empty;
        [JsonIgnore]
        public AppUser? Requester { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Requested borrowing window
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public string? ApprovedById { get; set; }
        [JsonIgnore]
        public AppUser? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? Notes { get; set; }

        // Return tracking
        public DateTime? ReturnedAt { get; set; }
        public string? ReturnNotes { get; set; }
    }
}
