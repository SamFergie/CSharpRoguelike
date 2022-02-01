namespace Roguelike.Core.Inventory.EquipmentTypes {
    public class GloveEquipment : Equipment {

        //Returns default/empty glove equipment
        public static GloveEquipment None() {
            return new GloveEquipment {
                Name = "None"
            };
        }

    }
}
