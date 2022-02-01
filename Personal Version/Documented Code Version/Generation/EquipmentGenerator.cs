using Roguelike.Core;
using Roguelike.Core.Inventory;
using Roguelike.Core.Inventory.EquipmentTypes;
using Roguelike.Core.Inventory.Items;
using Roguelike.Utilities;
using System;

namespace Roguelike.Generation {
    public class EquipmentGenerator {

        //This method returns a randomly
        //generate piece of equipment
        public Equipment GenerateEquipment() {
            int type = Game.random.Next(1, 5);
            if (type == 1) {
                //Generate random melee weapon
                return GenerateMeleeWeapon();
            } else if (type == 2) {
                //Generate random ranged weapon
                return GenerateRangedWeapon();
            } else if (type == 3) {
                //Generate random armour
                return GenerateArmour();
            } else if (type == 4) {
                //Generate random item
                return GenerateItem();
            }
            return null;
        }

        //This method actually generates a random piece of armour
        private Equipment GenerateArmour() {
            //The variety of the armour
            ArmourVariety variety = GenerateArmourVariety();
            //The object to be returned
            Equipment toReturn = null;
            //The name of the armour starts with
            //the variety of the equipment
            string varietyName = variety.ToString();
            int type = 6;// Game.random.Next(1, 6);
            if (type == 1) {
                //Should return a new helm armour object
                toReturn = new HelmEquipment {
                    Name = varietyName + " Helm",
                };
            } else if (type == 2) {
                //Should return a new chest armour object
                toReturn = new ChestEquipment {
                    Name = varietyName + " Chestplate",
                };
            } else if (type == 3) {
                //Should return a new glove armour object
                toReturn = new GloveEquipment {
                    Name = varietyName + " Gloves",
                };
            } else if (type == 4) {
                //Should return a new leg armour object
                toReturn = new LegEquipment {
                    Name = varietyName + " Trousers",
                };
            } else if (type == 5) {
                //Should return a new boot armour object
                toReturn = new BootEquipment {
                    Name = varietyName + " Boots",
                };
            }else if(type == 6) {
                //Should return a new ring armour object;
                return GenerateRing();
            }
            //This gives a linear distribution to the armours so that
            //each variety of armour has +1 armour than the previous variety
            toReturn.ArmourRating = Convert.ToInt16(variety) + 1;
            toReturn.Variety = variety;
            return toReturn;
        }

        //This method actually generates a random ring
        private Equipment GenerateRing() {
            RingEquipment ring = new RingEquipment();
            ring.Variety = GenerateRingVariety();
            ring.Name = ring.Variety.ToString() + " Ring";
            int adv = Game.random.Next(1, 100);
            if(adv >= 31 && adv < 51) {
                //20% chance of giving +1 to armour
                ring.ArmourRating = 1;
                ring.Name += " of Shielding";
            }else if(adv >= 51 && adv < 71) {
                //20% chance of giving +1 to max health
                ring.MaxHealth = 1;
                ring.Name += " of Health";
            } else if(adv >= 71 && adv < 91) {
                //20% chance of giving +1 to strength
                ring.Strength = 1;
                ring.Name += " of Strength";
            } else if(adv >= 91 && adv < 94) {
                //3% chance of giving +2 to armour
                ring.ArmourRating = 2;
                ring.Name += " of Shielding";
                ring.Name = ring.Name.Insert(0, "Forged ");
            } else if(adv >= 94 && adv < 97) {
                //3% chance of giving +2 to max health
                ring.MaxHealth = 2;
                ring.Name += " of Health";
                ring.Name = ring.Name.Insert(0, "Forged ");
            } else if(adv >= 97) {
                //3% chance of giving +2 to strength
                ring.Strength = 2;
                ring.Name += " of Strength";
                ring.Name = ring.Name.Insert(0, "Forged ");
            }
            return ring;
        }

        //This method actually generates a random melee weapon
        private Equipment GenerateMeleeWeapon() {
            //The variety of the generated weapon 
            WeaponVariety variety = GenerateWeaponVariety();
            //The name of the weapon starts with
            //the variety of the weapon
            string name = variety.ToString();
            //The type of melee weapon
            int type = Game.random.Next(1, 5);
            //The damage is the 'tier' of weapon number of dice
            //that are sized so that its 2 * the variety
            string damageValue = type.ToString() + "D" + Convert.ToString(4 + 2 * Convert.ToInt16(variety));
            //Assigns the name based on
            //the type of melee weapon
            if(type == 1) {
                name += " Sword";
            }else if(type == 2) {
                name += " Longsword";
            } else if(type == 3) {
                name += " Mace";
            } else if(type == 4) {
                name += " Hammer";
            }
            //Creates the weapon object
            Equipment toReturn = new MeleeEquipment {
                Name = name,
                DamageRange = damageValue,
                Variety = variety
            };
            //Returns the weapon object
            return toReturn;
        }

