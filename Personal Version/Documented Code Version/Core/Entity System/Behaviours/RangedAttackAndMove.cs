using Roguelike.Interfaces;
using Roguelike.Systems;
using RogueSharp;

namespace Roguelike.Core.Entity_System.Behaviours {
    public class RangedAttackAndMove : IBehaviour {

        //This behaviour consists of checking if a ranged 
        //attack could be made then moving towards the target
        public bool Act(Monster monster, CommandSystem commandSystem, PathfindingSystem pathfindingSystem) {
            //Fetch some objects and assign them as variables 
            DungeonMap dungeonMap = Game.dungeonMap;
            Player player = Game.player;

            //If the player is in the monster's FOV
            //radius, then the monster is alerted
            if (!monster.turnsAlterted.HasValue) {
                if (dungeonMap.IsInEntitiesFieldOfView(player.X, player.Y, monster)) {
                    monster.turnsAlterted = 1;
                }
            }

            //What path should the monster take
            Cell path = null;
            //If the monster is alerted then:
            if (monster.turnsAlterted.HasValue) {
                //Should the entity check for a new move to make
                bool findPath = false;
                //The player must be in the monsters FOV to make an attack
                if (dungeonMap.IsInEntitiesFieldOfView(player.X, player.Y, monster)) {
                    //If the monster made an attack then don't check for a path,
                    //otherwise the monster should check for a new path
                    if(!commandSystem.RangedEvent(monster, player, player.X, player.Y)) {
                        Game.messageLog.Add(monster.Name + " missed the player!");
                        findPath = true;
                    } else {
                        findPath = false; 
                    }
                } else {
                    //Player is not in the monsters FOV so check for a new path
                    findPath = true;
                }
                //Find the best possible move and
                //then try to move to that cell
                if (findPath) {
                    path = pathfindingSystem.Pathfind(dungeonMap.GetCell(monster.X, monster.Y), dungeonMap.GetCell(player.X, player.Y), monster.monsterSize);
                    if (path != null) {
                        commandSystem.MoveMonster(monster, path);
                    }
                    //Monster has been alerted for one more turn
                    monster.turnsAlterted++;
                    //If the monster hasn't seen the player
                    //in 15 turns then reset the timer
                    if (monster.turnsAlterted > 15) {
                        monster.turnsAlterted = null;
                    }
                }
            }
            return true;
        }
    }
}
