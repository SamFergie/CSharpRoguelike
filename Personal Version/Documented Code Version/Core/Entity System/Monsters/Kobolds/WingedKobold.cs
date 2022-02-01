using RLNET;
using Roguelike.Utilities;
using System;

namespace Roguelike.Core.Entity_System.Monsters.Kobolds {
    public class WingedKobold : Monster {

        //Static winged kobold method means reference
        //to a kobold object isn't needed
        public static WingedKobold Create() {
            //Generate health (which is limited at 7)
            int health = Math.Max(7, Game.diceSystem.Roll("3D8") - 3);

            //Create a new winged kobold
            //object using inline parameters
            WingedKobold newWingedKobold = new WingedKobold {
                Awareness = 10,
                Gold = Game.diceSystem.Roll("4D4"),
                Name = "Winged Kobold",

                ArmourRating = 10,
                Strength = 14,
                Constitution = 7,
                Intelligence = 7,
                Dexterity = 9,

                MaxHealth = health,
                Health = health,
                Speed = 6,

                colour = RLColor.Blend(DungeonColours.koboldColour, RLColor.Red, 0.5f),
                symbol = 'K',
                monsterSize = 1,

            };
            //Return object to fulfill method return type
            return newWingedKobold;
        }

    }
}
