using Roguelike.Utilities;

namespace Roguelike.Core.Inventory.EquipmentTypes {
    public class RingEquipment : Equipment {

        //Represents the rarity of the ring
        public new RingVariety Variety { get; set; }

        //Returns default/empty ring equipment
        public static RingEquipment None() {
            return new RingEquipment {
                Name = "None"
            };
        }

    }
}
