using Roguelike.Interfaces;
using Roguelike.Systems;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Core.Entity_System.Behaviours {
    public class LifeSteal : IBehaviour {

        //This bejaviour consists of a monster stealing an amount
        //of health from the player and also healing that amount
        public bool Act(Monster monster, CommandSystem commandSystem, PathfindingSystem pathfindingSystem) {
            //Fetch some objects from other classes
            //and assign them for easy access
            DungeonMap dungeonMap = Game.dungeonMap;
            Player player = Game.player;
            //If the player is in the monster's FOV
            //radius, then the monster is alerted
            if (!monster.turnsAlterted.HasValue) {
                if (dungeonMap.IsInEntitiesFieldOfView(player.X, player.Y, monster)) {
                    monster.turnsAlterted = 1;
                }
            }
            if (monster.turnsAlterted.HasValue) {
                if(dungeonMap.IsInEntitiesFieldOfView(player.X, player.Y, monster)){
                    //Determine the amount of health to steal
                    int healthToSteal = Game.random.Next(1, Game.player.Health);
                    //Output a message saying the monster stole health
                    Game.messageLog.Add(monster.Name + " stole " + healthToSteal + " from the player!");
                    //Deal damage to the player
                    Game.commandSystem.Damage(healthToSteal, Game.player);
                    //Restore health to the monster
                    monster.Health += healthToSteal;
                    //If the monster has more health than their max
                    //then their health equals their max value
                    if(monster.Health > monster.MaxHealth) {
                        monster.Health = monster.MaxHealth;
                    }
                } else {
                    return false;
                }
            }
            return true;
        }
    }
}
