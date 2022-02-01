using Roguelike.Core;
using Roguelike.Core.Entity_System;
using Roguelike.Core.Entity_System.Monsters.Bosses;
using Roguelike.Core.Entity_System.Monsters.Goblins;
using Roguelike.Core.Entity_System.Monsters.Kobolds;
using Roguelike.Generation.Dungeon;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Generation.Cave {
    public class CaveSystemGenerator {

        //This is the dungeon map that the class will 
        //modify until the final map has been made
        public DungeonMap map;

        private readonly int width;

        private readonly int height;

        private readonly float startChance;

        private readonly float birthLimit;

        private readonly float deathLimit;

        private readonly int numberOfSteps;

        private readonly int numberOfFloods;

        public CaveSystemGenerator(int w, int h, float start, float birth, float death, int steps, int floods) {
            width = w;
            height = h;

            startChance = start;

            birthLimit = birth;
            deathLimit = death;

            numberOfSteps = steps;

            numberOfFloods = floods;

            map = new DungeonMap();
        }

        public DungeonMap GenerateCaveSystem() {
            map.Initialize(width, height);

            CreateInitialMap();

            UpdateMap();

            CloseMap();

            PlacePlayer();

            PlaceMonsters(Game.equipmentGenerator, 10);

            PlaceStairs();

            return map;
        }

        private void CreateInitialMap() {
            for (int x = 0; x < map.Width; x++) {
                for (int y = 0; y < map.Height; y++) {
                    if(Game.random.Next(1, 101) > startChance * 100) {
                        map.SetCellProperties(x, y, true, true, false);
                    } else {
                        map.SetCellProperties(x, y, false, false, false);
                    }
                }
            }    
        }

        private void UpdateMap() {
            for (int i = 0; i < numberOfSteps; i++) {
                map = AutomotaStep(map);
            }

            if (IsMapValid(numberOfFloods)) {
                Console.WriteLine("Good Map");
            } else {
                Console.WriteLine("Bad Map");
                GenerateCaveSystem();
            }
        }

        private DungeonMap AutomotaStep(DungeonMap oldMap) {
            DungeonMap newMap = oldMap;
            for (int x = 0; x < oldMap.Width; x++) {
                for (int y = 0; y < oldMap.Height; y++) {
                    List<Cell> cells = oldMap.GetCellsInRadius(x, y, 1).ToList();
                    Cell itself = cells.SingleOrDefault(c => c.X == x && c.Y == y);
                    if(itself != null) {
                        cells.Remove(itself);
                    }
                    int alive = cells.Where(c => c.IsWalkable == true).ToList().Count;
                    if(oldMap.IsWalkable(x, y)) {
                        if(alive < deathLimit) {
                            //Kill
                            newMap.SetCellProperties(x, y, false, false, false);
                        }
                    } else {
                        if(alive > birthLimit) {
                            //Birth
                            newMap.SetCellProperties(x, y, true, true, false);
                        }
                    }
                }
            }
            return newMap;
        }

        //This method checks if the current map is valid
        private bool IsMapValid(int counts) {
            List<Cell> reachable = new List<Cell>();
            reachable = FloodFill();
            int numberFound = -1;
            numberFound = reachable.Count;
            //Fill any unwalkable spaces
            if (numberFound > (0.25 * width * height)) {
                FillUnreachableSpaces(reachable);
            }
            //This now checks if the newly filled map is valid
            reachable = FloodFill();
            numberFound = reachable.Count;
            //At least 25% of the map must be walkable for it to be valid
            return (numberFound > (0.25 * width * height));
        }
        
        //This method floods fills the whole map
        private List<Cell> FloodFill(int range = 25) {
            Queue<Cell> unvisited = new Queue<Cell>();
            List<Cell> visited = new List<Cell>();
            Cell cell = null;
            //Uses a random walkable starting cell within the top left corner
            bool found = false;
            while (!found) {
                cell = GetRandomWalkableCellWithinRange(map.GetCell(0, 0), range);
                if(cell != null) {
                    found = true;
                }
            }
            //Uses a bredth first search to find all cells within range
            unvisited.Enqueue(cell);
            while(unvisited.Count > 0) {
                Cell currentCell = unvisited.Dequeue();
                visited.Add(currentCell);
                List<Cell> adjacent = map.GetCellsInRadius(currentCell.X, currentCell.Y, 1).Where(c => c.IsWalkable == true).ToList();
                foreach (Cell c in adjacent) {
                    if (!visited.Contains(c) && !unvisited.Contains(c)) {
                        unvisited.Enqueue(c);
                    }
                }
            }
            return visited;
        }

        //This method floods fills the map, to within a restriced ranged
        private List<Cell> RestrictedFloodFill(Cell initialCell, int range, int minStartDist = 0) {
            Cell startCell = GetRandomWalkableCellWithinRange(initialCell, range, minStartDist);
            if(startCell == null) {
                return null;
            }
            //Uses a bredth first search algorithm to visited all ceels within range
            Queue<Cell> unvisited = new Queue<Cell>();
            List<Cell> visited = new List<Cell>();

            unvisited.Enqueue(startCell);
            while (unvisited.Count > 0) {
                //Visit the next cell
                Cell currentCell = unvisited.Dequeue();
                visited.Add(currentCell);
                //Get all adjacent cells that are walkable
                List<Cell> adjacent = map.GetCellsInRadius(currentCell.X, currentCell.Y, 1).Where(c => c.IsWalkable == true).ToList();
                foreach (Cell c in adjacent) {
                    //Only visit the cell if it hasn't already been
                    //visited or if it is to be visited soon
                    if (!visited.Contains(c) && !unvisited.Contains(c)) {
                        if (DistanceBetween2Cells(startCell, c) < range) {
                            unvisited.Enqueue(c);
                        }
                    }
                }
            }
            //Return all the cells that the algorithm visited
            return visited;
        }

        //This method fill any unreachable areas of the map
        private void FillUnreachableSpaces(List<Cell> reachable) {
            List<Cell> inMap = map.GetAllCells().ToList();
            foreach (Cell cell in inMap) {
                //If the cell cannot be walked to, 
                //then the cell is made unwalkable
                if (!reachable.Contains(cell) && cell.IsWalkable) {
                    map.SetCellProperties(cell.X, cell.Y, cell.IsTransparent, false, cell.IsExplored);
                }
            }

        }

        //This method is responsible for placing encounters all over the map
        private void PlaceMonsters(EquipmentGenerator equipmentGenerator, int playerStartDist) {
            int averageDistance = map.Height / 4;
            for (int x = playerStartDist; x < map.Width * .95; x+= averageDistance) {
                for (int y = playerStartDist; y < map.Height * .95; y+= averageDistance) {
                    if (x + averageDistance >= map.Width * .95 && y + averageDistance >= map.Height * .95) {
                        //Place boss
                        PlaceBossNearPoint(x, y, 4);
                        return;
                    } else {
                        //Place generic encounter
                        if(Game.random.Next(1, 101) < 90) {
                            PlaceMonstersNearPoint(x, y, 2);
                        }
                    }
                }
            }
        }

        //This method places a random encounter near a specified point
        private void PlaceMonstersNearPoint(int x, int y, int averageDistance) {
            List<Cell> possibleCells = RestrictedFloodFill(map.GetCell(x, y), averageDistance);
            //Generate a random encounter
            Encounter encounterToPlace = Game.encounterSystem.SelectRandomEncounter();
            //A list of all monsters to be added
            List<string> toAdd = encounterToPlace.monsters;
            //Iterates over all the monsters to be placed into the room
            for (int i = 0; i < toAdd.Count; i++) {
                //Find a random point within the room
                if(possibleCells == null) {
                    continue;
                }
                Cell point = possibleCells[Game.random.Next(0, possibleCells.Count)];
                //If there is a point where the monster can be placed
                if (point != null) {
                    //The monster to be added to the map
                    Monster monster = null;
                    //Interpret the string into a monster object
                    if (toAdd[i] == "Ko") {
                        monster = Kobold.Create();
                    } else if (toAdd[i] == "Wk") {
                        monster = WingedKobold.Create();
                    } else if (toAdd[i] == "Ak") {
                        monster = KoboldArcher.Create();
                    } else if (toAdd[i] == "Go") {
                        monster = Goblin.Create();
                    } else if (toAdd[i] == "Ho") {
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

        //This method selects a random boss and places them onto the map
        private void PlaceBossNearPoint(int x, int y, int averageDistance) {
            //Find all the cells within range
            List<Cell> possibleCells = RestrictedFloodFill(map.GetCell(x, y), averageDistance);
            int iterations = 0;
            while(possibleCells.Count == 0) {
                iterations++;
                possibleCells = RestrictedFloodFill(map.GetCell(x, y), iterations);
            }
            //Selects a random boss
            Monster boss = null;
            //There's 2 types of bosses so randomly select one
            int bossType = Game.random.Next(1, 4);
            if (bossType == 1) {
                //The boss is a dragon
                boss = Lich.Create();
            } else if (bossType == 2) {
                //The boss is a werewolf
                boss = Lich.Create();
            } else {
                //The boss is a lich
                boss = Lich.Create();
            }
            //Selects a random point that the boss could occupy
            Cell point = null;
            while (point == null) {
                point = possibleCells[Game.random.Next(0, possibleCells.Count)];
                for (int xIn = 0; xIn < boss.monsterSize; xIn++) {
                    for (int yIn = 0; yIn < boss.monsterSize; yIn++) {
                        //The point must have enough space to contain the boss
                        if(!map.GetCell(point.X + xIn, point.Y + yIn).IsWalkable) {
                            point = null;
                            yIn = boss.monsterSize;
                            xIn = boss.monsterSize;
                        }
                    }
                }
            }
            //Places the boss onto the coordinates
            //and onto the map
            boss.X = point.X;
            boss.Y = point.Y;
            map.AddMonster(boss, true);
        }

        //This method closes off the map by placing walls around the edge
        private void CloseMap() {
            //Adds all the border cells to a list
            List<Cell> border = map.GetCellsInRows(0, map.Height - 1).ToList();
            border.AddRange(map.GetCellsInColumns(0, map.Width - 1));
            //Iterates over every border cell and makes it unwalkable
            foreach (Cell cell in border) {
                map.SetCellProperties(cell.X, cell.Y, cell.IsTransparent, false, cell.IsExplored);
            }
        }

        //This method places the player onto the map
        private void PlacePlayer() {
            //If no player exists, create a new player
            Player player = Game.player;
            if (player == null) {
                player = new Player();
            }

            //Find all the possible cells in the top-left of the
            //map and place the player at a random one of those
            List<Cell> playerCells = RestrictedFloodFill(map.GetCell(0, 0), 15, 2);
            Cell playerCell = playerCells[Game.random.Next(0, playerCells.Count)];
            player.X = playerCell.X;
            player.Y = playerCell.Y;

            //Add the player object to the map
            map.AddPlayer(player);
        }

        //This method places stairs near the boss monster
        private void PlaceStairs() {
            List<Cell> cellsNearBoss = RestrictedFloodFill(map.GetCell(map.bossMonster.X, map.bossMonster.Y), 4);
            Cell stairsPoint = cellsNearBoss[Game.random.Next(0, cellsNearBoss.Count)];
            if(stairsPoint != null) {
                map.AddStairs(new Stairs(stairsPoint.X, stairsPoint.Y));
            } else {
                throw new Exception("No stairs placed");
            }

        }

        //This method returns a random walkable cell within a given range
        //also takes in an optional minimum range
        private Cell GetRandomWalkableCellWithinRange(Cell startCell, int range, int minRange = 0) {
            //List of all cells within range
            List<Cell> withinRange = new List<Cell>();
            //Iterates over every cell on the map and adds it to a list
            for (int x = minRange; x < map.Width; x++) {
                for (int y = minRange; y < map.Height; y++) {
                    //Calculates distance between current cell and starting cell
                    int dist = DistanceBetween2Cells(startCell, map.GetCell(x, y));
                    //If cell is within range, then add it to a list
                    if(map.IsWalkable(x, y) && dist < range && dist > minRange) {
                        withinRange.Add(map.GetCell(x, y));
                    }
                }
            }
            //If some cells are within range then add it to the list
            if(withinRange.Count != 0) {
                return withinRange[Game.random.Next(0, withinRange.Count)];
            }
            //Otherwise return null
            return null;
        }

        //This method returns the distance between any 2 cells
        private int DistanceBetween2Cells(Cell cell1, Cell cell2) {
            return Convert.ToInt16(Math.Sqrt((cell1.X - cell2.X) * (cell1.X - cell2.X) + (cell1.Y - cell2.Y) * (cell1.Y - cell2.Y)));
        }

    }
}
