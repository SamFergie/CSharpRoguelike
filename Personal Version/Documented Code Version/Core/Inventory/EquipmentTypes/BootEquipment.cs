namespace Roguelike.Core.Inventory.EquipmentTypes {
    public class BootEquipment : Equipment {

        //Returns default/empty boot equipment
        public static BootEquipment None() {
            return new BootEquipment {
                Name = "None"
            };
        }

    }
}
