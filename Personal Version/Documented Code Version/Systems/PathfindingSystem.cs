using Roguelike.Core;
using Roguelike.Core.Entity_System;
using Roguelike.Utilities;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Systems {
    public class PathfindingSystem {

        //Properties such as all the cells in a map,
        //the map to be searched on and the destination cell
        public List<Cell> cells;
        public DungeonMap map;
        public Cell path;

        //Constructor for the pathfinding system
        public PathfindingSystem(List<Cell> totalCells, DungeonMap dungeonMap) {
            cells = totalCells;
            map = dungeonMap;
        }

        //Main pathfinding method. Takes the starting cell and finds
        //the closest valid cell to the target cell
        public Cell Pathfind(Cell startingCell, Cell targetCell, int entitySize = 1) {
            Cell path = null;
            if(entitySize == 1) {
                //All possible adjacent cells
                List<Cell> adjacentCells = AdjacentCells(startingCell);
                //Closest walkable cell
                Cell closestCell = ClosestWalkableCell(adjacentCells, targetCell);
                //Assign closest cell to path
                path = closestCell;
            } else {
                //All possible cells
                List<Tuple<Cell, Direction>> adjacentCells = AdjacentCells(startingCell, entitySize);
                //Closest walkable cell
                Tuple<Cell, Direction> closestMove = ClosestWalkableCell(startingCell, adjacentCells, targetCell, entitySize);
                //Assign path to closest move
                path = closestMove.Item1;
            }
            if (path == null) {
                //No path was found so error is thrown
                IndexOutOfRangeException exception = new IndexOutOfRangeException("No cells are valid! Route isn't possible!");
                return null;
            } else {
                //Return closest cell
                return path;
            }
        }

        //Main pathfinding method. Takes the starting cell and finds
        //the closest valid cell to the target cell
        public Cell PathfindAway(Cell startingCell, Cell targetCell, int entitySize = 1) {
            Cell path = null;
            if (entitySize == 1) {
                //All possible adjacent cells
                List<Cell> adjacentCells = AdjacentCells(startingCell);
                //Closest walkable cell
                Cell closestCell = ClosestWalkableCell(adjacentCells, targetCell);
                //Assign closest cell to path
                path = closestCell;
            } else {
                //All possible cells
                List<Tuple<Cell, Direction>> adjacentCells = AdjacentCells(startingCell, entitySize);
                //Closest walkable cell
                Tuple<Cell, Direction> closestMove = ClosestWalkableCell(startingCell, adjacentCells, targetCell, entitySize);
                //Assign path to closest move
                path = closestMove.Item1;
            }
            if (path == null) {
                //No path was found so error is thrown
                IndexOutOfRangeException exception = new IndexOutOfRangeException("No cells are valid! Route isn't possible!");
                return null;
            } else {
                //Return closest cell
                return path;
            }
        }

        //This method finds all adjacent cells
        //from a given starting cell
        private List<Cell> AdjacentCells(Cell startingCell) {
            List<Cell> adjacentCells = new List<Cell>();
            //Attempts to get all 4 adjacent cells using error
            //catching incase such a cell doesn't exist
            try {
                adjacentCells.Add(map.GetCell(startingCell.X - 1, startingCell.Y));
            } catch {
            }
            try {
                adjacentCells.Add(map.GetCell(startingCell.X + 1, startingCell.Y));
            } catch {
            }
            try {
                adjacentCells.Add(map.GetCell(startingCell.X, startingCell.Y - 1));
            } catch {
            }
            try {
                adjacentCells.Add(map.GetCell(startingCell.X, startingCell.Y + 1));
            } catch {
            }
            //If none of the surrounding cells exist then return null
            // otherwise return the list of cells.
            if (adjacentCells.Count > 0) {
                return adjacentCells;
            } else {
                return null;
            }
        }

        //This method finds all adjacent cells from a
        //given starting cell, also returns the direction
        //of the move compared to the starting cell
        private List<Tuple<Cell, Direction>> AdjacentCells(Cell startingCell, int entitySize) {
            List<Tuple<Cell, Direction>> adjacentCells = new List<Tuple<Cell, Direction>>();
            //Attempts to get all 4 adjacent cells using error
            //catching incase such a cell doesn't exist
            try {
                adjacentCells.Add(new Tuple<Cell, Direction>(map.GetCell(startingCell.X - 1, startingCell.Y), Direction.Left));
            } catch {
            }
            try {
                adjacentCells.Add(new Tuple<Cell, Direction>(map.GetCell(startingCell.X + 1, startingCell.Y), Direction.Right));
            } catch {
            }
            try {
                adjacentCells.Add(new Tuple<Cell, Direction>(map.GetCell(startingCell.X, startingCell.Y - 1), Direction.Up));
            } catch {
            }
            try {
                adjacentCells.Add(new Tuple<Cell, Direction>(map.GetCell(startingCell.X, startingCell.Y + 1), Direction.Down));
            } catch {
            }
            //If none of the surrounding cells exist then return null
            // otherwise return the list of cells.
            if (adjacentCells.Count > 0) {
                return adjacentCells;
            } else {
                return null;
            }
        }

        //This method takes a list of possible moves and
        //finds the closest valid to a given target cell
        //assuming the entity has a size greater than 1
        private Tuple<Cell, Direction> ClosestWalkableCell(Cell entityCell, List<Tuple<Cell, Direction>> possibleMoves, Cell targetCell, int entitySize) {
            Tuple<Cell, Direction> closestMove = null;
            float lowestDistance = float.MaxValue;
            Cell closestCell = null;
            foreach (Tuple<Cell, Direction> move in possibleMoves) {
                //Checks if move is valid(since entity is larger than 1x1)
                if (IsMoveValid(entityCell, move.Item2, entitySize)) {
                    //This finds the distance between the 2 cells
                    float currentDistance = DistanceBetweenCells(move.Item1, targetCell);
                    if (currentDistance < lowestDistance) {
                        //A new closest cell has been found
                        closestCell = move.Item1;
                        closestMove = new Tuple<Cell, Direction>(closestCell, move.Item2);
                        lowestDistance = currentDistance;
                    }
                }
            }
            //Return best valid move found
            return closestMove;
        }

        //This method takes a list of possible moves and
        //finds the furthest valid from a given target cell
        //assuming the entity has a size greater than 1
        private Tuple<Cell, Direction> FurthestWalkableCell(Cell entityCell, List<Tuple<Cell, Direction>> possibleMoves, Cell targetCell, int entitySize) {
            Tuple<Cell, Direction> furthest = null;
            float furthestDistance = float.MinValue;
            Cell furthestCell = null;
            foreach (Tuple<Cell, Direction> move in possibleMoves) {
                //Checks if move is valid(since entity is larger than 1x1)
                if (IsMoveValid(move.Item1, move.Item2, entitySize)) {
                    //This finds the distance between the 2 cells
                    float currentDistance = DistanceBetweenCells(move.Item1, targetCell);
                    if (currentDistance > furthestDistance) {
                        //A new furthest cell has been found
                        furthestCell = move.Item1;
                        furthest = new Tuple<Cell, Direction>(furthestCell, move.Item2);
                        furthestDistance = currentDistance;
                    }
                }
            }
            //Return worst(best move away) valid move found
            return furthest;
        }

        //This method takes a possible movement and direction, along with
        //the size of entity and determines if such the given move is valid
        private bool IsMoveValid(Cell entityCell, Direction direction, int entitySize) {
            //Assigns empty list of cells
            List<Cell> cells = new List<Cell>();
            if (direction == Direction.Up) {
                //Gets all cells in line above the given entity
                cells = map.GetCellsAlongLine(entityCell.X, entityCell.Y - 1, entityCell.X + entitySize - 1, entityCell.Y - 1).ToList();
            } else if (direction == Direction.Down) {
                //Gets all cells in line below the given entity
                cells = map.GetCellsAlongLine(entityCell.X, entityCell.Y + entitySize, entityCell.X + entitySize - 1, entityCell.Y + entitySize).ToList();
            } else if (direction == Direction.Left) {
                //Gets all cells in line to left of the given entity
                cells = map.GetCellsAlongLine(entityCell.X - 1, entityCell.Y, entityCell.X - 1, entityCell.Y + entitySize - 1).ToList();
            } else if (direction == Direction.Right) {
                //Gets all cells in line to right of the given entity
                cells = map.GetCellsAlongLine(entityCell.X + entitySize, entityCell.Y, entityCell.X + entitySize, entityCell.Y + entitySize - 1).ToList();
            }
            //Checks all cells and if any
            //aren't walkable then move is invald
            foreach (Cell cell in cells) {
                if (!cell.IsWalkable) {
                    return false;
                }
            }
            //Move is otherwise valid
            return true;
        }

        //This method takes a list of possible cells and
        //finds the cloest to a given target cell
        private Cell ClosestWalkableCell(List<Cell> possibleCells, Cell targetCell) {
            //If the list of possible cells contains
            //the target, return that
            if (possibleCells.Contains(targetCell)) {
                return targetCell;
            }
            float lowestDistance = float.MaxValue;
            Cell closestCell = null;

            //Iterates through each cell and if the next cell 
            //is closer, updates 2 variables defined above
            foreach(Cell cell in possibleCells) {
                //This finds the distance between the 2 cells
                float currentDistance = DistanceBetweenCells(cell, targetCell);
                if(currentDistance < lowestDistance) {
                    //A new closest cell has been found
                    closestCell = cell;
                    lowestDistance = currentDistance;
                }
            }
            return closestCell;
        }

        //This method takes a list of possible cells and
        //finds the furthest from a given target cell
        private Cell FurthestWalkableCell(List<Cell> possibleCells, Cell targetCell) {
            float highestDistance = float.MinValue;
            Cell furthestDistance = null;

            //Iterates through each cell and if the next cell 
            //is closer, updates 2 variables defined above
            foreach (Cell cell in possibleCells) {
                //This finds the distance between the 2 cells
                float currentDistance = DistanceBetweenCells(cell, targetCell);
                if (currentDistance > highestDistance) {
                    //A new furthest cell has been found
                    furthestDistance = cell;
                    highestDistance = currentDistance;
                }
            }
            return furthestDistance;
        }

        //Method takes 2 cells and finds the
        //pythagorean distance between them
        private float DistanceBetweenCells(Cell toCheck, Cell targetCell) {
            float distance = (float)Math.Pow((toCheck.X - targetCell.X), 2);
            distance += (float)Math.Pow((toCheck.Y - targetCell.Y), 2);
            return (float)Math.Sqrt(distance);
        }

    }
}
