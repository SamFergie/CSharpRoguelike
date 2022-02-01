using RLNET;
using Roguelike.Core.Inventory.EquipmentTypes;
using Roguelike.Utilities;

namespace Roguelike.Core.Entity_System {
    public class Player : Entity {

        //This is the constructor for a Player
        //entity. The player has an FOV of 15,
        //15 gold, name "Rogue" etc...
        public Player() {
            Awareness = 15;
            Gold = 15;
            Name = "Rogue";

            ArmourRating = 6;
            Strength = 16;
            Constitution = 14;
            Intelligence = 12;
            Dexterity = 11;

            MaxHealth = 4 + Strength;
            Health = MaxHealth;
            Speed = 5;

            //This is the visual information needed to draw
            //the player onto the screen
            colour = Palette.Player;
            symbol = '@';
            X = 57;
            Y = 50;

            //Equips items into the players
            //specific inventory slots
            Equip();
        }

        //This method draws the player's stats onto the
        //provided console (passed as paramater)
        public void DrawStats(RLConsole console) {
            console.Clear();

            //Displays misc. player information
            console.Print(1, 1, "Name: " + Name, RLColor.White);
            console.Print(1, 3, "Health: " + Health + "/" + MaxHealth, RLColor.White);
            console.Print(1, 5, "Armour: " + ArmourRating, RLColor.White);
            console.Print(1, 7, "Gold: " + Gold, RLColor.Yellow);

            //Displays stats
            console.Print(1, 10, "Stats: ", RLColor.White);
            console.Print(1, 12, "Strength: " + Strength, RLColor.Blend(RLColor.Blue, RLColor.White, 0.5f));
            console.Print(1, 14, "Dexterity: " + Dexterity, RLColor.Blend(RLColor.Green, RLColor.White, 0.5f));
            console.Print(1, 16, "Constitution: " + Constitution, RLColor.Blend(RLColor.Magenta, RLColor.White, 0.5f));
            console.Print(1, 18, "Intelligence: " + Intelligence, RLColor.Blend(RLColor.Blend(RLColor.Red, RLColor.Yellow, 0.5f), RLColor.White, 0.5f));
        }

        //This method draws the players currently equipped
        //items onto the console which is the first parameter
        public void DrawEquipment(RLConsole console) {
            if(Helm.Name.Length > 23) {
                console.Print(1, 1, "Helm: " + Helm.Name.Substring(0, 19) + "...", RLColor.White);
            } else {
                console.Print(1, 1, "Helm: " + Helm.Name, RLColor.White);
            }
            if (Chest.Name.Length > 22) {
                console.Print(1, 3, "Chest: " + Chest.Name.Substring(0, 19) + "...", RLColor.White);
            } else {
                console.Print(1, 3, "Chest: " + Chest.Name, RLColor.White);
            }
            if (Gloves.Name.Length > 21) {
                console.Print(1, 5, "Gloves: " + Gloves.Name.Substring(0, 19) + "...", RLColor.White);
            } else {
                console.Print(1, 5, "Gloves: " + Gloves.Name, RLColor.White);
            }
            if (Legs.Name.Length > 23) {
                console.Print(1, 7, "Legs: " + Legs.Name.Substring(0, 19) + "...", RLColor.White);
            } else {
                console.Print(1, 7, "Legs: " + Legs.Name, RLColor.White);
            }
            if (Boots.Name.Length > 22) {
                console.Print(1, 9, "Boots: " + Boots.Name.Substring(0, 19) + "...", RLColor.White);
            } else {
                console.Print(1, 9, "Boots: " + Boots.Name, RLColor.White);
            }
            if (Ring.Name.Length > 22) {
                console.Print(1, 11, "Ring: " + Ring.Name.Substring(0, 19) + "...", RLColor.White);
            } else {
                console.Print(1, 11, "Ring: " + Ring.Name, RLColor.White);
            }
            if (Weapon.Name.Length > 20) {
                console.Print(1, 13, "Weapon: " + Weapon.Name.Substring(0, 19) + "...", RLColor.White);
            } else {
                console.Print(1, 13, "Weapon: " + Weapon.Name, RLColor.White);
            }
        }

        //This method draws the whole of the players inventory
        //onto the console specified by the first parameter
        public void DrawInventory(RLConsole console) {
            int i = 0;
            //Validates there is an item in the
            //player's inventory before continuing
            if(Inventory.Count > i) {
                //Iterates through the x values, skipping every 20 columns
                for (int x = 30; x < 80; x += 20) {
                    //Iterates through the y values, skipping 1 row
                    for (int y = 1; y < 19; y += 2) {
                        //If the length of the item's name would intersect the adjacent item
                        //then the displayed name is restricted and followed by a "..."
                        if(Inventory[i].Name.Length > 19) {
                            //Prints out most of the items name followed by "..." 
                            //to indicate that some of the name is hidden
                            console.Print(x, y, Inventory[i].Name.Substring(0, 16) + "...", RLColor.White);
                        } else {
                            console.Print(x, y, Inventory[i].Name, RLColor.White);
                        }
                        //Validates that the player's inventory
                        //contains a next item before incrementing i
                        if(Inventory.Count > (i + 1)) {
                            i++;
                        } else {
                            return;
                        }
                    }
                }
            }
        }

        //This method adds default inventory items
        //into the player's specific inventory slots
        private void Equip() {
            Helm = new HelmEquipment {
                ArmourRating = 1,
                Name = "Leather Helm",
                Variety = ArmourVariety.Leather
            };
            Chest = new ChestEquipment {
                ArmourRating = 1,
                Name = "Leather Chestplate",
                Variety = ArmourVariety.Leather
            };
            Gloves = new GloveEquipment {
                ArmourRating = 1,
                Name = "Leather Gloves",
                Variety = ArmourVariety.Leather
            };
            Legs = new LegEquipment {
                ArmourRating = 1,
                Name = "Leather Trousers",
                Variety = ArmourVariety.Leather
            };
            Boots = new BootEquipment{
                ArmourRating = 1,
                Name = "Leather Boots",
                Variety = ArmourVariety.Leather
            };
            Weapon = new MeleeEquipment {
                Name = "Iron Longsword",
                DamageRange = "2D4",
                Variety = WeaponVariety.Iron
            };
        }

    }
}