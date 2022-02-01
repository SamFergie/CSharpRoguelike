using Roguelike.Core.Entity_System.Behaviours;
using Roguelike.Systems;
using Roguelike.Utilities;
using System;

namespace Roguelike.Core.Entity_System.Monsters.Bosses {
    public class Dragon : Monster {

        //Static dragon method means reference to
        //a dragon object isn't needed
        public static Dragon Create() {
            //Generate health (which has a minimum of 12)
            int health = Math.Max(12, Game.diceSystem.Roll("3D8"));

            //Create a new dragon object
            //using inline parameters
            Dragon dragon = new Dragon {
                Awareness = 15,
                Gold = Game.diceSystem.Roll("4D8"),
                Name = "Dragon",

                ArmourRating = 12,

                Strength = 15,
                Constitution = 12,
                Intelligence = 11,
                Dexterity = 6,

                MaxHealth = health,
                Health = health,
                Speed = 15,

                colour = DungeonColours.dragonColour,
                symbol = 'D',
                monsterSize = 2
            };
            //Return object to fulfill method return type
            return dragon;
        }

        //Dragon specific override for AI        
        public override void PerformAction(CommandSystem commandSystem, PathfindingSystem pathfindingSystem) {
            StandardMoveAndPush behaviour = new StandardMoveAndPush();
            behaviour.Act(this, commandSystem, pathfindingSystem);
        }

    }
}
