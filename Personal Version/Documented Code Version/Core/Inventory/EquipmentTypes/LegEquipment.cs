namespace Roguelike.Core.Inventory.EquipmentTypes {
    public class LegEquipment : Equipment {
        
        //Returns default/empty leg equipment
        public static LegEquipment None() {
            return new LegEquipment {
                Name = "None"
            };
        }

    }
}
