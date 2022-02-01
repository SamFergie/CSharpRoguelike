using Roguelike.Interfaces;
using Roguelike.Systems;
using RogueSharp;

namespace Roguelike.Core.Entity_System.Behaviours {
    public class StandardMoveAndPush : IBehaviour {

        //This behaviour consists of moving towards
        //the target and, if possible, attacking them
        //using a melee attack, this attack will, if it hits,
        //push the target back by 1 tile
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

            //If monster is alerted then perform this logic:
            Cell path = null;
            if (monster.turnsAlterted.HasValue) {
                //Use the pathfinding system to find the ideal cell to move to
                if (monster.monsterSize != 1) {
                    path = pathfindingSystem.Pathfind(dungeonMap.GetCell(monster.X, monster.Y), dungeonMap.GetCell(player.X, player.Y), monster.monsterSize);
                } else {
                    path = pathfindingSystem.Pathfind(dungeonMap.GetCell(monster.X, monster.Y), dungeonMap.GetCell(player.X, player.Y));
                }
                //If a path was found:
                if (path != null) {
                    //If the monster can't be moved:
                    if (!commandSystem.MoveMonster(monster, path)) {
                        //If the player is at the location of the path:
                        if (dungeonMap.IsMonsterAdjacentToPlayer(monster)) {
                            //Try and attack the player:
                            if(commandSystem.Attack(monster, player)) {
                                //The attack was successful so the player is pushed
                                //away from the attacker
                                commandSystem.Push(monster, player);
                            }
                        }
                    }
                }
                //The monster has been alerted for one more turn
                monster.turnsAlterted++;
                //IF the monster has been alerted for more than 15 turns
                //and hasn't seen the player then reset the timer
                if (monster.turnsAlterted > 15) {
                    monster.turnsAlterted = null;
                }
            }
            return true;
        }
    }
}
