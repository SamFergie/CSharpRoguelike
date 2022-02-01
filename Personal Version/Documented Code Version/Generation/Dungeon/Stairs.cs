using RLNET;
using Roguelike.Interfaces;
using Roguelike.Utilities;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Generation.Dungeon {
    public class Stairs : IDrawable {

        //The properties necessary for the
        //object to be drawn onto the map
        public RLColor colour { get; set; }
        public char symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Stairs(int x, int y) {
            symbol = '<';
            colour = DungeonColours.doorLockedColourFov;
            X = x;
            Y = y;
        }

        public void Draw(RLConsole console, IMap map) {
            if(!map.IsExplored(X, Y)) {
                return;
            }
            if(!map.IsInFov(X, Y)) {
                console.Set(X, Y, colour, DungeonColours.floorBackgroundFov, symbol);
            } else {
                console.Set(X, Y, RLColor.Blend(colour, RLColor.Gray, 0.5f), DungeonColours.floorBackgroundFov, symbol);
            }
        }
    }
}
