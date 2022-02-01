using RLNET;
using Roguelike.Interfaces;
using Roguelike.Systems;
using RogueSharp;

namespace Roguelike.Core.Entity_System.Behaviours {
    public class Frenzy : IBehaviour {

        //This behaviour will augment the stats of a monsters
        //by making the monster 'frenzied'
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
                //Validates that the monster has a property called "Frenzied"
                //(i.e. could the monster become frenzied) 
                if (monster.GetType().GetField("frenzied") != null) {
                    //Add 2 to the strength of the monster
                    monster.Strength += 2;
                    //Output a message informing the player of what has happened
                    Game.messageLog.Add(Game.player.Name + " frenzied " + monster.Name + "! Look out, it's become even stronger!", RLColor.Blend(RLColor.Red , RLColor.White, 0.25f));
                    //Change the name of the monster to reinforce to the player what has happened
                    monster.Name = monster.Name.Insert(0, "Frenzied ");
                    //Change the colour of the monster so the player has  
                    //an immediate notice of something has happened
                    monster.colour = RLColor.Blend(monster.colour, RLColor.Red, 0.5f);
                }
            }
            //Return value mandated by the interface
            return true;
        }

    }
}