        //This method actually generates a random ranged weapon
        private Equipment GenerateRangedWeapon() {
            WeaponVariety variety = GenerateWeaponVariety();
            //The name of the weapon starts with
            //the variety of the weapon
            string name = variety.ToString();
            //The type of ranged weapon
            int type = Game.random.Next(1, 4);
            //The damage is the 'tier' of weapon number of D6s
            string damageValue = Convert.ToInt16(variety + 1).ToString() + "D6";
            //Assigns the range which has a base of 4 and
            //each 'tier' of ranged weapon has an additional 2
            int range = (type * 3) + 3;
            //Assigns the name based on
            //the type of ranged weapon
            if(type == 1) {
                name += " Slingshot";
            }else if(type == 2) {
                name += " Bow";
            }else if(type == 3) {
                name += " Longbow";
            }
            Equipment toReturn = new RangedEquipment() {
                Name = name,
                DamageRange = damageValue,
                Range = range,
                Variety = variety
            };
            return toReturn;
        }

        //This method actually generates a random item
        private Equipment GenerateItem() {
            int type = Game.random.Next(1, 3);
            //Generates either a health potion or a scroll of fireball if new
            //items are added and could be generated, they should be added here
            if (type == 1) {
                //Each health potion restores 5 or 10 health
                return new HealthPotion(Game.random.Next(1, 3) * 5);
            } else {
                return new ScrollOfFireball();
            }
        }

        //This method generates a random armour variety
        private ArmourVariety GenerateArmourVariety() {
            //Generates random number 1-100(inclusive)
            int type = Game.random.Next(1, 101);
            if (type < 61) {
                //Gives a 60% chance of being leather
                return ArmourVariety.Leather;
            } else if (type >= 61 && type < 91) {
                //Gives a 30% chance of being hide
                return ArmourVariety.Hide;
            } else if (type >= 91 && type < 96) {
                //Gives a 5% chance of being chainmail
                return ArmourVariety.Chainmail;
            } else if (type >= 96 && type < 99) {
                //Gives a 3% chance of being plate
                return ArmourVariety.Plate;
            } else if (type >= 99) {
                //Gives a 2% chance of being dragonscale
                return ArmourVariety.Dragonscale;
            } else {
                //Should never happen but a catch-all
                //is used to stop errors occuring
                return ArmourVariety.Leather;
            }
        }

        //This method generates a random weapon variety
        private WeaponVariety GenerateWeaponVariety() {
            //Generates random number 1-100(inclusive)
            int type = Game.random.Next(1, 101);
            if (type < 61) {
                //Gives a 60% chance of being iron
                return WeaponVariety.Iron;
            } else if (type >= 61 && type < 91) {
                //Gives a 30% chance of being steel
                return WeaponVariety.Steel;
            } else if (type >= 91 && type < 99) {
                //Gives a 8% chance of being titanium
                return WeaponVariety.Titanium;
            } else if (type >= 99) {
                //Gives a 2% chance of being uru
                return WeaponVariety.Uru;
            } else {
                //Should never happen but a catch-all
                //is used to stop errors occuring
                return WeaponVariety.Iron;
            }
        }

        //This method generates a random ring variety
        private RingVariety GenerateRingVariety() {
            //Generates random number 1-100(inclusive)
            int type = Game.random.Next(1, 101);
            if (type < 61) {
                //Gives a 60% chance of being iron
                return RingVariety.Iron;
            } else if (type >= 61 && type < 91) {
                //Gives a 30% chance of being silver
                return RingVariety.Silver;
            } else if (type >= 91 && type < 99) {
                //Gives a 8% chance of being gold
                return RingVariety.Gold;
            } else if (type >= 99) {
                //Gives a 2% chance of being platinum
                return RingVariety.Platinum;
            } else {
                //Should never happen but a catch-all
                //is used to stop errors occuring
                return RingVariety.Iron;
            }
        }
    }
}