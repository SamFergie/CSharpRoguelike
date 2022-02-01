using RLNET;
using Roguelike.Core.Entity_System;
using Roguelike.Core.Inventory;
using Roguelike.Core.Inventory.Items;
using Roguelike.Generation.Dungeon;
using Roguelike.Utilities;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Core {
    public class DungeonMap : Map {

        public string mapName{ get; set; }

        //A list of all doors in the dungeon map
        public List<Door> doors { get; set; }

        //A list of all monsters in the dungeon map
        public List<Monster> monsters { get; set; }
        
        //A reference to the boss monster
        public Monster bossMonster { get; set; }

        //A list of all treasure in the dungeon map
        public List<Treasure> treasures { get; set; }

        //A list of all stairs in the dungeon map
        public List<Stairs> stairs { get; set; }

        //A reference to the boss room within the map
        public Room bossRoom { get; set; }

        //Dungeon Map Constructor
        public DungeonMap() {
            doors = new List<Door>();
            monsters = new List<Monster>();
            treasures = new List<Treasure>();
            stairs = new List<Stairs>();
        }

        //Main draw method where every cell in the dungeon map is drawn
        //onto the console, which is passed as a parameter, along with the
        //stats console for drawing monster stats
        public void Draw(RLConsole mapConsole, RLConsole statConsole) {
            //This code will set the symbol for every cell in the map
            foreach(Cell cell in GetAllCells()) {
                SetConsoleSymbolForCell(mapConsole, cell);
            }

            //Draw every door onto the dungeon map
            foreach(Door door in doors) {
                door.Draw(mapConsole, this);
            }

            foreach(Stairs stairs in stairs) {
                stairs.Draw(mapConsole, this);
            }

            //Draw every treasure onto the map
            foreach (Treasure treasure in treasures) {
                treasure.Draw(mapConsole, this);
            }

            //This represents the number of monsters currently in the player's FOV
            int numberInFOV = 0;
            //Call draw method for all monsters
            for (int i = 0; i < monsters.Count; i++) {
                Monster monster = monsters[i];
                monster.Draw(mapConsole, this);
                //Determines if the monsters[i] is in FOV
                for (int x = 0; x < monsters[i].monsterSize; x++) {
                    for (int y = 0; y < monsters[i].monsterSize; y++) {
                        if(IsInEntitiesFieldOfView(monster.X + x, monster.Y + y, Game.player)) {
                            //The monster is in FOV so render the stats
                            //for the monster based on the number in FOV
                            monster.DrawStats(statConsole, numberInFOV);
                            numberInFOV++;
                            //Skip the next loop by failing check condition of: 
                            //is x < monsters[i].monsterSize (which is no longer true)
                            x = monster.monsterSize + 1;
                            break;
                        }
                    }
                }
            }
        }

        //This method sets the symbol for any given cell
        private void SetConsoleSymbolForCell(RLConsole console, Cell cell) {
            //Cell is not explored so don't set any symbol for that cell
            if (!cell.IsExplored) {
                return;
            }
            
            //If cell is in FOV then use 'Fov' colours, otherwise
            //use darker alternative colours. Also use a different symbol
            //for walls '#' vs floors '.'
            if(IsInFov(cell.X, cell.Y)) {
                if (cell.IsWalkable) {
                    console.Set(cell.X, cell.Y, DungeonColours.floorFov, DungeonColours.floorBackgroundFov, '.');
                } else {
                    console.Set(cell.X, cell.Y, DungeonColours.wallFov, DungeonColours.wallBackgroundFov, '#');
                }
            } else {
                if (cell.IsWalkable) {
                    console.Set(cell.X, cell.Y, DungeonColours.floor, DungeonColours.floorBackground, '.');
                } else {
                    console.Set(cell.X, cell.Y, DungeonColours.wall, DungeonColours.wallBackground, '#');
                }
            }
        }

        //This method updates a player's FOV
        public void UpdatePlayerFieldOfView() {
            Player player = Game.player;
            //Calculates the FOV of the current player
            ComputeFov(player.X, player.Y, player.Awareness, true);

            //For every cell in the map, set the cell to be explored if the cell is
            //inside the current FOV. Colour of the cell is handled elsewhere
            foreach(Cell cell in GetAllCells()) {
                if(IsInFov(cell.X, cell.Y)){
                    SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                }
            }
        }

        //This method takes a monster and calculates if 
        //a given X,Y coordinate is in their FOV
        public bool IsInEntitiesFieldOfView(int x, int y, Entity entity) {
            if(entity is Player) {
                FieldOfView playerFOV = new FieldOfView(this);
                playerFOV.ComputeFov(entity.X, entity.Y, entity.Awareness, true);
                return playerFOV.IsInFov(x, y);
            } else if(entity is Monster) {
                Monster monster = entity as Monster;
                FieldOfView monsterFOV = new FieldOfView(this);
                if (monster.monsterSize == 1) {
                    monsterFOV.ComputeFov(monster.X, monster.Y, monster.Awareness, true);
                    return monsterFOV.IsInFov(x, y);
                } else {
                    for (int xIn = 0; xIn < monster.monsterSize; xIn++) {
                        for (int yIn = 0; yIn < monster.monsterSize; yIn++) {
                            monsterFOV.ComputeFov(monster.X + xIn, monster.Y + yIn, monster.Awareness, true);
                            if(monsterFOV.IsInFov(x, y)) {
                                return true;
                            }
                        }
                    }
            }

            }
            return false;
        }

        //This method attempts to set an entities position
        //and returns a boolean based upon the success it has
        public bool SetEntityPosition(Entity entity, int X, int Y) {
            //Checks cell is walkable and that the door could be opened
            if(GetCell(X, Y).IsWalkable && CanOpenDoor(entity, X, Y)) {
                //Sets the current position of the entity to be walkable
                SetIsWalkable(entity.X, entity.Y, true);

                //Moves the entity
                entity.X = X;
                entity.Y = Y;

                //Sets the new position of the entity to be unwalkable
                SetIsWalkable(entity.X, entity.Y, false);

                //Attempt to pick up any treasure in the new coordinates
                PickUpTreasure(entity, X, Y);

                //Attempt to descend any stairs
                DescendStairs(entity, X, Y);

                //If the entity is a player then update the player's FOV
                if (entity is Player) {
                    UpdatePlayerFieldOfView();
                }
                return true;
            }
            //The entity couldn't be moved so return true
            return false;
        }

        //This method attempts to set an entities (with size greater than 1)
        //position and returns a boolean based upon the success it has
        public bool SetEntityPosition(Entity entity, int X, int Y, int entitySize = 1) {
            //If size of entity is 1, use other method
            //since it is quicker
            if(entitySize == 1) {
                SetEntityPosition(entity, X, Y);
            }
            //Sets cells of the entity to be walkable again
            for (int x = 0; x < entitySize; x++) {
                for (int y = 0; y < entitySize; y++) {
                    SetIsWalkable(entity.X + x, entity.Y + y, true);
                }
            }
            //One final validation that cell is wallkable
            //then sets all cells to be unwalkable and moves
            //X, Y coordinates otherwise return false
            entity.X = X;
            entity.Y = Y;
            for (int x = 0; x < entitySize; x++) {
                for (int y = 0; y < entitySize; y++) {
                    //Make the occupied cell unwalkable
                    SetIsWalkable(entity.X + x, entity.Y + y, false);

                    //Attempt to pick up any treasure in the new coordinates
                    PickUpTreasure(entity, entity.X + x, entity.Y + y);
                }
            }
            return true;
        }

        //This method updates a certain cell's walkability to the 3rd parameter
        public void SetIsWalkable(int x, int y, bool isWalkable) {
            Cell cell = GetCell(x, y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
        }

        //This method sets the game's instance of a player to the specified player
        //and makes their cell unwalkable then performs an FOV calculation
        public void AddPlayer(Player player) {
            Game.player = player;
            SetIsWalkable(player.X, player.Y, false);
            UpdatePlayerFieldOfView();
            //Adds player to the scheduling system
            Game.schedulingSystem.Add(player);
        }

        //Takes a new monster and adds it to the map
        public void AddMonster(Monster monster, bool isBoss = false) {
            //Checks if the monster is a boss monster
            if(isBoss == true) {
                bossMonster = monster;
            }
            monsters.Add(monster);
            //Sets all cells to be unwalkable
            for (int x = 0; x < monster.monsterSize; x++) {
                for (int y = 0; y < monster.monsterSize; y++) {
                    SetIsWalkable(monster.X + x, monster.Y + y, false);
                }
            }
            //Adds monster to the scheduling system
            Game.schedulingSystem.Add(monster);
        }

        //Takes a monster and removes it from the map
        public void RemoveMonster(Monster monster) {
            //If the monster to be removed is the boss monster 
            //then set the reference to the boss equal to false
            if (monster == bossMonster) {
                bossMonster = null;
            }
            //Validates the list contains monster
            //to be removed
            if (monsters.Contains(monster)) {
                monsters.Remove(monster);
            }
            //Sets all cells to be unwalkable
            for (int x = 0; x < monster.monsterSize; x++) {
                for (int y = 0; y < monster.monsterSize; y++) {
                    SetIsWalkable(monster.X + x, monster.Y + y, true);
                }
            }
            //Removes monster from scheduling system
            Game.schedulingSystem.Remove(monster);
        }

        //This method determines if a given X,Y coordinate is a cell
        //occupied by the given monster and returns true/false accordingly
        public bool IsCellPartOfMonster(int x, int y, Monster monster) {
            if (monster.monsterSize != 1) {
                //Finds all cells contain the monster
                List<Cell> cells = new List<Cell>();
                //Finds all cells that the monster occupies
                for (int xIn = 0; xIn < monster.monsterSize; xIn++) {
                    for (int yIn = 0; yIn < monster.monsterSize; yIn++) {
                        cells.Add(GetCell(monster.X + xIn, monster.Y + yIn));
                    }
                }
                //Checks every cell to see if found
                foreach (Cell cell in cells) {
                    if (cell.X == x && cell.Y == y) {
                        return true;
                    }
                }
            }
            //A monster was not found at the cell
            return false;
        }

        //This method simply takes a treasure and adds it
        //to the game map
        public void AddTreasure(Treasure treasure) {
            //Find treasure at new treasures location
            Treasure currentTreasure = GetTreasure(treasure.X, treasure.Y);
            //Check if treasure at new treasures location
            if(currentTreasure != null) {
                //Add all the gold to the old treasure
                currentTreasure.AddGold(treasure.goldAmount);
                //Add all the equipment to the old treasure
                foreach (Equipment equipment in treasure.equipment) {
                    if(equipment.Name == "Key") {
                        currentTreasure.equipment = new List<Equipment>() { new Key() };
                        break;
                    } else {
                        currentTreasure.AddEquipment(equipment);
                    }
                }
            } else {
                //No treasure currently exists
                //so add it to the map
                treasures.Add(treasure);
            }
        }

        //This method simply takes stairs
        //and adds it to the game map
        public void AddStairs(Stairs stair) {
            stairs.Add(stair);
        }

        //This method uses a predicate to find a treausure
        //at a given X, Y coodinate
        public Treasure GetTreasure(int x, int y) {
            return treasures.SingleOrDefault(t => t.X == x && t.Y == y);
        }

        //This method uses a predicate to find
        //a stairs at a given X, Y coordinate
        public Stairs GetStairs(int x, int y) {
            return stairs.SingleOrDefault(s => s.X == x && s.Y == y);
        }

        //This method can be used when an entity moved to a new
        //cell and want to try to collect any treasure there
        private void PickUpTreasure(Entity entity, int x, int y) {
            if(entity is Player) {
                //Find the treasure at the X, Y coordinates
                Treasure treasure = GetTreasure(x, y);
                //If treasure wasy found
                if(treasure != null) {
                    //Add the gold to the player's amount
                    entity.Gold += treasure.goldAmount;
                    //Has the list of equipment been initialised
                    if(treasure.equipment != null) {
                        //Does the list even contain any equipment
                        if (treasure.equipment.Count > 0) {
                            //If treasure contains a key then the entity should pick the key up
                            if(entity.Inventory.Contains(new Key())) {
                                entity.Inventory.Add(new Key());
                            } else {
                                //Add a random item from the equipment to the players inventory
                                entity.Inventory.Add(treasure.equipment[Game.random.Next(0, treasure.equipment.Count)]);
                            }
                        }
                    }
                    //Remove the treasure from the game map
                    treasures.Remove(treasure);
                }
            } else {
                //Do nothing since only the
                //player can collect treasure
                return;
            }
        }

        //This method is used for when an player moves onto a set of
        //stairs and they wish to descend onto a new level 
        private void DescendStairs(Entity entity, int x, int y) {
            if(entity is Player) {
                Stairs stairs = GetStairs(x, y);
                if(stairs != null) {
                    if(bossMonster == null) {
                        Game.shouldLoadLevel = true;
                    } else {
                        Game.messageLog.Add("Kill the boss to proceed to the next level!");
                    }
                }
            }
        }

        //Returns a door at given
        //coordinates or null 
        public Door GetDoor(int x, int y) {
            //Uses lambda predicate function to check X and Y coordinates
            return doors.SingleOrDefault(d => d.X == x && d.Y == y);
        }

        //This method checks if a door is at the
        //intended movement location and opens it
        private void OpenDoor(Entity entity, Door door) {
            //Set the door to open and unlocked
            door.isOpen = true;
            if (door.isLocked) {
                door.isLocked = false;
            }
            //Change the cell's properties so it is transparent
            Cell cell = GetCell(door.X, door.Y);
            SetCellProperties(door.X, door.Y, true, cell.IsWalkable, cell.IsExplored);
            //Set the symbol of the cell
            door.symbol = '-';
            //Output a message to the message log
            Game.messageLog.Add(entity.Name + " opened a door.");
        }

        //This method checks if the player can
        //open a door at the new movmeent location
        private bool CanOpenDoor(Entity entity, int x, int y) {
            Door door = GetDoor(x, y);
            if(door == null) {
                //No door exists at location
                //so entity can proceed
                return true;
            }
            if (door.isOpen) {
                //Door is open so
                //entity can proceed
                return true;
            }
            bool hasKey = false;
            if(Key.DoesEntityHaveKey(entity)) {
                //Player has a key in their inventory
                hasKey = true;
            }
            if (door.isLocked) {
                //Locked and closed door
                if(hasKey && entity is Player) {
                    //The player has a key, so can open the door
                    OpenDoor(entity, door);
                    //Finds the first key in the player's inventory and use it
                    (Game.player.Inventory.Where(i => i.Name == "Key").ToList().ElementAt(0) as Item).Use();
                } else {
                    //Door is locked but player doesn't have a key so can't open the door
                    return false;
                }
            } else {
                //Door isn't locked so open door
                OpenDoor(entity, door);
            }
            foreach (Door d in doors) {
                Cell c = GetCell(d.X, d.Y);
                //If for whatever reason the cell
                //is transparent when closed:
                if (c.IsTransparent && !d.isOpen) {
                    //Sets the properties of the cell to fix a bug
                    //where certain doors are made transparent
                    SetCellProperties(d.X, d.Y, false, c.IsWalkable, c.IsExplored);
                }
            }
            return true;
        }

        //This method determines if a monster is adjacent to the player
        //and therefore if the monster could attack the player (using a melee attack)
        public bool IsMonsterAdjacentToPlayer(Monster monster) {
            //All cells above monster
            List<Cell> cells = GetCellsAlongLine(monster.X, monster.Y - 1, monster.X + monster.monsterSize - 1, monster.Y - 1).ToList();
            //All cells to left of monster
            cells.AddRange(GetCellsAlongLine(monster.X - 1, monster.Y, monster.X - 1, monster.Y + monster.monsterSize - 1));
            //All cells to right of monster
            cells.AddRange(GetCellsAlongLine(monster.X + monster.monsterSize, monster.Y, monster.X + monster.monsterSize, monster.Y + monster.monsterSize - 1));
            //All cells below monster
            cells.AddRange(GetCellsAlongLine(monster.X, monster.Y + monster.monsterSize, monster.X + monster.monsterSize - 1, monster.Y + monster.monsterSize));
            foreach (Cell cell in cells) {
                //Checks all cells to see if player is there
                //and return true if player is
                if(cell.X == Game.player.X && cell.Y == Game.player.Y) {
                    return true;
                }
            }
            //Otherwise return false
            return false;
        }

        //This method detemines if a given room
        //has any walkable space in it
        public bool DoesRoomHaveWalkableSpace(Rectangle room) {
            //Iterate through all cells in given room
            for (int x = 1; x < room.Width; x++) {
                for (int y = 1; y < room.Height; y++) {
                    try {
                        if(IsWalkable(x + room.X, y + room.Y)) {
                            return true;
                        }
                    } catch {
                        //An error could occur due to a room
                        //clipping outside the bounds of the map
                    }
                }
            }
            return false;
        }

        //This method returns a random walkable position in the room
        public Point GetRandomWalkablePositionInRoom(Rectangle room) {
            //Checks room has walkable space
            if (DoesRoomHaveWalkableSpace(room)) {
                //Has 100 iterations at which point
                //chance of returning null is <1%
                for (int i = 0; i < 100; i++) {
                    int x = Game.random.Next(1, room.Width - 1) + room.X;
                    int y = Game.random.Next(1, room.Height - 1) + room.Y;
                    if(IsWalkable(x, y)) {
                        return new Point(x, y);
                    }
                }
            }
            //No position has been found
            return null;
        }

        //This method returns a random walkable position in a room given a monster of a certain size
        public Point GetRandomWalkablePositionInRoom(Rectangle room, int monsterSize) {
            if(monsterSize == 1) {
                return GetRandomWalkablePositionInRoom(room);
            } else {
                //Has 100 iterations at which point
                //chance of returning null is <1%
                for (int i = 0; i < 100; i++) {
                    //Selects a random point within the room
                    int x = Game.random.Next(1, room.Width - 1) + room.X;
                    int y = Game.random.Next(1, room.Height - 1) + room.Y;
                    //Sets a flag for if that position is walkable
                    bool isWalkable = true;
                    //Iterate over all cells that the monster would occupy given their size
                    for (int xIn = 0; xIn < monsterSize; xIn++) {
                        for (int yIn = 0; yIn < monsterSize; yIn++) {
                            //If any of the cells the monster would occupy are unwalkable then
                            //the point selected is invalid so skip onto the next point
                            if(!IsWalkable(x + xIn, y + yIn)) {
                                isWalkable = false;
                            }
                            //if the flag is set then break outof the loop
                            if (!isWalkable) {
                                break;
                            } else if(xIn == monsterSize - 1 && yIn == monsterSize - 1) {
                                //Otherwise, if the flag is still set to true and the loop is
                                //at the end then the point is valid so return the point
                                return new Point(x, y);
                            }
                        }
                        //The randomly selected cell is unwalkable
                        //so skip to another random cell
                        if (!isWalkable) {
                            break;
                        }
                    }
                }
            }
            //No position has been found
            return null;
        }

        //This method returns a random walkable position within a radius
        public Point GetWalkablePositionNearPoint(Point point, int radius, int monsterSize) {
            List<Point> possiblePoints = new List<Point>();
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    int dist = Convert.ToInt16(Math.Sqrt((point.X - x) * (point.X - x) + (point.Y - y) * (point.Y - y)));
                    if(dist < radius) {
                        bool valid = true;
                        for (int xIn = 0; xIn < monsterSize; xIn++) {
                            for (int yIn = 0; yIn < monsterSize; yIn++) {
                                if(!GetCell(x + xIn, y + yIn).IsWalkable) {
                                    xIn = monsterSize;
                                    yIn = monsterSize;
                                    valid = false;
                                }
                            }
                        }
                        if (valid) {
                            possiblePoints.Add(new Point(x, y));
                        }
                    }
                }
            }
            if(possiblePoints.Count > 0) {
                return possiblePoints[Game.random.Next(0, possiblePoints.Count)];
            } else {
                return null;
            }
        }

        //This method returns the direction between entity1 and entity2,
        //from entity1's perspective this method works as long as the
        //entities are adjacent, otherwise it returns the none direction
        public Direction GetDirectionBetweenTwoEntities(Entity entity1, Entity entity2) {
            Direction direction = Direction.None;
            Monster monster = entity1 as Monster;
            if(monster.monsterSize != 1) {
                //Checks all cells above the entity to see if
                //entity 2 is there
                List<Cell> cells = Game.dungeonMap.GetCellsAlongLine(monster.X, monster.Y - 1, monster.X + monster.monsterSize - 1, monster.Y - 1).ToList();
                foreach (Cell cell in cells) {
                    if(cell.X == entity2.X && cell.Y == entity2.Y) {
                        direction = Direction.Up;
                        return direction;
                    }
                }
                //Checks all cells to left of entity1 to see if
                //entity 2 is there
                cells = Game.dungeonMap.GetCellsAlongLine(monster.X - 1, monster.Y, monster.X - 1, monster.Y + monster.monsterSize - 1).ToList();
                foreach (Cell cell in cells) {
                    if (cell.X == entity2.X && cell.Y == entity2.Y) {
                        direction = Direction.Left;
                        return direction;
                    }
                }
                //Checks all cells to right of entity1 to see if
                //entity 2 is there
                cells = Game.dungeonMap.GetCellsAlongLine(monster.X + monster.monsterSize - 1, monster.Y, monster.X + monster.monsterSize - 1, monster.Y + monster.monsterSize - 1).ToList();
                foreach (Cell cell in cells) {
                    if (cell.X == entity2.X && cell.Y == entity2.Y) {
                        direction = Direction.Right;
                        return direction;
                    }
                }
                //Checks all cells below entity1 to see if
                //entity 2 is there
                cells = Game.dungeonMap.GetCellsAlongLine(monster.X, monster.Y + monster.monsterSize - 1, monster.X + monster.monsterSize - 1, monster.Y + monster.monsterSize - 1).ToList();
                foreach (Cell cell in cells) {
                    if (cell.X == entity2.X && cell.Y == entity2.Y) {
                        direction = Direction.Down;
                        return direction;
                    }
                }
            }
            return direction;
        }

        //Call with the monster being the second parameter and the player/non monster being the first
        public int GetDistanceBetweenTwoEntites(Entity entity1, Entity entity2) {
            //Checks the parameters so that the first parameter is an entity and the second a monster
            if(entity1 is Monster) {
                return GetDistanceBetweenTwoEntites(entity2, entity1);
            }
            //The shortest distance temporarily is the largest integer value
            int shortestDistance = int.MaxValue;
            //Converts the entity to a monster
            Monster monster = entity2 as Monster;
            //Goes through each of the monsters cells and checks
            //the distance to find the shortest distance
            for (int x = 0; x < monster.monsterSize; x++) {
                for (int y = 0; y < monster.monsterSize; y++) {
                    //Pythagorean distance calculation
                    int dist = Convert.ToInt16(Math.Sqrt(Math.Pow(entity1.X - (monster.X + x), 2) + Math.Pow(entity1.Y - (monster.Y + y), 2)));
                    //If the distance is shorter than the previously shotest
                    //distance then update the new shortest distance
                    if(dist < shortestDistance) {
                        shortestDistance = dist;
                    }
                }
            }
            //Return the shortest distance found
            return shortestDistance;
        }

    }
}