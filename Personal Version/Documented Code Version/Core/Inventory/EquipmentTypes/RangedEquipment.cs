namespace Roguelike.Core.Inventory.EquipmentTypes {
    public class RangedEquipment : WeaponEquipment {

        //Represents the range the weapon has
        public int Range { get; set; }

        //Returns default/empty ranged equipment
        public static new RangedEquipment None() {
            return new RangedEquipment {
                Name = "None",
                DamageRange = "1D1",
                Range = 0
            };
        }

    }
}
