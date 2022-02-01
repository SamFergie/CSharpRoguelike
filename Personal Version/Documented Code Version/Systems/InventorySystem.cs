using RLNET;
using Roguelike.Core;
using Roguelike.Core.Inventory;
using Roguelike.Core.Inventory.EquipmentTypes;
using System;
using System.Linq;

namespace Roguelike.Systems {
    public class InventorySystem {

        //This method equips or unequips the item
        //that the player clicked on
        public bool InventoryLeftClick(int x, int y) {
            if(x < 30) {
                //Clicked on a currently equipped item
                Equipment clickedOn = WhichInventoryItemClickedOn(x, y);
                //If the player clicked on a valid item
                //then it should be unequipped
                if(clickedOn != null) {
                    UnEquipItem(clickedOn);
                }
                return false;
            } else {
                //Clicked on an item within their general inventory
                int indexClickedOn = WhichIndexIsCoordinate(x, y);
                if(indexClickedOn == -1) {
                    return false;
                } else if(IsIndexValid(indexClickedOn, y)) {
                    //If the index is valid and the object type is of a type that
                    //means the player can equip it, then the player should do so
                    Equipment clickedOn = Game.player.Inventory.ElementAt(indexClickedOn);
                    if(clickedOn is Item) {
                        //The clicked on object is an item so that item should be used
                        (clickedOn as Item).Use();
                    } else {
                        //The clicked on object is a piece of equipment to equip it
                        EquipItem(clickedOn);
                    }
                }
            }
            return false;
        }

        //This method discards the item
        //that the player clicked on
        public bool InventoryRightClick(int x, int y) {
            //Find item they clicked on and discard it
            int indexClickedOn = WhichIndexIsCoordinate(x, y);
            //Validates they clicked on an index
            if(indexClickedOn == -1) {
                return false;
                //Validates that the index clicked on is valid
            } else if(IsIndexValid(indexClickedOn, x)) {
                //Returns the exact item clicked on and removes it
                Equipment clickedOn = Game.player.Inventory.ElementAt(indexClickedOn);
                //If the clicked on item isn't a key (or a
                //subtype of a key) then discard the item
                if (!clickedOn.Name.Contains("Key")) {
                    DiscardItem(clickedOn);
                }
            }
            return false;
        }

        //This method is called whenever the player could
        //have hovered over some item within their inventory
        public void InventoryPreview(int x, int y, RLConsole console) {
            //This is the equipment that the
            //player could be hovering over
            Equipment hoveredOver = null;
            //Is the cursor within the
            //player's current inventory
            if (x < 30) {
                //Find which item has been hovered over
                hoveredOver = WhichInventoryItemClickedOn(x, y);
            } else if (x >= 30) {
                int indexHoveredOver = WhichIndexIsCoordinate(x, y);
                if (indexHoveredOver == -1) {
                    //Hovering over nothing
                    return;
                } else if (IsIndexValid(indexHoveredOver, x)) {
                    //Find the inventory item 
                    //that is being hovered over
                    hoveredOver = Game.player.Inventory.ElementAt(indexHoveredOver);
                }
            }
            //Check the type of item/equipment
            //that is being hovered over
            if (hoveredOver is MeleeEquipment) {
                //Melee Weapon Preview
                MeleeWeaponPreview(x, console, hoveredOver as MeleeEquipment);
            } else if (hoveredOver is RangedEquipment) {
                //Ranged Weapon Preview
                RangedWeaponPreview(x, console, hoveredOver as RangedEquipment);
            } else if (hoveredOver is Item){
                //Item Preview
                ItemPreview(x, console, hoveredOver as Item);
            } else if(hoveredOver != null) {
                //Equipment Preview
                EquipmentPreview(x, console, hoveredOver);
            }
        }

