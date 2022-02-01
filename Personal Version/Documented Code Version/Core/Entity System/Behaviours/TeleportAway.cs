using Roguelike.Interfaces;
using Roguelike.Systems;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Core.Entity_System.Behaviours {
    public class TeleportAway : IBehaviour {

        //This method allows an entity to teleport away from the player 
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
                //The monster has been alerted for one more turn
                monster.turnsAlterted++;
                //If the monster has been alerted for more than 15 turns
                //and hasn't seen the player then reset the timer
                if (monster.turnsAlterted > 15) {
                    monster.turnsAlterted = null;
                }
                if(Game.dungeonMap.bossRoom != null) {
                    Point point = Game.dungeonMap.GetRandomWalkablePositionInRoom(Game.dungeonMap.bossRoom.rectangle, monster.monsterSize);
                    if(point != null) {
                        Game.dungeonMap.SetEntityPosition(monster, point.X, point.Y, monster.monsterSize);
                        return true;
                    }
                } else {
                    //Map doesn't contain rooms so find alternative point to move to
                    Point point = Game.dungeonMap.GetWalkablePositionNearPoint(new Point(monster.X, monster.Y), 8, monster.monsterSize);
                    if(point != null) {
                        Game.dungeonMap.SetEntityPosition(monster, point.X, point.Y, monster.monsterSize);
                        return true;
                    } else {
                        Console.WriteLine("No point to teleport to!");
                    }
                }
            }
            return false;
        }
    }
}
