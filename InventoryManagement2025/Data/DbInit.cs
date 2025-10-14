using InventoryManagement2025.Models;

namespace InventoryManagement2025.Data
{
    public class DbInit
    {
        public static void Initialize(SchoolInventory context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Equipment.Any())
            {
                return;   // DB has been seeded
            }

            var equipment = new Equipment[]
            {

                new Equipment{Name="Lenovo ThinkPad", Type="Laptop", SerialNumber="LT11223", Condition=Condition.Good, Status=EquipmentStatus.Available, Location="Room 103", PhotoUrl="https://example.com/photos/lenovo_thinkpad.jpg"},
                new Equipment{Name="BenQ Monitor", Type="Monitor", SerialNumber="BM44556", Condition=Condition.Excellent, Status=EquipmentStatus.Available, Location="Room 204", PhotoUrl="https://example.com/photos/benq_monitor.jpg"},
                new Equipment{Name="Logitech Webcam", Type="Webcam", SerialNumber="LW77889", Condition=Condition.Fair, Status=EquipmentStatus.UnderRepair, Location="Conference Room", PhotoUrl="https://example.com/photos/logitech_webcam.jpg"},
                new Equipment{Name="Samsung Tablet", Type="Tablet", SerialNumber="ST99001", Condition=Condition.Good, Status=EquipmentStatus.Unavailable, Location="Reception", PhotoUrl="https://example.com/photos/samsung_tablet.jpg"}
            };
            foreach (Equipment s in equipment)
            {
                context.Equipment.Add(s);
            }
            context.SaveChanges();
        }
    }
}
