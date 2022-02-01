using Roguelike.Core;
using Roguelike.Core.Entity_System;
using Roguelike.Core.Entity_System.Monsters.Bosses;
using Roguelike.Core.Entity_System.Monsters.Goblins;
using Roguelike.Core.Entity_System.Monsters.Kobolds;
using Roguelike.Core.Inventory.Items;
using Roguelike.Generation;
using Roguelike.Generation.Dungeon;
using Roguelike.Utilities;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougelike.Generation.Dungeon {
    class DungeonGenerator {

        //Private random number generator created
        //to reduce dependancy on the game class 
        private Random rand;

        //This is the width and height of a dungeon map
        //once set, these cannot be changed
        private readonly int width;
        private readonly int height;

        //This is the min and max width of a room
        private readonly int roomMinWidth;
        private readonly int roomMaxWidth;

        //This is the min and max height of a room
        private readonly int roomMinHeight;
        private readonly int roomMaxHeight;

        //This is the width and height of a boss room
        private readonly int bossRoomWidth;
        private readonly int bossRoomHeight;

        //This is the min and max lengths of a corridor
        private readonly int corridorMinLength;
        private readonly int corridorMaxLength;

        //This is the dungeon map that the class will modify
        //until the final map has been made
        private readonly DungeonMap map;

        //Lists to store every room and corridor in the map
        private List<Room> rooms = new List<Room>();
        private List<Corridor> corridors = new List<Corridor>();

        private Room bossRoom;

        //This is the constructor for a dungeon generator, parameters refer to the max/min
        //width and heights of rooms, corridors and boss rooms
        public DungeonGenerator(int w, int h, int rMinW, int rMinH, int rMaxW, int rMaxH, int bRW, int bRH) {
            //Assigns the random number generator to reduce  
            //dependancy upon the main game class
            rand = Game.random;

            //Assigns the width and height properties
            //for rooms and corridors
            width = w;
            height = h;

            roomMinWidth = rMinW;
            roomMinHeight = rMinH;

            roomMaxWidth = rMaxW;
            roomMaxHeight = rMaxH;

            bossRoomWidth = bRW;
            bossRoomHeight = bRH;

            corridorMinLength = Math.Max(roomMaxWidth, roomMaxHeight) - 7;
            corridorMaxLength = Convert.ToInt32(corridorMinLength * 1.25f);

            //Sets the lists of rooms and
            //corridors to be empty lists
            rooms = new List<Room>();
            corridors = new List<Corridor>();

            //Sets the boss room to null since
            //there is currently no boss room
            bossRoom = null;

            //Initialises the dungeon map
            map = new DungeonMap();
        }

        //This is the method called to start creation
        //of a new DungeonMap
        public DungeonMap CreateMap(EquipmentGenerator equipmentGenerator) {
            //Sets up the map with a given number of cells
            map.Initialize(width, height);

            //Goes through each cell and sets each cell to unwalkable,
            //unexplored, and opaque
            foreach(Cell cell in map.GetAllCells()) {
                map.SetCellProperties(cell.X, cell.Y, false, false, false);
            }

            //Starts the map generation process
            CreateDungeon();

            //Finds a suitable room that could be a boss room
            Room potentialRoom = FurthestRoomWithOneConnection();

            //Resizes that room and locks the door
            ResizeRoom(potentialRoom);

            //Place the player onto the game map and move them to the centre of the map
            PlacePlayer();

            //Place monsters onto map
            PlaceMonsters(equipmentGenerator);

            //Place a set of stairs onto the map
            PlaceStairs();

            //Returns the map as modifed
            return map;
        }

        //This private method creates the initial room and starts the overall dungeon generation process
        private void CreateDungeon() {
            //Selects the dimensions for the first room, which is always square
            int roomSize = rand.Next(roomMinWidth, roomMaxWidth);

            //Top left corner of the room means the room is always in the centre of the map
            Point point = new Point((width / 2) - (roomSize / 2), height / 2 - (roomSize / 2));
            Room room = CreateRoom(new Rectangle(point.X, point.Y, roomSize, roomSize));

            //Pass the next stages of the dungeon generation to a new method for code separation
            ExpandDungeon();
        }

        //This private method selects a random open room and attempts
        //to expand the map from there, stops after 400 tries
        private void ExpandDungeon() {
            bool isFull = false;
            int tries = 0;
            while (isFull == false && tries < 150) {
                Room toExpandFrom = GetRandomRoomWithOpenDirection();
                if (toExpandFrom == null) {
                    //The map is full, so stop
                    isFull = true;
                } else {
                    //Attempt to expand the dungeon from
                    //the randomly selected free room
                    ExpandFromRoom(toExpandFrom);
                }
                tries++;
            }
        }

        //This method is responsible for determining if a room can be expanded from,
        //and then carrying out that expansion
        public bool ExpandFromRoom(Room toExpandFrom) {
            //This stage of the method checks if the room can be expanded from
            bool canExpand = false;
            int t = 0;
            int variation = 0;
            Direction toExpandRoomFrom = Direction.None;
            while (canExpand == false && t < 12) {
                toExpandRoomFrom = toExpandFrom.RandomOpenDirection();
                if (toExpandRoomFrom == Direction.None) {
                    return false;
                }
                //Checks if the selected direction could be expanded from
                int radius = Math.Max(roomMaxWidth + corridorMaxLength + 1, roomMaxHeight + corridorMaxLength + 1);
                Tuple<bool, int> expansionValues = CanPointBeExpandedFrom(toExpandFrom, toExpandRoomFrom, radius);
                if (expansionValues.Item1) {
                    canExpand = true;
                    variation = expansionValues.Item2;
                }
                t++;
            }
            //The room has been checked 12 times and no valid expansion has been found
            if (t == 12) {
                return false;
            }

            //Determines a corridor length and randomly selects if
            //the room will be bent or not
            int corridorLength = rand.Next(corridorMinLength, corridorMaxLength);
            bool bent = false;

            if (corridorLength > 4 && Game.random.Next(0, 100) > 70) {
                bent = true;
            }

            //Determines the width and height of the new room
            int roomWidth = rand.Next(roomMinWidth, roomMaxWidth);
            int roomHeight = rand.Next(roomMinHeight, roomMaxHeight);

            //The new corridor will not be bent so only
            //1 straight corridor will be generated
            if (!bent) {
                Room newRoom = new Room();
                Point expandFrom = new Point();
                Point expandTo = new Point();
                //Create a new point which is the start of the corridor,
                //a point which is the end of the corridor and a room
                //which occurs at the end of that corridor
                if (toExpandRoomFrom == Direction.Left) {
                    expandFrom = new Point(toExpandFrom.middleLeft.X, toExpandFrom.centre.Y + variation);
                    expandTo = new Point(toExpandFrom.middleLeft.X - corridorLength, toExpandFrom.middleLeft.Y + variation);
                    newRoom = CreateRoom(CreateRectangleFromPoint(expandTo, Direction.Left, roomWidth, roomHeight));
                    CreateDoor(new Point(expandTo.X + 1, expandTo.Y), newRoom);
                } else if (toExpandRoomFrom == Direction.Right) {
                    expandFrom = new Point(toExpandFrom.middleRight.X, toExpandFrom.centre.Y + variation);
                    expandTo = new Point(toExpandFrom.middleRight.X + corridorLength, toExpandFrom.centre.Y + variation);
                    newRoom = CreateRoom(CreateRectangleFromPoint(expandTo, Direction.Right, roomWidth, roomHeight));
                    CreateDoor(new Point(expandTo.X - 1, expandTo.Y), newRoom);
                } else if (toExpandRoomFrom == Direction.Up) {
                    expandFrom = new Point(toExpandFrom.middleTop.X + variation, toExpandFrom.middleTop.Y);
                    expandTo = new Point(toExpandFrom.middleTop.X + variation, toExpandFrom.middleTop.Y - corridorLength);
                    newRoom = CreateRoom(CreateRectangleFromPoint(expandTo, Direction.Up, roomWidth, roomHeight));
                    CreateDoor(new Point(expandTo.X, expandTo.Y + 1), newRoom);
                } else if (toExpandRoomFrom == Direction.Down) {
                    expandFrom = new Point(toExpandFrom.middleBottom.X + variation, toExpandFrom.middleBottom.Y);
                    expandTo = new Point(toExpandFrom.middleBottom.X + variation, toExpandFrom.middleBottom.Y + corridorLength);
                    newRoom = CreateRoom(CreateRectangleFromPoint(expandTo, Direction.Down, roomWidth, roomHeight));
                    CreateDoor(new Point(expandTo.X, expandTo.Y - 1), newRoom);
                }
                //Create the corridor using the 2 new points along
                //with a door at the end of the corridor
                CreateCorridor(expandFrom, expandTo, toExpandRoomFrom, toExpandFrom, newRoom, map);
                CreateDoor(expandFrom, toExpandFrom);
            } else {
                //Creates a series of empty objects that
                //are necessary for the new corridor
                Room newRoom = new Room();
                Point expandFrom = new Point();
                Point expandTo = new Point();

                Point midPoint = new Point();
                Point bendPoint = new Point();

                int corridorZag = RandomVariationNonZero(1);
                //Create a new point which is the start of the corridor, a point which
                //is the end of the corridor and a room which occurs at the end
                //of that corridor. Also creates 2 points near the middle
                //of the corridor which the corridor will bend
                if (toExpandRoomFrom == Direction.Left) {
                    expandFrom = new Point(toExpandFrom.middleLeft.X, toExpandFrom.centre.Y + variation);
                    midPoint = new Point(Convert.ToInt16(toExpandFrom.middleLeft.X - Math.Ceiling((double)corridorLength / 2)), toExpandFrom.centre.Y + variation);
                    bendPoint = new Point(Convert.ToInt16(toExpandFrom.middleLeft.X - Math.Ceiling((double)corridorLength / 2) + 1), toExpandFrom.centre.Y + variation + corridorZag);
                    expandTo = new Point(toExpandFrom.middleLeft.X - corridorLength, toExpandFrom.middleLeft.Y + variation + corridorZag);
                    newRoom = CreateRoom(CreateRectangleFromPoint(expandTo, Direction.Left, roomWidth, roomHeight));
                    CreateDoor(new Point(expandTo.X + 1, expandTo.Y), newRoom);
                } else if (toExpandRoomFrom == Direction.Right) {
                    expandFrom = new Point(toExpandFrom.middleRight.X, toExpandFrom.centre.Y + variation);
                    midPoint = new Point(Convert.ToInt16(toExpandFrom.middleRight.X + Math.Ceiling((double)corridorLength / 2) + 1), toExpandFrom.centre.Y + variation);
                    bendPoint = new Point(Convert.ToInt16(toExpandFrom.middleRight.X + Math.Ceiling((double)corridorLength / 2)), toExpandFrom.centre.Y + variation + corridorZag);
                    expandTo = new Point(toExpandFrom.middleRight.X + corridorLength, toExpandFrom.centre.Y + variation + corridorZag);
                    newRoom = CreateRoom(CreateRectangleFromPoint(expandTo, Direction.Right, roomWidth, roomHeight));
                    CreateDoor(new Point(expandTo.X - 1, expandTo.Y), newRoom);
                } else if (toExpandRoomFrom == Direction.Up) {
                    expandFrom = new Point(toExpandFrom.middleTop.X + variation, toExpandFrom.middleTop.Y);
                    midPoint = new Point(toExpandFrom.middleTop.X + variation, toExpandFrom.middleTop.Y - Convert.ToInt16(Math.Ceiling((double)corridorLength / 2)));
                    bendPoint = new Point(toExpandFrom.middleTop.X + variation + corridorZag, Convert.ToInt16(toExpandFrom.middleTop.Y - Math.Ceiling((double)corridorLength / 2) + 1));
                    expandTo = new Point(toExpandFrom.middleTop.X + variation + corridorZag, toExpandFrom.middleTop.Y - corridorLength);
                    newRoom = CreateRoom(CreateRectangleFromPoint(expandTo, Direction.Up, roomWidth, roomHeight));
                    CreateDoor(new Point(expandTo.X, expandTo.Y + 1), newRoom);
                } else if (toExpandRoomFrom == Direction.Down) {
                    expandFrom = new Point(toExpandFrom.middleBottom.X + variation, toExpandFrom.middleBottom.Y);
                    midPoint = new Point(toExpandFrom.middleBottom.X + variation, Convert.ToInt16(toExpandFrom.middleBottom.Y + Math.Ceiling((double)corridorLength / 2)));
                    bendPoint = new Point(toExpandFrom.middleBottom.X + variation + corridorZag, Convert.ToInt16(toExpandFrom.middleBottom.Y + Math.Ceiling((double)corridorLength / 2) - 1));
                    expandTo = new Point(toExpandFrom.middleBottom.X + variation + corridorZag, toExpandFrom.middleBottom.Y + corridorLength);
                    newRoom = CreateRoom(CreateRectangleFromPoint(expandTo, Direction.Down, roomWidth, roomHeight));
                    CreateDoor(new Point(expandTo.X, expandTo.Y - 1), newRoom);
                }
                //Creates the 2 corridors, since a single
                //corridor cannot bend in the middle
                //along with the 2nd door
                CreateCorridor(expandFrom, midPoint, toExpandRoomFrom, toExpandFrom, newRoom, map);
                CreateCorridor(bendPoint, expandTo, toExpandRoomFrom, toExpandFrom, newRoom, map, false);
                CreateDoor(expandFrom, toExpandFrom);
            }
            //If the code made it this far then the expansion was a success
            return true;
        }

        #region Create A Feature

        //This method takes in a rectangle, uses it to make
        //a new room and adds the new room to the map
        private Room CreateRoom(Rectangle roomToCreate) {
            Room room = new Room(roomToCreate, map);
            rooms.Add(room);
            return room;
        }

        //This method takes in a point, direction and dimensional values
        //and uses those to generate a rectangle of the specified parameters
        private Rectangle CreateRectangleFromPoint(Point toCreateFrom, Direction direction, int width, int height) {
            int xCoord = 0;
            int yCoord = 0;

            //The point the room is created from is the midpoint of
            //the opposing direction's wall
            if (direction == Direction.Down) {
                xCoord = toCreateFrom.X - (width / 2);
                yCoord = toCreateFrom.Y - 1;
            } else if (direction == Direction.Up) {
                xCoord = toCreateFrom.X - (width / 2);
                yCoord = toCreateFrom.Y - height + 1;
            } else if (direction == Direction.Left) {
                xCoord = toCreateFrom.X - width + 1;
                yCoord = toCreateFrom.Y - (height / 2);
            } else if (direction == Direction.Right) {
                xCoord = toCreateFrom.X - 1;
                yCoord = toCreateFrom.Y - (height / 2);
            }

            Rectangle rect = new Rectangle(xCoord, yCoord, width, height);
            return rect;
        }

        //This method creates a corridor from the given parameters, it also
        //adds the newly created corridor to the list of corridors
        private Corridor CreateCorridor(Point to, Point from, Direction direction, Room room1, Room room2, DungeonMap dungeonMap, bool shouldAdd = true) {
            Corridor corridor = new Corridor(to, from, direction, room1, room2, dungeonMap, shouldAdd);
            corridors.Add(corridor);
            return corridor;
        }

        //This method takes in a point and (optionally) a room
        //and adds a door at that point
        private void CreateDoor(Point toCreateAt, Room toCreateAdjacentTo = null) {
            //If the door is next to the boss room then
            //the door should be locked.
            if (toCreateAdjacentTo.rectangle.Width != bossRoomWidth) {
                //Alter the properties so that the
                //player can walk on the door cell
                map.SetCellProperties(toCreateAt.X, toCreateAt.Y, false, true, false);
                Door newDoor = new Door {
                    X = toCreateAt.X,
                    Y = toCreateAt.Y,
                    isOpen = false,
                    isLocked = false,
                    room = toCreateAdjacentTo
                };
                map.doors.Add(newDoor);
            } else {
                //Don't alter the cell properties so the
                //player can't walk on a locked door
                Door newDoor = new Door {
                    X = toCreateAt.X,
                    Y = toCreateAt.Y,
                    isOpen = false,
                    isLocked = true,
                    room = toCreateAdjacentTo
                };
                map.doors.Add(newDoor);
            }
        }

        //This is the method that kicks of the process
        //of resizing a room into the boss room
        private void ResizeRoom(Room roomToResize) {
            roomToResize.ResizeRoom(RectangleForResizing(roomToResize), map);
            bossRoom = roomToResize;
            map.bossRoom = roomToResize;
            CreateLockedDoor();
        }

        //This method locks boss room door
        private void CreateLockedDoor() {
            foreach (Door door in map.doors) {
                //Is the door attached to the boss room?
                if (door.room == bossRoom) {
                    //Alter the position of the door to the boss room
                    if(bossRoom.corridors[0].direction == Direction.Down) {
                        door.Y -= 1;
                    }else if(bossRoom.corridors[0].direction == Direction.Up) {
                        door.Y += 1;
                    }
                    //Lock the door
                    door.isLocked = true;
                    //Stop the foreach loop
                    return;
                }
            }
        }

        #endregion

        //This method creates a new rectangle with the dimensions
        //of a boss room based around a room that already exists
        private Rectangle RectangleForResizing(Room roomToResize) {
            //Creates a rectange that should be used for the boss room.
            Point pointToResizeFrom = null;
            if (roomToResize.corridors[0].direction == Direction.Down) {
                pointToResizeFrom = roomToResize.middleTop;
                return CreateRectangleFromPoint(pointToResizeFrom, Direction.Down, bossRoomWidth, bossRoomHeight);
            } else if (roomToResize.corridors[0].direction == Direction.Up) {
                pointToResizeFrom = roomToResize.middleBottom;
                return CreateRectangleFromPoint(pointToResizeFrom, Direction.Up, bossRoomWidth, bossRoomHeight);
            } else if (roomToResize.corridors[0].direction == Direction.Left) {
                pointToResizeFrom = new Point(roomToResize.middleRight.X - 1, roomToResize.middleRight.Y);
                return CreateRectangleFromPoint(pointToResizeFrom, Direction.Left, bossRoomWidth, bossRoomHeight);
            } else if (roomToResize.corridors[0].direction == Direction.Right) {
                pointToResizeFrom = new Point(roomToResize.middleLeft.X + 1, roomToResize.middleLeft.Y);
                return CreateRectangleFromPoint(pointToResizeFrom, Direction.Right, bossRoomWidth, bossRoomHeight);
            }
            return null;
        }

        //This method checks if a room could be expanded from by checking
        //all cells in a certain radius and if any are occupied then no
        //expansion can be made. Returns tuple of (can be expanded, variation)
        private Tuple<bool, int> CanPointBeExpandedFrom(Room toExpandFrom, Direction toExpandInto, int radius) {
            Point expansionPoint = GetExpansionPointFromDirection(toExpandFrom, toExpandInto);
            //Generates random variation so corridors start at random(ish) points along the walls
            int variation = RandomVariation(2);
            //Attempts to check every cell but use try-catch in case some cells fall outside the map boundaries
            try {
                if (toExpandInto == Direction.Down) {
                    for (int x = expansionPoint.X - radius + variation; x < expansionPoint.X + radius + variation; x++) {
                        for (int y = expansionPoint.Y + 1; y < expansionPoint.Y + 1 + radius; y++) {
                            if (map.GetCell(x, y).IsWalkable) {
                                return new Tuple<bool, int>(false, 0);
                            }
                        }
                    }
                } else if (toExpandInto == Direction.Up) {
                    for (int x = expansionPoint.X - radius + variation; x < expansionPoint.X + radius + variation; x++) {
                        for (int y = expansionPoint.Y - 1 - radius; y < expansionPoint.Y - 1; y++) {
                            if (map.GetCell(x, y).IsWalkable) {
                                return new Tuple<bool, int>(false, 0);
                            }
                        }
                    }
                } else if (toExpandInto == Direction.Left) {
                    for (int y = expansionPoint.Y - radius + variation; y < expansionPoint.Y + radius + variation; y++) {
                        for (int x = expansionPoint.X - 1 - radius; x < expansionPoint.X - 1; x++) {
                            if (map.GetCell(x, y).IsWalkable) {
                                return new Tuple<bool, int>(false, 0);
                            }
                        }
                    }
                } else if (toExpandInto == Direction.Right) {
                    for (int y = expansionPoint.Y - radius + variation; y < expansionPoint.Y + radius + variation; y++) {
                        for (int x = expansionPoint.X + 1; x < expansionPoint.X + 1 + radius; x++) {
                            if (map.GetCell(x, y).IsWalkable) {
                                return new Tuple<bool, int>(false, 0);
                            }
                        }
                    }
                }
            } catch {
                //The expansion falls outside the map boundary so don't expand
                return new Tuple<bool, int>(false, 0);
            }
            //The room/wall could be expanded from
            return new Tuple<bool, int>(true, variation);
        }

        //This method takes a direction and returns the point
        //from which a corridor should extend or a null value
        private Point GetExpansionPointFromDirection(Room room, Direction direction) {
            //Return the point in the middle of that specific wall
            // e.g. the middle of the top wall if the direction is up
            if (direction == Direction.Up) {
                return room.middleTop;
            } else if (direction == Direction.Down) {
                return room.middleBottom;
            } else if (direction == Direction.Left) {
                return room.middleLeft;
            } else if (direction == Direction.Right) {
                return room.middleRight;
            }
            return null;
        }

        //This method checks if a player already exists
        //and makes a new one, if one doesn't
        //and then adds it to the dungeon map
        private void PlacePlayer() {
            Player player = Game.player;
            if (player == null) {
                player = new Player();
            }

            player.X = rooms[0].centre.X - 2;
            player.Y = rooms[0].centre.Y - 2;

            map.AddPlayer(player);
        }

        //This method randomly places monsters
        //inside rooms within the map
        private void PlaceMonsters(EquipmentGenerator equipmentGenerator) {
            foreach (Room room in rooms) {
                //Each room has 75% chance of having a monster
                if(Game.random.Next(1, 101) < 75 && room != bossRoom) {
                    //Place a random encounter into the room
                    PlaceMonstersInRoom(room);
                }
            }
            //Gives a key to a random monster
            PlaceKey();

            //Places a boss inside
            //the boss room
            PlaceBoss();
        }

        //Add some monsters to a given room
        private void PlaceMonstersInRoom(Room room) {
            //Generate a random encounter
            Encounter encounterToPlace = Game.encounterSystem.SelectRandomEncounter();
            //A list of all monsters to be added
            List<string> toAdd = encounterToPlace.monsters;
            //Iterates over all the monsters to be placed into the room
            for (int i = 0; i < toAdd.Count; i++) {
                //Find a random point within the room
                Point point = map.GetRandomWalkablePositionInRoom(room.rectangle);
                //If there is a point where the monster can be placed
                if(point != null) {
                    //The monster to be added to the map
                    Monster monster = null;
                    //Interpret the string into a monster object
                    if(toAdd[i] == "Ko") {
                        monster = Kobold.Create();
                    }else if(toAdd[i] == "Wk") {
                        monster = WingedKobold.Create();
                    }else if(toAdd[i] == "Ak") {
                        monster = KoboldArcher.Create();
                    }else if(toAdd[i] == "Go") {
                        monster = Goblin.Create();
                    }else if(toAdd[i] == "Ho") {
                        monster = Hobgoblin.Create();
                    }
                    try {
                        monster.X = point.X;
                        monster.Y = point.Y;
                        map.AddMonster(monster);
                    } catch {
                        //An error occured possibly because another string
                        //that isn't listed above has been detected
                    }
                }
            }
        }

        //This method randomly places a bnoss
        //inside the existing boss room
        private void PlaceBoss() {
            Monster boss = null;
            //There's 2 types of bosses so randomly select one
            int bossType = Game.random.Next(1, 4);
            if(bossType == 1) {
                //The boss is a dragon
                boss = Dragon.Create();
            } else if(bossType == 2) {
                //The boss is a werewolf
                boss = Werewolf.Create();
            } else {
                //The boss is a lich
                boss = Lich.Create();
            }
            //Places the boss onto the coordinates
            //and onto the map
            Point bossPoint = map.GetRandomWalkablePositionInRoom(bossRoom.rectangle, boss.monsterSize);
            boss.X = bossPoint.X;
            boss.Y = bossPoint.Y;
            map.AddMonster(boss, true);
        }

        //This method gives a key
        //object to a random monster
        public void PlaceKey() {
            //Select a random monster
            Monster monster = null;// map.monsters[Game.random.Next(0, map.monsters.Count)];
            if (monster != null) {
                //Give the monster the key
                monster.Inventory.Add(new Key());
            }
        }

        //This method places a set of stairs
        //at a random point within the boss room
        private void PlaceStairs() {
            Point stairsPoint = map.GetRandomWalkablePositionInRoom(bossRoom.rectangle);
            if(stairsPoint != null) {
                map.AddStairs(new Stairs(stairsPoint.X, stairsPoint.Y));
            } else {
                throw new Exception("No stairs placed");
            }
        }

        //This method returns a list of monsters that can be
        //placed inside a room where the number of monsters
        //is dependant upon the difficulty of that monster type
        private List<Monster> MonstersForRoom() {
            List<Monster> toReturn = new List<Monster>();
            int type = Game.random.Next(1, 5);
            //For each possible value of 'type' variable,
            //a different monster is added to the list
            //The number of monsters added depends
            //on the difficulty of the monster
            if(type == 1) {
                for (int i = 0; i < Game.random.Next(5); i++) {
                    toReturn.Add(Kobold.Create());
                }
            } else if(type == 2) {
                for (int i = 0; i < Game.random.Next(3); i++) {
                    toReturn.Add(WingedKobold.Create());
                }
            } else if (type == 3) {
                for (int i = 0; i < Game.random.Next(6); i++) {
                    toReturn.Add(Goblin.Create());
                }
            } else if (type == 4) {
                for (int i = 0; i < Game.random.Next(4); i++) {
                    toReturn.Add(Hobgoblin.Create());
                }
            } else {
                //Ensures no errors occur by 
                //not returning a null value
                return toReturn;
            }
            //Returns the populated
            //list of monsters
            return toReturn;
        }

        //This method returns a random room that has at least
        //one open direction or returns null if non exist
        private Room GetRandomRoomWithOpenDirection() {
            //All rooms with an open direction
            List<Room> validRooms = rooms.Where(r => r.corridors.Count < 4).ToList();
            if (validRooms.Count > 0) {
                return validRooms[rand.Next(0, validRooms.Count)];
            }
            //No rooms were found so return null, this is handled elsewhere
            return null;
        }

        //This method returns a random value, up to a max,
        //of either 1 or 2 which is weighted towards
        //lower values such as 0 or 1
        private int RandomVariation(int maxValue) {
            if (maxValue == 2) {
                int var = rand.Next(0, 100);
                if (var > 90) {
                    return 2;
                } else if (var <= 90 && var > 65) {
                    return 1;
                } else if (var <= 65 && var > 35) {
                    return 0;
                } else if (var <= 35 && var > 10) {
                    return -1;
                } else {
                    return -2;
                }
            } else if (maxValue == 1) {
                int var = rand.Next(0, 100);
                if (var <= 90 && var > 65) {
                    return 1;
                } else if (var <= 65 && var > 35) {
                    return 0;
                } else if (var <= 35 && var > 10) {
                    return -1;
                }
            }
            return 0;
        }

        //This method returns a random value, up to a max,
        //of either 1 or 2 which is weighted towards
        //lower values such as 1 or -1 and cannot
        //return a value of 0
        private int RandomVariationNonZero(int maxValue) {
            if (maxValue == 2) {
                int var = rand.Next(0, 100);
                if (var > 90) {
                    return 2;
                } else if (var <= 90 && var > 50) {
                    return 1;
                } else if (var <= 50 && var > 10) {
                    return -1;
                } else {
                    return -2;
                }
            } else {
                int var = rand.Next(0, 100);
                if (var > 50) {
                    return 1;
                } else {
                    return -1;
                }
            }
        }

        //This uses a bredth first search to find
        //the room that is furthest from the start
        //and that only has one corridor to it
        private Room FurthestRoomWithOneConnection() {
            Queue<Room> toVisit = new Queue<Room>();
            Stack<Room> visited = new Stack<Room>();

            //Add the first room to the list of rooms to visit
            toVisit.Enqueue(rooms[0]);

            //Number of times a room has multiple connections (i.e splits)
            int splits = 0;

            //While there are more rooms to visit
            while (toVisit.Count != 0) {
                //Remove the room at front of queue
                Room currentRoom = toVisit.Dequeue();
                if (currentRoom.corridors.Count > 1) {
                    splits++;
                }
                //Add all adjoining rooms
                foreach (Corridor corridor in currentRoom.corridors) {
                    if (!visited.Contains(corridor.room2)) {
                        toVisit.Enqueue(corridor.room2);
                    }
                }
                //Add room to list of visited rooms
                visited.Push(currentRoom);
            }
            //Reverse order of visited rooms stack
            visited.Reverse();
            //Take the X values from the start of the visited stack
            List<Room> potentialRooms = visited.Take(splits).ToList();
            List<Room> validRooms = new List<Room>();
            for (int r = 0; r < potentialRooms.Count; r++) {
                if (potentialRooms[r].CouldRoomBeResized(RectangleForResizing(potentialRooms[r]), map) && potentialRooms[r].corridors.Count == 1) {
                    validRooms.Add(potentialRooms[r]);
                }
            }
            //Return either a random room, if multiple are at the same distance, or the largest room in the dungeon (probably won't occur)
            if (validRooms.Count > 0) {
                return validRooms[Game.random.Next(0, validRooms.Count)];
            } else {
                return LargestRoom();
            }
        }

        //This method iterates through all the rooms in the map
        //and returns the largest of those by area
        private Room LargestRoom() {
            int largestArea = 0;
            Room largestRoom = null;
            foreach (Room room in rooms) {
                if (room != rooms[0] && room.corridors.Count == 1) {
                    int currentArea = (room.rectangle.Right - room.rectangle.Left) * (room.rectangle.Bottom - room.rectangle.Top);
                    if (currentArea > largestArea) {
                        largestArea = currentArea;
                        largestRoom = room;
                    }
                }
            }
            return largestRoom;
        }
    }
}
