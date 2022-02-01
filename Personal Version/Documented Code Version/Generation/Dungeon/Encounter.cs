using System.Collections.Generic;

namespace Roguelike.Generation.Dungeon {
    public class Encounter {

        //List of strings which represent the
        //monsters in the particular encounter
        public List<string> monsters;

        //Integer which represents the difficulty
        //of this specific encounter
        public int difficulty;

        //Constructor needs no input parameters
        //and instead sets properties at default values 
        public Encounter() {
            monsters = new List<string>();
            difficulty = -1;
        }
    }
}
