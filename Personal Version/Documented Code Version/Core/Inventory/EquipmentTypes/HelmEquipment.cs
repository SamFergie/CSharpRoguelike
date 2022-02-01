namespace Roguelike.Core.Inventory.EquipmentTypes {
    public class HelmEquipment : Equipment {

        //Returns default/empty helm equipment
        public static HelmEquipment None() {
            return new HelmEquipment {
                Name = "None"
            };
        }

    }
}
