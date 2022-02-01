using Roguelike.Core;
using Roguelike.Utilities;
using RogueSharp;
using System;

namespace Roguelike.Generation.Dungeon {
    public class Corridor {

        //The 2 rooms that the corridor connects
        public Room room1;
        public Room room2;

        //Readonly information about the corridor, such as
        //direction, length and the specific points it connects
        public readonly Direction direction;
        public readonly Point to;
        public readonly Point from;
        public readonly int length;

        //This constructor creates a new corridor from given parameters, which are:
        //the 2 points the corridor connectes, the direction it goes in, the 2 rooms it connects,
        //the dungeon map that it appears on, and whether or not the corridor should be added to the rooms
        public Corridor(Point f, Point t, Direction dir, Room r1, Room r2, DungeonMap map, bool shouldAdd = true) {
            //Assigns the room properties from the parameters specified
            room1 = r1;
            room2 = r2;

            direction = dir;

            from = f;
            to = t;

            //Changes cell properties to create the actual corridors within the dungeon map
            if (direction == Direction.Up || direction == Direction.Down) {
                //This corridor is a vertical corridor so Y values will change
                //Length of corridor is positive difference between Y values
                length = Math.Abs(from.Y - to.Y);
                if (from.Y < to.Y) {
                    //The start position is higher than the end position
                    //so the corridor goes down the screen
                    for (int y = from.Y; y < to.Y; y++) {
                        map.SetCellProperties(from.X, y, true, true, false);
                    }
                } else {
                    //The end position is higher than the start position
                    //so the corridor goes up the screen. Needs to apply
                    //a vertical offset since Y counts from 0
                    for (int y = to.Y + 1; y < from.Y + 1; y++) {
                        map.SetCellProperties(from.X, y, true, true, false);
                    }
                }
            } else {
                //This corridor is a horizontal corridor so X values will change
                //Length of corridor is positive difference between X values
                length = Math.Abs(from.X - to.X);
                if (from.X < to.X) {
                    //The start position is to the left of the end position
                    //so the corridor goes from left to right
                    for (int x = from.X; x < to.X; x++) {
                        map.SetCellProperties(x, from.Y, true, true, false);
                    }
                } else {
                    //The end position is tom the left of the start position
                    //so the corridor goes from right to left. Needs to apply
                    //a horizontal offset since X counts from 0
                    for (int x = to.X + 1; x < from.X + 1; x++) {
                        map.SetCellProperties(x, from.Y, true, true, false);
                    }
                }
            }
            //If the corridor should be added to the rooms then
            //do so, the add corridor method isn't used both times
            //since in the second room the opposite direction must be removed
            if (shouldAdd) {
                room1.AddCorridor(this);
                room2.corridors.Add(this);
            }
            //Removes the opposite direction from the 2nd room
            if (direction == Direction.Left) {
                room2.RemoveOpenDirection(Direction.Right);
            }else if(direction == Direction.Right) {
                room2.RemoveOpenDirection(Direction.Left);
            } else if(direction == Direction.Down) {
                room2.RemoveOpenDirection(Direction.Up);
            } else if (direction == Direction.Up) {
                room2.RemoveOpenDirection(Direction.Down);
            }

        }
    }
}
