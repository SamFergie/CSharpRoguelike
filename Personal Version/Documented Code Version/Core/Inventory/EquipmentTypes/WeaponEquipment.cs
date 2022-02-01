using Roguelike.Utilities;

namespace Roguelike.Core.Inventory.EquipmentTypes {
    public class WeaponEquipment : Equipment {

        //Represents the range of damage the weapon can do
        public string DamageRange { get; set; }
        //Represents the rarity of the weapon
        public new WeaponVariety Variety { get; set; }

        //Returns default/empty weapon equipment
        public static WeaponEquipment None() {
            return new WeaponEquipment() {
                Name = "None",
                DamageRange = "1D1"
            };
        }

    }
}