        //This method generates and displays the preview box for a melee weapon
        private void MeleeWeaponPreview(int x, RLConsole console, MeleeEquipment toPreview) {
            //Validate that the equipment to be
            //previewed isn't a null value
            if(toPreview == null) {
                return;
            }
            //The position on the x-axis for the preview
            //box to be displayed
            int xPos = 30;
            //The height of the current line to be drawn
            int yPos = 1;
            //If the item is towards the left of the inventory
            //console then the preview window is to be towards the right
            //so set the x position towards the right of the console
            if(x < 70) {
                xPos = 70;
            }
            //Displays a preview message so the user knows what this is
            //also add a gap between messages (y+=2)
            console.Print(xPos, yPos, "Preview: ", RLColor.White, RLColor.Black);
            yPos += 2;
            //Displays the name of the item
            console.Print(xPos, yPos, toPreview.Name, RLColor.White, RLColor.Black);
            yPos += 2;
            //Displays the damage roll of the weapon so the user
            //can make decisions about which weapon to use
            console.Print(xPos, yPos, toPreview.DamageRange + " Damage Roll", RLColor.Blend(RLColor.Red, RLColor.White, 0.5f), RLColor.Black);
            //If the weapon has any bonus strength(in the case of a special weapon)
            //then increment y and display that as well
            if(toPreview.Strength != 0) {
                yPos += 2;
                console.Print(xPos, yPos, toPreview.Strength + " Strength", RLColor.Blend(RLColor.Blue, RLColor.White, 0.5f), RLColor.Black);
            }
            //Set the background colour for the whole preview window to a special black/gray colour
            console.SetBackColor(xPos, 1, 30, yPos, RLColor.Blend(RLColor.Gray, RLColor.Black, 0.25f));
        }
        
        //This method generates and displays the preview box for a ranged weapon
        private void RangedWeaponPreview(int x, RLConsole console, RangedEquipment toPreview) {
            //Validate that the equipment to be
            //previewed isn't a null value
            if (toPreview == null) {
                return;
            }
            //The position on the x-axis for the preview
            //box to be displayed
            int xPos = 30;
            //The height of the current line to be drawn
            int yPos = 1;
            //If the item is towards the left of the inventory
            //console then the preview window is to be towards the right
            //so set the x position towards the right of the console
            if (x < 70) {
                xPos = 70;
            }
            //Displays a preview message so the user knows what this is
            //also add a gap between messages (y+=2)
            console.Print(xPos, yPos, "Preview: ", RLColor.White, RLColor.Black);
            yPos += 2;
            //Displays the name of the item
            console.Print(xPos, yPos, toPreview.Name, RLColor.White, RLColor.Black);
            yPos += 2;
            //Displays the damage roll of the weapon so the user
            //can make decisions about which weapon to use
            console.Print(xPos, yPos, toPreview.DamageRange + " Damage Roll", RLColor.Blend(RLColor.Red, RLColor.White, 0.5f), RLColor.Black);
            yPos += 2;
            //Displays the range of the weapon so the user
            //can make decisions about which weapon to use
            console.Print(xPos, yPos, toPreview.Range + " Range", RLColor.Blend(RLColor.Green, RLColor.White, 0.5f), RLColor.Black);
            //If the weapon has any bonus strength(in the case of a special weapon)
            //then increment y and display that as well
            if (toPreview.Strength != 0) {
                yPos += 2;
                console.Print(xPos, yPos, toPreview.Strength + " Strength", RLColor.Blend(RLColor.Blue, RLColor.White, 0.5f), RLColor.Black);
            }
            //Set the background colour for the whole preview window to a special black/gray colour
            console.SetBackColor(xPos, 1, 30, yPos, RLColor.Blend(RLColor.Gray, RLColor.Black, 0.25f));
        }

