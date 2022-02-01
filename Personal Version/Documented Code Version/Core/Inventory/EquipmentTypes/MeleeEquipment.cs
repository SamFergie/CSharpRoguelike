namespace Roguelike.Core.Inventory.EquipmentTypes {
    public class MeleeEquipment : WeaponEquipment {

        //Returns default/empty melee equipment
        public static new MeleeEquipment None() {
            return new MeleeEquipment {
                Name = "None",
                DamageRange = "1D1"
            };
        }

    }
}
