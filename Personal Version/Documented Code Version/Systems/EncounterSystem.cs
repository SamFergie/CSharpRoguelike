using Roguelike.Core;
using Roguelike.Generation.Dungeon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Roguelike.Systems {
    public class EncounterSystem {

        //A list of all the possible encounters
        //that could be generated
        public List<Encounter> encounters;

        //Constructor sets up the lists
        //and creates a new text reader
        public EncounterSystem() {
            encounters = new List<Encounter>();
            TextReader textReader = new StreamReader(@"Encounters.txt");
            string line;
            //Loops through all lines in the file and, while the
            //value of that line isn't null will parse the line
            while((line = textReader.ReadLine()) != null) {
                ParseLine(line);
            }
        }

        //This method selects a random encounter
        //based of a proabability distribution
        public Encounter SelectRandomEncounter() {
            int difficultyChance = 95;//Game.random.Next(1, 101);
            if(difficultyChance > 90) {
                //10% chance for Deadly Encounter
                List<Encounter> validEncounters = encounters.Where(e => e.difficulty == 4).ToList();
                //Return a random deadly encounter
                return validEncounters[Game.random.Next(0, validEncounters.Count - 1)];
            } else if(difficultyChance <= 90 && difficultyChance < 60) {
                //30% chance for hard encounter
                List<Encounter> validEncounters = encounters.Where(e => e.difficulty == 3).ToList();
                //Return a random hard encounter
                return validEncounters[Game.random.Next(0, validEncounters.Count - 1)];
            } else if(difficultyChance <= 60 && difficultyChance < 20) {
                //40% chance for medium encounter
                List<Encounter> validEncounters = encounters.Where(e => e.difficulty == 2).ToList();
                //Return a random medium encounter
                return validEncounters[Game.random.Next(0, validEncounters.Count - 1)];
            } else {
                //20% chance for easy encounter
                List<Encounter> validEncounters = encounters.Where(e => e.difficulty == 1).ToList();
                //Return a random easy encounter
                return validEncounters[Game.random.Next(0, validEncounters.Count - 1)];
            }
        }

        //Takes a line of an encounter and generates a tuple based
        //on such the line of string alongside that encounter's difficulty
        public void ParseEncounter(string monstersInEncounter, int difficulty) {
            //The encounter to be generated
            Tuple<int, string> monstersForEncounter;
            //The number of monsters to be generated
            int numberOfMonsters = 0;
            //The position of the first letter in the string
            int pos = 0;
            //Goes through all the characters in the encounter string
            //and until it finds the first letter increments pos
            while (!Char.IsLetter(monstersInEncounter[pos])) {
                pos++;
            }
            //If pos isn't 0 then the encounter has multiple monsters
            if(pos != 0) {
                //Finds the first (pos) characters i.e the number
                //at the start of the string
                numberOfMonsters = Convert.ToInt16(monstersInEncounter.Substring(0, pos));
                //This makes a new tuple of a number of monsters where the second parameter
                //is the string value between the number and the first space position
                monstersForEncounter = new Tuple<int, string>(numberOfMonsters, monstersInEncounter.Substring(pos, monstersInEncounter.IndexOf(' ') - 1));
            } else {
                //The encounter only has one monster
                monstersForEncounter = new Tuple<int, string>(1, monstersInEncounter);
            }
            //Generates an encounter based on the tuple varaible
            GenerateEncounter(monstersForEncounter, difficulty);
        }

        //Takes a line of an encounter and generates a tuple based on such the
        //line of string alongside that encounter's difficulty however this
        //method takes in an encounter that contains multiple types of monsters
        public void ParseEncounter(List<string> monstersInEncounter, int difficulty) {
            //The encounter to be generated
            List<Tuple<int, string>> monstersForEncounter = new List<Tuple<int, string>>();
            //Goes through each monster type and adds a tuple based on the 
            //specific type of monster within the overall encounter
            foreach (string monsterTypes in monstersInEncounter) {
                //The number of monsters to be generated
                int numberOfMonsters = 0;
                //The position of the first letter in the string
                int pos = 0;
                //Goes through all the characters in the encounter string
                //and until it finds the first letter increments pos
                while (!Char.IsLetter(monsterTypes[pos])) {
                    pos++;
                }
                //If pos isn't 0 then the encounter has multiple monsters
                if (pos != 0) {
                    //Finds the first (pos) characters i.e the number
                    //at the start of the string
                    numberOfMonsters = Convert.ToInt16(monsterTypes.Substring(0, pos));
                    //This makes a new tuple of a number of monsters where the second parameter
                    //is the string value between the number and the first space position
                    monstersForEncounter.Add(new Tuple<int, string>(numberOfMonsters, monsterTypes.Substring(pos)));
                } else {
                    //The monster type only has one monster
                    monstersForEncounter.Add(new Tuple<int, string>(1, monsterTypes));
                }
            }
            //Generates an encounter based on the tuple varaible
            GenerateEncounter(monstersForEncounter, difficulty);
        }

        //This method takes a line of text and breaks
        //it into specific monster configurations
        public void ParseLine(string line) {
            //Does the line contain different
            //types of monsters?
            if (line.Contains("/")) {
                List<int> slashIndexes = new List<int>();
                List<string> monsterConfigs = new List<string>();

                //Loop through all '/' characters starting at the
                //first and ending when none are left
                for (int i = line.IndexOf('/'); i > -1; i = line.IndexOf('/', i + 1)) {
                    slashIndexes.Add(i);
                }

                //Substring the line of text between the different '/' characters
                int previous = 0;
                for (int i = 0; i < slashIndexes.Count + 1; i++) {
                    //If the monster config is the final one on the list, then substring
                    //until the index of the ' ' (which is the end of the list)
                    if (i == slashIndexes.Count) {
                        monsterConfigs.Add(line.Substring(previous + 1, line.IndexOf(' ') - previous - 1));
                    }else if(i > 0) {
                        monsterConfigs.Add(line.Substring(previous + 1, slashIndexes[i] - previous - 1));
                        previous = slashIndexes[i];
                    } else {
                        monsterConfigs.Add(line.Substring(previous, slashIndexes[i]));
                        previous = slashIndexes[i];
                    }
                }
                //Multiple types of monsters in the encounter
                ParseEncounter(monsterConfigs, ParseDifficulty(line.ElementAt(line.Length - 1).ToString()));
            } else {
                //Only one type of monster in the encounter
                ParseEncounter(line, ParseDifficulty(line.ElementAt(line.Length - 1).ToString()));
            }
        }

        //This method takes a string representing difficulty
        //and turns it into an integer (higher value means harder)
        public int ParseDifficulty(string difficultyString) {
            if(difficultyString == "E") {
                return 1;
            }else if(difficultyString == "M") {
                return 2;
            } else if (difficultyString == "H") {
                return 3;
            } else if (difficultyString == "D") {
                return 4;
            }
            //Otherwise return -1
            return -1;
        }

        //This method takes a tuple of an integer and a string
        //which represent the number of monsters and the string representing
        //those monsters along with the difficulty of the encounter
        public void GenerateEncounter(Tuple<int, string> encounterSetup, int diff) {
            //Create new encounter with correct difficulty
            Encounter encounter = new Encounter {
                difficulty = diff
            };
            //For every monster to be added(item1 in  tuple)
            //then a monster (item2 in tuple) is assed
            for (int i = 0; i < encounterSetup.Item1; i++) {
                encounter.monsters.Add(encounterSetup.Item2);
            }
            //Add the encounter to the system's
            //overall list of possible encounters
            encounters.Add(encounter);
        }

        //This method takes a list of tuples of an integer and a string which
        //represent the number of each type of monsters and the string representing
        //the number of that type monsters along with the difficulty of the encounter
        public void GenerateEncounter(List<Tuple<int, string>> encounterSetup, int diff) {
            //Create new encounter with correct difficulty
            Encounter encounter = new Encounter {
                difficulty = diff
            };
            //For every group of monsters to be added perform the same
            //loop as single monster encounter
            for (int i = 0; i < encounterSetup.Count; i++) {
                //For every monster to be added(item1 in tuple)
                //then a monster (item2 in tuple) is assed
                for (int j = 0; j < encounterSetup[i].Item1; j++) {
                    encounter.monsters.Add(encounterSetup[i].Item2);
                }
            }
            //Add the encounter to the system's
            //overall list of possible encounters
            encounters.Add(encounter);
        }

    }
}
