using RLNET;
using Roguelike.Core.Entity_System;
using Roguelike.Interfaces;
using Roguelike.Utilities;
using RogueSharp;
using System;
using System.Collections.Generic;

namespace Roguelike.Core.Inventory {
    public class Treasure : IDrawable {

        //The amount of gold stored
        public int goldAmount { get; set; }

        //All the equipment stored
        public List<Equipment> equipment;

        //The properties necessary to be drawn
        //onto the game map
        public RLColor colour { get; set; }
        public char symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        //Default constructor needs only
        //an X and a Y coordinate
        public Treasure(int x, int y) {
            symbol = '$';
            colour = RLColor.Yellow;
            X = x;
            Y = y;
            equipment = new List<Equipment>();
        }

        //This static method is used when generating completely random
        //piles of treasure, such as for the end of a level/dungeon map
        public static Treasure GetRandomTreasure(int x, int y, Monster monster) {
            Treasure toReturn = new Treasure(x, y);
            //Treasure up to the monster's amount of gold
            //However to avoid errors the code ensures the second parameter
            //is greater than the first parameter using Math.Max
            int gold = Game.random.Next(1, Math.Max(monster.Gold, 2));
            toReturn.AddGold(gold);

            //33% chance of adding random equipment to the treasure
            if(Game.random.Next(1, 101) < 1000) {
                Equipment equipment = Game.equipmentGenerator.GenerateEquipment();
                toReturn.AddEquipment(equipment);
            }
            return toReturn;
        }

        //This method will draw the treasure onto the game map
        public void Draw(RLConsole console, IMap map) {
            //If the cell hasn't been explored
            //then don't draw anything
            if(!map.IsExplored(X, Y)) {
                return;
            }
            //The cell has been explored so something should be drawn to the screen
            if(!map.IsInFov(X, Y)) {
                console.Set(X, Y, colour, DungeonColours.floorBackgroundFov, symbol);
            } else {
                console.Set(X, Y, RLColor.Blend(colour, RLColor.Gray, 0.5f), DungeonColours.floorBackgroundFov, symbol);
            }
        }

        //Adds gold to the treasure
        public void AddGold(int amount) {
            goldAmount += amount;
        }

        //Adds equipment to the treasure
        public void AddEquipment(Equipment e) {
            equipment.Add(e);
        }

    }
}
