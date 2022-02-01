using RLNET;
using Roguelike.Core.Entity_System.Behaviours;
using Roguelike.Systems;
using System;

namespace Roguelike.Core.Entity_System.Monsters.Bosses {
    public class Werewolf : Monster {

        //Has the werewolf been frenzied?
        public bool frenzied;

        //Static werewolf method means reference
        //to a werewolf object isn't needed
        public static Werewolf Create() {
            //Genertate health (which has a minimum of 13)
            int health = Math.Max(13, Game.diceSystem.Roll("3D8"));

            //Create a new werewolf object
            //using inline parameters
            Werewolf werewolf = new Werewolf {
                Awareness = 17,
                Gold = Game.diceSystem.Roll("3D6"),
                Name = "Werewolf",

                ArmourRating = 13,

                Strength = 14,
                Constitution = 12,
                Intelligence = 10,
                Dexterity = 9,

                MaxHealth = health,
                Health = health,
                Speed = 4,

                colour = RLColor.Blend(RLColor.Red, RLColor.White, 0.5f),
                symbol = 'W',
                monsterSize = 1,
                //By default, werewolfs are not frenzied
                frenzied = false
            };
            //Return object to fulfill method return type
            return werewolf;
        }

        //Werewolf specific override for AI
        public override void PerformAction(CommandSystem commandSystem, PathfindingSystem pathfindingSystem) {
            //Checks if monster has less than max
            //health and isn't frenzied before becoming frenzied
            if(Health < MaxHealth && !frenzied) {
                frenzied = true;
                Frenzy frenzy = new Frenzy();
                frenzy.Act(this, commandSystem, pathfindingSystem);
            } else {
                //If the monster has recieve no damage 
                //or is not frenzied then the standard AI will happen
                base.PerformAction(commandSystem, pathfindingSystem);
            }
        }

    }
}
