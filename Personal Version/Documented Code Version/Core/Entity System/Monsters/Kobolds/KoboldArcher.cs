using RLNET;
using Roguelike.Core.Entity_System.Behaviours;
using Roguelike.Core.Inventory.EquipmentTypes;
using Roguelike.Systems;
using Roguelike.Utilities;
using System;

namespace Roguelike.Core.Entity_System.Monsters.Kobolds {
    public class KoboldArcher : Monster {

        //Static kobold archer method means reference to
        //a kobold archer object isn't needed
        public static KoboldArcher Create() {
            //Generate health (which has a minimum of 2)
            int health = Math.Max(2, Game.diceSystem.Roll("1D6"));

            //Create a new kobold archer
            //object using inline parameters
            KoboldArcher koboldArcher = new KoboldArcher {
                Awareness = 12,
                Gold = Game.diceSystem.Roll("2D4"),
                Name = "Kobold Archer",

                ArmourRating = 8,
                Strength = 12,
                Constitution = 7,
                Intelligence = 7,
                Dexterity = 10,

                MaxHealth = health,
                Health = health,
                Speed = 7,

                colour = RLColor.Blend(DungeonColours.koboldColour, RLColor.White, 0.75f),
                symbol = 'K',
                monsterSize = 1,

                //Kobold archers have a ranged weapon
                //so they can use ranged behvaiours
                Weapon = new RangedEquipment {
                    Name = "Shortbow",
                    DamageRange = "1D3",
                    Range = 4,
                },
            };
            //Return object to fulfill method return type
            return koboldArcher;
        }

        //Kobold Archer specific override for AI
        public override void PerformAction(CommandSystem commandSystem, PathfindingSystem pathfindingSystem) {
            RangedAttackAndMove behaviour = new RangedAttackAndMove();
            behaviour.Act(this, commandSystem, pathfindingSystem);
        }

    }
}
