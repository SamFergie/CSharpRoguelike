using RLNET;
using Roguelike.Core.Entity_System;
using RogueSharp;
using System;
using System.Collections.Generic;

namespace Roguelike.Core.Inventory.Items {
    public class ScrollOfFireball : Item {

        //This stores the damage roll to use
        public string damageRange { get; private set; }

        //This stores the range to which
        //the scroll can damage enemies
        public int range { get; private set; }

        //This constructor takes no parameters and ensures
        //that all created scrolls will be the same
        public ScrollOfFireball() {
            //Sets properties
            Name = "Scroll of Fireball";
            isUseable = true;
            //The use message simply describes the player unwrapping the scroll and dealing damage
            useMessage = Game.player.Name + " unweapped the scroll and unleashed a whirlwind of fire!";
            //This gives a generic narrative description of what the item would look like
            previewMessage = "A dried piece of parchment bound tightly by a wax seal. Written centuries ago by ancient magicians.";

            //The scroll deals 3D6 damage (3-18)
            damageRange = "3D6";
            //Can affect monsters only 4 cells away
            range = 4;
        }

        //This is the overriden use method which damages 
        public override void Use() {
            //Displays the preview message and
            //removes from player's inventory
            base.Use();
            Cell playerCell = Game.dungeonMap.GetCell(Game.player.X, Game.player.Y);
            List<Monster> toKill = new List<Monster>();
            foreach (Monster monster in Game.dungeonMap.monsters) {
                //Calculate the distance between the player and the monster
                int dist = Convert.ToInt16(Math.Sqrt(Math.Pow((playerCell.X - monster.X), 2) + Math.Pow((playerCell.Y - monster.Y), 2)));
                if (dist <= range) {
                    //Monster is within range
                    monster.Health -= Game.diceSystem.Roll(damageRange);
                    if(monster.Health <= 0) {
                        //Killed the monster
                        toKill.Add(monster);
                    } else {
                        //Damaged the monster
                        Game.messageLog.Add("The fireball damaged " + monster.Name);
                    }
                }
            }
            //Use the death method from the command system
            //to kill all the monsters who have no health remaining
            foreach (Monster killed in toKill) {
                Game.commandSystem.Death(killed);
            }
        }

        //This is the overriden preview method
        public override void Preview(RLConsole console, int x) {
            int yPos = 1;
            //Shows a generic "Preview: " message at the top
            console.Print(x, yPos, "Preview: ", RLColor.White, RLColor.Black);
            yPos += 2;
            //Shows the range of damage the scroll can do
            console.Print(x, yPos, "Damage Range: " + damageRange, RLColor.Blend(RLColor.White, RLColor.Red, 0.5f), RLColor.Black);
            yPos += 2;
            //Shows the range to which the scroll can deal damage
            console.Print(x, yPos, "Range: " + range, RLColor.White, RLColor.Black);
            yPos += 2;
            //Adds a number of lines whilst
            //outputting the message to the console
            yPos += console.Print(x, yPos, previewMessage, RLColor.White, RLColor.Black, 30);
            //Change the background colour for the preview box
            console.SetBackColor(x, 1, 30, yPos - 1, RLColor.Blend(RLColor.Gray, RLColor.Black, 0.25f));
        }
    }
}