        //This method generates and displays the preview box for a melee weapon
        private void EquipmentPreview(int x, RLConsole console, Equipment toPreview) {
            //Validate that the equipment to be
            //previewed isn't a null value
            if (toPreview == null) {
                return;
            }
            //The position on the x-axis for the preview
            //box to be displayed
            int xPos = 30;
            //The height of the current line to be drawn
            int yPos = 1;
            //If the equipment is towards the left of the inventory
            //console then the preview window is to be towards the right
            //so set the x position towards the right of the console
            if (x < 70) {
                xPos = 70;
            }
            //Displays a preview message so the user knows what this is
            //also add a gap between messages (y+=2)
            console.Print(xPos, yPos, "Preview: ", RLColor.White, RLColor.Black);
            yPos += 2;
            //Displays the name of the item
            console.Print(xPos, yPos, toPreview.Name, RLColor.White, RLColor.Black);
            //If the equipment has an armour rating
            //then increment y and display that as well
            if (toPreview.ArmourRating != 0) {
                yPos += 2;
                console.Print(xPos, yPos, toPreview.ArmourRating + " Armour", RLColor.Blend(RLColor.Green, RLColor.White), RLColor.Black);
            }
            //If the equipment has any bonus health
            //then increment y and display that as well
            if (toPreview.MaxHealth != 0) {
                yPos += 2;
                console.Print(xPos, yPos, toPreview.MaxHealth+ " Max. Health", RLColor.Blend(RLColor.Red, RLColor.White), RLColor.Black);
            }
            //If the equipment has any bonus strength
            //then increment y and display that as well
            if (toPreview.Strength != 0) {
                yPos += 2;
                console.Print(xPos, yPos, toPreview.Strength + " Strength", RLColor.Blend(RLColor.Blue, RLColor.White, 0.5f), RLColor.Black);
            }
            //Set the background colour for the whole preview window to a special black/gray colour
            console.SetBackColor(xPos, 1, 30, yPos, RLColor.Blend(RLColor.Gray, RLColor.Black, 0.25f));
        }

        //This method determines the X position for the preview window before
        //allowing the individual item to generate their preview content
        private void ItemPreview(int x, RLConsole console, Item toPreview) {
            //If no item is to be previewed then do nothing
            if(toPreview == null) {
                return;
            }
            //Determine the X position for the box to be displayed
            int xPos = 30;
            if(x < 70) {
                xPos = 70;
            }
            //The item has to make its preview window since different
            //items could need different preview window formats
            toPreview.Preview(console, xPos);
        }

        //This method goes through each of the currently equipped items
        //and determines which item the player clicked on
        private Equipment WhichInventoryItemClickedOn(int x, int y) {
            if(y == 1) {
                if(x > 6 && x < 7 + Game.player.Helm.Name.Length && Game.player.Helm.Name != "None") {
                    return Game.player.Helm;
                }
            } else if (y == 3) {
                if (x > 7 && x < 8 + Game.player.Chest.Name.Length && Game.player.Chest.Name != "None") {
                    return Game.player.Chest;
                }
            } else if (y == 5) {
                if (x > 8 && x < 9 + Game.player.Gloves.Name.Length && Game.player.Gloves.Name != "None") {
                    return Game.player.Gloves;
                }
            } else if (y == 7) {
                if (x > 6 && x < 7 + Game.player.Legs.Name.Length && Game.player.Legs.Name != "None") {
                    return Game.player.Legs;
                }
            } else if (y == 9) {
                if (x > 7 && x < 8 + Game.player.Boots.Name.Length && Game.player.Boots.Name != "None") {
                    return Game.player.Boots;
                }
            } else if (y == 11) {
                if (x > 6 && x < 7 + Game.player.Ring.Name.Length && Game.player.Ring.Name != "None") {
                    return Game.player.Ring;
                }
            } else if (y == 13) {
                if (x > 6 && x < 7 + Game.player.Weapon.Name.Length && Game.player.Weapon.Name != "None") {
                    return Game.player.Weapon;
                }
            }
            //Clicked on some other area of
            //the inventory so return null
            return null;
        }

