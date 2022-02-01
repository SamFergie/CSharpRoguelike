using RLNET;
using Roguelike.Interfaces;
using Roguelike.Utilities;
using RogueSharp;

namespace Roguelike.Generation.Dungeon {
    public class Door : IDrawable {

        //The room that the door is attached to
        public Room room { get; set; }

        //Is the door open/closed or locked/unlocked
        public bool isOpen { get; set; }
        public bool isLocked { get; set; }

        //The colours of the room cell
        public RLColor colour { get ; set ; }
        public RLColor backgroundColour { get; set; }

        //The symbol and positions fo the room cell
        public char symbol { get ; set ; }
        public int X { get ; set ; }
        public int Y { get ; set ; }

        //This is the constructor for the door, takes in no
        //parameters since most/all doors will be the same
        public Door() {
            symbol = '+';
            colour = DungeonColours.doorColour;
            backgroundColour = DungeonColours.doorColourBackground;
        }

        //The draw method for the door, as required by the IDrawable interface
        public void Draw(RLConsole console, IMap map) {
            //The cell hasn't been explored yet so do not drawn anything
            if(!map.GetCell(X, Y).IsExplored) {
                return;
            }
            //Apply silver colours if the door is locked
            if (!isLocked) {
                //Apply lighter colours if the door is
                //within the player's FOV
                if (map.IsInFov(X, Y)) {
                    colour = DungeonColours.doorColourFov;
                    backgroundColour = DungeonColours.doorColourBackgroundFov;
                } else {
                    colour = DungeonColours.doorColour;
                    backgroundColour = DungeonColours.doorColourBackground;
                }
            } else {
                //Apply lighter colours if the door is
                //within the player's FOV
                if (map.IsInFov(X, Y)) {
                    colour = DungeonColours.doorLockedColourFov;
                    backgroundColour = DungeonColours.doorColourBackgroundFov;
                } else {
                    colour = DungeonColours.doorLockedColour;
                    backgroundColour = DungeonColours.doorColourBackground;
                }
            }

            //Sets the properties of cell on the console
            console.Set(X, Y, colour, backgroundColour, symbol);
        }
    }
}
