using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagement2025.Models;

namespace InventoryManagement2025.Data
{
    public class DbInit
    {
        public static void Initialize(SchoolInventory context)
        {
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new()
                    {
                        FirstName = "Alex",
                        LastName = "Morgan",
                        Email = "alex.morgan@example.edu",
                        PhoneNumber = "+1-555-0100",
                        Role = UserRole.Administrator,
                        Department = "IT Services"
                    },
                    new()
                    {
                        FirstName = "Jamie",
                        LastName = "Rivera",
                        Email = "jamie.rivera@example.edu",
                        PhoneNumber = "+1-555-0101",
                        Role = UserRole.Technician,
                        Department = "AV Support"
                    },
                    new()
                    {
                        FirstName = "Taylor",
                        LastName = "Nguyen",
                        Email = "taylor.nguyen@example.edu",
                        PhoneNumber = "+1-555-0102",
                        Role = UserRole.Staff,
                        Department = "Science"
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            if (!context.Equipment.Any())
            {
                var equipment = new List<Equipment>
                {
                    new()
                    {
                        Name = "Lenovo ThinkPad",
                        Type = "Laptop",
                        SerialNumber = "LT11223",
                        Condition = ConditionStatus.Good,
                        Status = EquipmentStatus.Available,
                        Location = "Room 103",
                        PhotoUrl = "https://example.com/photos/lenovo_thinkpad.jpg"
                    },
                    new()
                    {
                        Name = "BenQ Monitor",
                        Type = "Monitor",
                        SerialNumber = "BM44556",
                        Condition = ConditionStatus.Excellent,
                        Status = EquipmentStatus.Available,
                        Location = "Room 204",
                        PhotoUrl = "https://example.com/photos/benq_monitor.jpg"
                    },
                    new()
                    {
                        Name = "Logitech Webcam",
                        Type = "Webcam",
                        SerialNumber = "LW77889",
                        Condition = ConditionStatus.Fair,
                        Status = EquipmentStatus.Maintenance,
                        Location = "Conference Room",
                        PhotoUrl = "https://example.com/photos/logitech_webcam.jpg"
                    },
                    new()
                    {
                        Name = "Samsung Tablet",
                        Type = "Tablet",
                        SerialNumber = "ST99001",
                        Condition = ConditionStatus.Good,
                        Status = EquipmentStatus.CheckedOut,
                        Location = "Reception",
                        PhotoUrl = "https://example.com/photos/samsung_tablet.jpg"
                    }
                };

                context.Equipment.AddRange(equipment);
                context.SaveChanges();
            }

            var usersSnapshot = context.Users.ToList();
            var equipmentSnapshot = context.Equipment.ToList();

            if (!context.ConditionLogs.Any() && equipmentSnapshot.Any())
            {
                var technician = usersSnapshot.FirstOrDefault(u => u.Role == UserRole.Technician);

                var logs = new List<ConditionLog>
                {
                    new()
                    {
                        EquipmentId = equipmentSnapshot[0].EquipmentId,
                        LoggedByUserId = technician?.UserId,
                        Condition = ConditionStatus.Excellent,
                        Notes = "Initial inspection completed."
                    },
                    new()
                    {
                        EquipmentId = equipmentSnapshot[2].EquipmentId,
                        LoggedByUserId = technician?.UserId,
                        Condition = ConditionStatus.Fair,
                        Notes = "Minor scratches observed on casing."
                    }
                };

                context.ConditionLogs.AddRange(logs);
                context.SaveChanges();
            }

            if (!context.Requests.Any() && equipmentSnapshot.Any() && usersSnapshot.Any())
            {
                var requester = usersSnapshot.First(u => u.Role == UserRole.Staff);

                var requests = new List<Request>
                {
                    new()
                    {
                        EquipmentId = equipmentSnapshot[3].EquipmentId,
                        UserId = requester.UserId,
                        Type = RequestType.Checkout,
                        Status = RequestStatus.Approved,
                        RequestedAt = DateTime.UtcNow.AddDays(-5),
                        NeededBy = DateTime.UtcNow.AddDays(-2),
                        Notes = "Tablet needed for parent orientation."
                    },
                    new()
                    {
                        EquipmentId = equipmentSnapshot[1].EquipmentId,
                        UserId = requester.UserId,
                        Type = RequestType.Maintenance,
                        Status = RequestStatus.Pending,
                        RequestedAt = DateTime.UtcNow.AddDays(-1),
                        Notes = "Monitor flickers intermittently."
                    }
                };

                context.Requests.AddRange(requests);
                context.SaveChanges();
            }
        }
    }
}
