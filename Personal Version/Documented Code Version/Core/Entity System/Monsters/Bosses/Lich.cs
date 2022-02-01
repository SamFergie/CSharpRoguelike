using Roguelike.Core.Entity_System.Behaviours;
using Roguelike.Systems;
using Roguelike.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Core.Entity_System.Monsters.Bosses {
    public class Lich : Monster {

        //This determines the number of actions
        //until the Lich can teleport again
        private int teleportTime { get; set; }
        //This determines the number of actions
        //until the Lich can lifesteal again
        private int lifeStealTimer { get; set; }

        //Static lich method means reference
        //to a lich object isn't needed
        public static Lich Create() {
            //Generate health (which has a minimum of 15)
            int health = Math.Max(15, Game.diceSystem.Roll("5D8"));

            //Create a new Lich object
            //using inline parmaeters
            Lich lich = new Lich {
                Awareness = 15,
                Gold = Game.diceSystem.Roll("2D12"),
                Name = "Lich",

                ArmourRating = 14,
                Strength = 16,
                Constitution = 13,
                Intelligence = 18,
                Dexterity = 11,

                MaxHealth = health,
                Health = health,
                Speed = 12,

                colour = DungeonColours.lichColour,
                symbol = 'L',
                monsterSize = 3,

                teleportTime = -1
            };
            //Return lich object to fulfill method return type
            return lich;
        }

        //Lich specific override for AI        
        public override void PerformAction(CommandSystem commandSystem, PathfindingSystem pathfindingSystem) {
            //Calculate distance to player
            int dist = Game.dungeonMap.GetDistanceBetweenTwoEntites(Game.player, this);
            //If distance is less than [VALUE] then either teleport or melee attack
            if (dist < 3) {
                //Try to Teleport Away
                if(teleportTime == -1) {
                    TeleportAway behaviour = new TeleportAway();
                    behaviour.Act(this, commandSystem, pathfindingSystem);
                    teleportTime = 5;
                    return;
                }
            }
            //If the Lich can't teleport and the player is within range, they will try and lifesteal the player
            if (dist >= 3 && dist < 10) {
                //Otherwise try to lifesteal the player
                if (lifeStealTimer == -1 && Health < MaxHealth) {
                    //Lifesteals
                    LifeSteal behaviour = new LifeSteal();
                    behaviour.Act(this, commandSystem, pathfindingSystem);
                    lifeStealTimer = 8;
                    return;
                }
            }
            //Subtract one from the necessary timers
            if(teleportTime > -1) {
                teleportTime -= 1;
            }
            if(lifeStealTimer > -1) {
                lifeStealTimer -= 1;
            }
            //Default combat behaviour
            base.PerformAction(commandSystem, pathfindingSystem);
        }
    }
}