        //This method takes a piece of
        //equipment and attempts equips it
        public void EquipItem(Equipment toEquip) {
            //The equipment the player currently has equipped
            //in the relevant slot is held in this variable
            Equipment tempEquipment = null;
            //Determines the type of equipment that
            //the player wants to equip
            if(toEquip is HelmEquipment) {
                tempEquipment = Game.player.Helm;
                Game.player.Helm = toEquip as HelmEquipment;
            }else if (toEquip is ChestEquipment) {
                tempEquipment = Game.player.Chest;
                Game.player.Chest = toEquip as ChestEquipment;
            } else if (toEquip is GloveEquipment) {
                tempEquipment = Game.player.Gloves;
                Game.player.Gloves = toEquip as GloveEquipment;
            } else if (toEquip is LegEquipment) {
                tempEquipment = Game.player.Legs;
                Game.player.Legs = toEquip as LegEquipment;
            } else if (toEquip is BootEquipment) {
                tempEquipment = Game.player.Boots;
                Game.player.Boots = toEquip as BootEquipment;
            } else if (toEquip is RingEquipment) {
                tempEquipment = Game.player.Ring;
                Game.player.Ring = toEquip as RingEquipment;
            } else if (toEquip is WeaponEquipment) {
                tempEquipment = Game.player.Weapon;
                Game.player.Weapon = toEquip as WeaponEquipment;
            }
            //If a piece of equipment has been changed,
            //remove it from the player's inventory
            if(tempEquipment != null) {
                Game.player.Inventory.Remove(toEquip);
                //If the item unequiped wasn't called "None"
                //then it is added to the player's inventory
                if(tempEquipment.Name != "None") {
                    Game.player.Inventory.Add(tempEquipment);
                }
            }
        }

        //This method removes a currently equipped
        //item and places in into their inventory
        public void UnEquipItem(Equipment toUnequip) {
            //If inventory is full, 
            //then don't do anything
            if(Game.player.Inventory.Count >= 27) {
                return;
            } else {
                //Add to inventory
                Game.player.Inventory.Add(toUnequip);
            }
            if(toUnequip is HelmEquipment) {
                Game.player.Helm = HelmEquipment.None();
            } else if (toUnequip is ChestEquipment) {
                Game.player.Chest = ChestEquipment.None();
            } else if (toUnequip is GloveEquipment) {
                Game.player.Gloves = GloveEquipment.None();
            } else if (toUnequip is LegEquipment) {
                Game.player.Legs = LegEquipment.None();
            } else if (toUnequip is BootEquipment) {
                Game.player.Boots = BootEquipment.None();
            } else if (toUnequip is RingEquipment) {
                Game.player.Ring = RingEquipment.None();
            } else if (toUnequip is WeaponEquipment) {
                Game.player.Weapon = WeaponEquipment.None();
            }
        }

        //This method removes a specified item
        //from the player's inventory
        public void DiscardItem(Equipment toDiscard) {
            //Validates that the player's inventory before the
            //item is removed, this stops a null reference exception
            if (Game.player.Inventory.Contains(toDiscard)) {
                Game.player.Inventory.Remove(toDiscard);
            }
        }

        //This method finds which index of the player's
        //inventory has been clicked on 
        private int WhichIndexIsCoordinate(int x, int y) {
            int index = -1;
            if(x >= 30 && x <= 48) {
                index = Convert.ToInt16(Math.Floor((double)y / 2));
            }else if(x >= 50 && x <= 68) {
                index = Convert.ToInt16(Math.Floor((double)y / 2)) + 9;
            } else if(x >= 70 && x <= 88) {
                index = Convert.ToInt16(Math.Floor((double)y / 2)) + 18;
            }
            return index;
        }

        //This method determines if a given
        //index has actually been clicked on
        private bool IsIndexValid(int index, int x) {
            //Does the player have [index] number of items
            //in their inventory?
            if(Game.player.Inventory.Count > index) {
                if(Game.player.Inventory[index].Name.Length > 19) {
                    return true;
                }
                int column = WhichColumnClickedOn(x);
                int relativeX = -1;
                if(column == 1) {
                    relativeX = x - 30;
                }else if(column == 2) {
                    relativeX = x - 50;
                }else if(column == 3) {
                    relativeX = x - 70;
                }
                if(relativeX < Game.player.Inventory[index].Name.Length) {
                    return true;
                }
                return false;
            } else {
                //Player doesn't have [index] number
                //of items so the value is invalid
                return false;
            }
        }

        //This method determines which column 
        //was clicked on based of a specified X value
        private int WhichColumnClickedOn(int x) {
            if(x >= 30 && x <= 48) {
                return 1;
            }else if(x >= 50 && x <= 68) {
                return 2;
            }else if(x >= 70 && x <= 88) {
                return 3;
            }
            //Return default/null value
            return -1;
        }
    }
}
