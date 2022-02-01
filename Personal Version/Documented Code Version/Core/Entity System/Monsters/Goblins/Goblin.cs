using Roguelike.Core.Entity_System.Behaviours;
using Roguelike.Systems;
using Roguelike.Utilities;
using System;

namespace Roguelike.Core.Entity_System.Monsters.Goblins {
    public class Goblin : Monster{

        //Static goblin method means reference to
        //a goblin object isn't needed
        public static Goblin Create() {
            //Generate health (which has a minimum of 4)
            int health = Math.Max(4, Game.diceSystem.Roll("2D6") - 2);

            //Create a new goblin object
            //using inline parameters
            Goblin newGoblin = new Goblin {
                Awareness = 12,
                Gold = Game.diceSystem.Roll("6D6"),
                Name = "Goblin",

                ArmourRating = 7,

                Strength = 9,
                Constitution = 9,
                Intelligence = 10,
                Dexterity = 12,

                MaxHealth = health,
                Health = health,
                Speed = 4,

                colour = DungeonColours.goblinColour,
                symbol = 'G',
                monsterSize = 1,
            };

            //Return object to fulfill method return type
            return newGoblin;
        }

        //Goblin specific override for AI
        public override void PerformAction(CommandSystem commandSystem, PathfindingSystem pathfindingSystem) {
            //If less than (or at) half health, the goblin should run away
            if(Health <= MaxHealth / 2) {
                RunAway behaviour = new RunAway();
                behaviour.Act(this, commandSystem, pathfindingSystem);
            } else {
                //Otherwise it should enagage the player
                base.PerformAction(commandSystem, pathfindingSystem);
            }
        }

    }
}
