using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace InventoryManagement2025.Models
{
    public class AppUser : IdentityUser
    {
        // Additional user profile fields can go here

        // Navigation: requests created by this user
        [JsonIgnore]
        public ICollection<EquipmentRequest> Requests { get; set; } = new List<EquipmentRequest>();

        // Navigation: requests this user approved
        [JsonIgnore]
        public ICollection<EquipmentRequest> ApprovedRequests { get; set; } = new List<EquipmentRequest>();

        // Display name of the user
        public string? Name { get; set; }
    }
}
