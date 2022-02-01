using Roguelike.Core;
using Roguelike.Utilities;
using RogueSharp;
using System.Collections.Generic;

namespace Roguelike.Generation.Dungeon {
    public class Room {

        //The Rougesharp object that represents all the
        //cells in the room
        public Rectangle rectangle;

        //The 4 main corners of the room
        public Point topLeft;
        public Point topRight;
        public Point bottomLeft;
        public Point bottomRight;

        //The 4 midpoints of each wall
        public Point middleLeft;
        public Point middleRight;
        public Point middleBottom;
        public Point middleTop;

        //The centre/closest point to the centre
        public Point centre;

        //A list of all the directions from which a
        //corridor could go
        public List<Direction> openDirections;

        //A list of all corridors connecting to this room
        public List<Corridor> corridors;

        //Constructor for a Room object
        public Room() {

        }

        //Contructor for a Room object, takes 2 parameters
        //the rectangle about which the room is to be generated
        //and the map unpon which the room is to be placed
        public Room(Rectangle rect, DungeonMap map) {
            //Assigns key properties of the newly created room
            rectangle = rect;
            centre = rect.Center;

            //Sets the corners to match the room information
            topLeft = new Point(rect.Left, rect.Top);
            topRight = new Point(rect.Right, rect.Top);
            bottomLeft = new Point(rect.Left, rect.Bottom);
            bottomRight = new Point(rect.Right, rect.Bottom);

            //Sets the midpoints to match the room information
            middleTop = new Point((topLeft.X + topRight.X) / 2, topLeft.Y);
            middleBottom = new Point((bottomLeft.X + bottomRight.X) / 2, bottomLeft.Y);
            middleLeft = new Point(topLeft.X, (topLeft.Y + bottomLeft.Y) / 2);
            middleRight = new Point(topRight.X, (topRight.Y + bottomRight.Y) / 2);

            //Iterates through every cell inside the room and
            //sets the cell to be transparent and walkable yet unexplored
            for (int x = rect.Left + 1; x < rect.Right; x++) {
                for (int y = rect.Top + 1; y < rect.Bottom; y++) {
                    map.SetCellProperties(x, y, true, true, false);
                }
            }
            //Adds a random feature to the room (40% chance)
            AddRandomFeature(map);

            //Initialises the empty list of corridors
            corridors = new List<Corridor>();

            //Initialises the list of open directions, includes all
            //directions since the blank room has no corridors
            openDirections = new List<Direction>() { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        }

        //This method alters the size of the room to be equal
        //to a new rectangle. This could be needed for creating a
        //boss room or if an error occurs during map generation
        public void ResizeRoom(Rectangle newRect, DungeonMap map) {
            //Removes the old room by making all cells
            //in it, unwalkable, opaque and unexplored
            for (int x = rectangle.Left + 1; x < rectangle.Right; x++) {
                for (int y = rectangle.Top + 1; y < rectangle.Bottom; y++) {
                    map.SetCellProperties(x, y, false, false, false);
                }
            }

            //Assigns the new rect values
            rectangle = newRect;
            centre = rectangle.Center;

            //Updates the variables to match the new room
            topLeft = new Point(rectangle.Left, rectangle.Top);
            topRight = new Point(rectangle.Right, rectangle.Top);
            bottomLeft = new Point(rectangle.Left, rectangle.Bottom);
            bottomRight = new Point(rectangle.Right, rectangle.Bottom);

            middleTop = new Point((topLeft.X + topRight.X) / 2, topLeft.Y);
            middleBottom = new Point((bottomLeft.X + bottomRight.X) / 2, bottomLeft.Y);
            middleLeft = new Point(topLeft.X, (topLeft.Y + bottomLeft.Y) / 2);
            middleRight = new Point(topRight.X, (topRight.Y + bottomRight.Y) / 2);

            //Tunnels the room onto the dungeonMap by making some cells walkable
            for (int x = rectangle.Left + 1; x < rectangle.Right; x++) {
                for (int y = rectangle.Top + 1; y < rectangle.Bottom; y++) {
                    //Try catch statement needed incase some cells fall outside the map bounds
                    //this error is extremely unlikely to occur but better
                    //to err on the side of caution
                    try {
                        map.SetCellProperties(x, y, true, true, false);
                    } catch {
                    }
                }
            }
        }

        //This method checks if the room exceeds the bounds of the map
        //and then uses that value to determine if the room could be resized
        public bool CouldRoomBeResized(Rectangle newRect, DungeonMap map) {
            if (newRect.Left <= 0) {
                return false;
            }
            if (newRect.Right >= map.Width) {
                return false;
            }
            if (newRect.Top <= 0) {
                return false;
            }
            if (newRect.Bottom >= map.Height) {
                return false;
            }
            return true;
        }

        //This method is used to remove a direction from a room's
        //list of open directions, also validates that the room
        //has such a direction open before removing it
        public void RemoveOpenDirection(Direction direction) {
            if (openDirections.Contains(direction)) {
                openDirections.Remove(direction);
            }
        }

        //This method returns a randomly selected open direction
        //for use when randomly selecting a corridor direction to be added.
        public Direction RandomOpenDirection() {
            if(openDirections.Count > 0) {
                return openDirections[Game.random.Next(0, openDirections.Count)];
            } else {
                //No open directions were found so simply returns the none direction
                return Direction.None;
            }
        }

        //This method attempts to add a new corridor to the room, if 
        //the direction is already occupied, this corridor isn't added
        public void AddCorridor(Corridor newCorridor) {
            if (!openDirections.Contains(newCorridor.direction)) {
                return;
            }
            //The room can have a corridor going in the new direction
            //so add the corridor to the list and remove the direction
            //from the list of open directions
            corridors.Add(newCorridor);
            RemoveOpenDirection(newCorridor.direction);
        }

        //This method takes the current room object and gives
        //it a random feature, with a 40% chance
        public void AddRandomFeature(DungeonMap map) {
            if (Game.random.Next(1, 20) > 12) {
                if (rectangle.Width == rectangle.Height && (rectangle.Width % 2 == 0)) {
                    map.SetCellProperties(centre.X, centre.Y, false, false, false);
                } else if (rectangle.Width == rectangle.Height && (rectangle.Width % 2 != 0)) {
                    map.SetCellProperties(centre.X, centre.Y, false, false, false);
                    map.SetCellProperties(centre.X, centre.Y + 1, false, false, false);
                    map.SetCellProperties(centre.X + 1, centre.Y, false, false, false);
                    map.SetCellProperties(centre.X + 1, centre.Y + 1, false, false, false);
                } else if (rectangle.Width % 2 == 0 && rectangle.Height % 2 == 0) {
                    map.SetCellProperties(centre.X - 2, centre.Y - 2, false, false, false);
                    map.SetCellProperties(centre.X - 2, centre.Y + 2, false, false, false);
                    map.SetCellProperties(centre.X + 2, centre.Y - 2, false, false, false);
                    map.SetCellProperties(centre.X + 2, centre.Y + 2, false, false, false);
                }
            }
        }

    }
}
