using InventoryManagement2025.Models;

namespace InventoryManagement2025.Data
{
    public class DbInit
    {
        public static void Initialize(SchoolInventory context)
        {
            // Ensure database is created through migrations in Program.cs (context.Database.Migrate())

            if (context.Equipment.Any())
            {
                return;   // DB has been seeded
            }

            var equipment = new Equipment[]
            {
                new Equipment{Name="Laptop Dell Inspiron 15", Type="Laptop", SerialNumber="SN-DL-001", Condition=Condition.Excellent, Status=EquipmentStatus.Available, Location="Room A1", PhotoUrl="https://example.com/photos/laptop1.jpg", IsSensitive=false},
                new Equipment{Name="3D Printer Creality", Type="Printer", SerialNumber="SN-PR-002", Condition=Condition.Good, Status=EquipmentStatus.CheckedOut, Location="Lab 2", PhotoUrl="https://example.com/photos/printer.jpg", IsSensitive=true},
                new Equipment{Name="Projector Epson X500", Type="Projector", SerialNumber="SN-EP-003", Condition=Condition.Excellent, Status=EquipmentStatus.Available, Location="Office 3", PhotoUrl="https://example.com/photos/projector.jpg", IsSensitive=false},
                new Equipment{Name="Server Rack HP ProLiant", Type="Server", SerialNumber="SN-SV-004", Condition=Condition.Fair, Status=EquipmentStatus.UnderRepair, Location="Storage B", PhotoUrl="https://example.com/photos/server.jpg", IsSensitive=true},
                new Equipment{Name="Microscope Nikon A10", Type="Optical", SerialNumber="SN-MC-005", Condition=Condition.Damaged, Status=EquipmentStatus.Retired, Location="Lab 1", PhotoUrl="https://example.com/photos/microscope.jpg", IsSensitive=false},
                new Equipment{Name="Desktop PC Lenovo", Type="Computer", SerialNumber="SN-PC-006", Condition=Condition.Good, Status=EquipmentStatus.Available, Location="Room B2", PhotoUrl="https://example.com/photos/desktop.jpg", IsSensitive=false},
                new Equipment{Name="Tablet Samsung Galaxy", Type="Tablet", SerialNumber="SN-TB-007", Condition=Condition.Excellent, Status=EquipmentStatus.CheckedOut, Location="Office 2", PhotoUrl="https://example.com/photos/tablet.jpg", IsSensitive=true},
                new Equipment{Name="Barcode Scanner Honeywell", Type="Scanner", SerialNumber="SN-SC-008", Condition=Condition.Fair, Status=EquipmentStatus.Unavailable, Location="Library", PhotoUrl="https://example.com/photos/scanner.jpg", IsSensitive=false},
                new Equipment{Name="Router Cisco 2900", Type="Networking", SerialNumber="SN-RT-009", Condition=Condition.Good, Status=EquipmentStatus.Available, Location="Room C3", PhotoUrl="https://example.com/photos/router.jpg", IsSensitive=false},
                new Equipment{Name="External HDD Seagate", Type="Storage", SerialNumber="SN-HD-010", Condition=Condition.Excellent, Status=EquipmentStatus.Available, Location="IT Storage", PhotoUrl="https://example.com/photos/hdd.jpg", IsSensitive=true},
                new Equipment{Name="Oscilloscope Tektronix", Type="Electronics", SerialNumber="SN-OS-011", Condition=Condition.Fair, Status=EquipmentStatus.UnderRepair, Location="Lab 3", PhotoUrl="https://example.com/photos/oscilloscope.jpg", IsSensitive=false},
                new Equipment{Name="Monitor Dell 24\"", Type="Display", SerialNumber="SN-MN-012", Condition=Condition.Good, Status=EquipmentStatus.Available, Location="Office 1", PhotoUrl="https://example.com/photos/monitor.jpg", IsSensitive=true},
                new Equipment{Name="Camera Canon EOS", Type="Camera", SerialNumber="SN-CM-013", Condition=Condition.Damaged, Status=EquipmentStatus.Retired, Location="Storage C", PhotoUrl="https://example.com/photos/camera.jpg", IsSensitive=false},
                new Equipment{Name="Smartboard Samsung", Type="Display", SerialNumber="SN-SB-014", Condition=Condition.Excellent, Status=EquipmentStatus.CheckedOut, Location="Lab 4", PhotoUrl="https://example.com/photos/smartboard.jpg", IsSensitive=true},
                new Equipment{Name="Speaker JBL Studio", Type="Audio", SerialNumber="SN-SP-015", Condition=Condition.Good, Status=EquipmentStatus.Available, Location="Room D1", PhotoUrl="https://example.com/photos/speaker.jpg", IsSensitive=false},
                new Equipment{Name="UPS APC 1500VA", Type="Power", SerialNumber="SN-UP-016", Condition=Condition.Fair, Status=EquipmentStatus.Unavailable, Location="Maintenance Room", PhotoUrl="https://example.com/photos/ups.jpg", IsSensitive=true}
            };

            context.Equipment.AddRange(equipment);
            context.SaveChanges();
        }
    }
}
