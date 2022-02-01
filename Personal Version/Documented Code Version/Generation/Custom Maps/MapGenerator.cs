using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RLNET;
using Roguelike.Core;
using RogueSharp;

namespace Roguelike.Generation.Custom_Maps {
    public class MapGenerator {

        public List<DungeonMap> dungeonMaps;
        private RLConsole targetConsole;

        public MapGenerator(RLConsole console) {
            targetConsole = console;
            dungeonMaps = new List<DungeonMap>();
            try {
                foreach (string file in Directory.EnumerateFiles("_Resources/Maps", "*.txt")){
                    if (IsFileValid(File.ReadAllLines(file).ToList())) {
                        dungeonMaps.Add(GenerateCustomMap(File.ReadAllLines(file).ToList()));
                    }
                }
            } catch (Exception e){
                throw e;
            }
        }

        private bool IsFileValid(List<string> file) {
            if (file.Count > targetConsole.Height) {
                return false;
            }
            foreach (string line in file) {
                if (line.Length > targetConsole.Width) {
                    return false;
                }
            }
            return true;
        }

        public DungeonMap GenerateCustomMap(List<string> mapFile) {
            DungeonMap map = new DungeonMap();
            map.Initialize(targetConsole.Width, targetConsole.Height);
            for (int y = 0; y < mapFile.Count; y++) {
                for (int x = 0; x < mapFile[y].Length; x++) {
                    if(mapFile[y].ElementAt(x) == '#') {
                        map.SetCellProperties(x, y, false, false, false);
                    }else if (mapFile[y].ElementAt(x) == ' ') {
                        map.SetCellProperties(x, y, true, true, false);
                    } else if (mapFile[y].ElementAt(x) == '@') {
                        map.SetCellProperties(x, y, true, true, false);
                        PlacePlayer(map, x, y);
                    }
                }
            }
            return map;
        }

        private void PlacePlayer(DungeonMap map, int x, int y) {
            Game.player.X = x;
            Game.player.Y = y;
            map.AddPlayer(Game.player);
        }

    }
}
