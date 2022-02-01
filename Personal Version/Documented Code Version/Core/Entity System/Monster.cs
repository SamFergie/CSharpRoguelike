using RLNET;
using Roguelike.Core.Entity_System.Behaviours;
using Roguelike.Systems;
using Roguelike.Utilities;
using RogueSharp;
using System;

namespace Roguelike.Core.Entity_System {
    public class Monster : Entity {

        //How long since alterted (can be null)
        public int? turnsAlterted { get; set; }
        //How large is monster
        public int monsterSize { get; set; }

        //This method contains the AI for each specific type of monster
        //and uses logic to determine which AI behaviour to execute
        public virtual void PerformAction(CommandSystem commandSystem, PathfindingSystem pathfindingSystem) {
            //Create a new standard behaviour and act upon it
            StandardMoveAndAttack behaviour = new StandardMoveAndAttack();
            behaviour.Act(this, commandSystem, pathfindingSystem);
        }

        //Draws the monster onto the provided
        //console from given map position
        public new void Draw(RLConsole console, IMap map) {
            //Checks if monster cell is explored
            bool explored = false;
            for (int x = 0; x < monsterSize; x++) {
                for (int y = 0; y < monsterSize; y++) {
                    if(map.GetCell(X + x, Y + y).IsExplored) {
                        explored = true;
                    }
                }
            }
            //The cell containing the monster hasn't
            //been explored so don't render anything
            if (!explored) {
                return; 
            }
            //Checks if monster is within the player's FOV
            bool visible = false;
            for (int x = 0; x < monsterSize; x++) {
                for (int y = 0; y < monsterSize; y++) {
                    if (map.GetCell(X + x, Y + y).IsInFov) {
                        visible = true;
                    }
                }
            }
            //If visible, monster is drawn to console
            //otherwise the floor is drawn to the console
            if (visible) {
                for (int x = 0; x < monsterSize; x++) {
                    for (int y = 0; y < monsterSize; y++) {
                        console.Set(X + x, Y + y, colour, DungeonColours.floorBackground, symbol);
                    }
                }
            } else {
                for (int x = 0; x < monsterSize; x++) {
                    for (int y = 0; y < monsterSize; y++) {
                        console.Set(X + x, Y + y, DungeonColours.floor, DungeonColours.floorBackground, '.');
                    }
                }
            }
        }

        //This method displays a health bar for the monster
        //and uses position to indicate how far down the health
        //bar should be displayed
        public void DrawStats(RLConsole console, int position) {
            //Determine y postion given a blank gap between each health bar
            int yPosition = 24 + (2 * position);

            //Prints out the symbol that represents the monster
            //in the colour it appears on the map as (to help the player)
            console.Print(1, yPosition, symbol.ToString(), colour);

            //Determines what % of health the monster has and therefore
            //what % of the total console the bar should occupy
            double healthRatio = (double)Health / (double)MaxHealth;
            int width = Convert.ToInt32(healthRatio * 35);
            int remainingWidth = 35 - width;

            //Sets the backing colour of each health bar with the total bar having a different colour
            console.SetBackColor(3, yPosition, width, 1, new RLColor(143, 136, 112));
            console.SetBackColor(3 + width, yPosition, remainingWidth, 1, new RLColor(86, 82, 67));

            //Displays the numerical amount of health left
            console.Print(2, yPosition, ": " + Name + ": " + Health + "/" + MaxHealth, RLColor.Blend(RLColor.White, RLColor.Black, 0.1f));
        }

    }
}
