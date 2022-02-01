namespace Roguelike.Core.Inventory.EquipmentTypes {
    public class ChestEquipment : Equipment {

        //Returns default/empty chest equipment
        public static ChestEquipment None() {
            return new ChestEquipment {
                Name = "None"
            };
        }

    }
}
