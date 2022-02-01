using Roguelike.Utilities;
using System;

namespace Roguelike.Core.Entity_System.Monsters.Kobolds {
    public class Kobold : Monster {

        //Static kobold method means reference to
        //a kobold object isn't needed
        public static Kobold Create() {
            //Generate health (which is limited at 5)
            int health = Math.Max(5, Game.diceSystem.Roll("2D8") - 3);

            //Create a new kobold object
            //using inline parameters
            Kobold newKobold = new Kobold {
                Awareness = 10,
                Gold = Game.diceSystem.Roll("3D4"),
                Name = "Kobold",

                ArmourRating = 10,
                Strength = 12,
                Constitution = 7,
                Intelligence = 7,
                Dexterity = 9,

                MaxHealth = health,
                Health = health,
                Speed = 7,

                colour = DungeonColours.koboldColour,
                symbol = 'K',
                monsterSize = 1,

            };
            //Return object to fulfill method return type
            return newKobold;
        }

    }
}
