using Microsoft.AspNetCore.Identity;

namespace InventoryManagement2025.Models
{
    public class AppUser : IdentityUser
    {
        // Additional user profile fields can go here

        // Navigation: requests created by this user
        public ICollection<EquipmentRequest> Requests { get; set; } = new List<EquipmentRequest>();

        // Navigation: requests this user approved
        public ICollection<EquipmentRequest> ApprovedRequests { get; set; } = new List<EquipmentRequest>();
    }
}
