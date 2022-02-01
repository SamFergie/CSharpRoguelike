using RLNET;
using Roguelike.Core.Entity_System.Behaviours;
using Roguelike.Core.Inventory.EquipmentTypes;
using Roguelike.Systems;
using Roguelike.Utilities;
using System;

namespace Roguelike.Core.Entity_System.Monsters.Goblins {
    public class Hobgoblin : Monster {

        //Static hobgoblin method means reference
        //to a hobgoblin object isn't needed
        public static Hobgoblin Create() {
            //Generate health (which has a minimum value of 6)
            int health = Math.Max(6, Game.diceSystem.Roll("3D6") - 2);

            //Create a new hobbolin object
            //using inline constructor
            Hobgoblin newHobgoblin = new Hobgoblin {
                Awareness = 12,
                Gold = Game.diceSystem.Roll("8D6"),
                Name = "Hobgoblin",

                ArmourRating = 9,
                Strength = 11,
                Constitution = 9,
                Intelligence = 10,
                Dexterity = 12,

                MaxHealth = health,
                Health = health,
                Speed = 6,

                colour = RLColor.Blend(DungeonColours.goblinColour, RLColor.Black, 0.75f),
                symbol = 'H',
                monsterSize = 1,

                //Create weapon so monster has
                //a greater damage potential
                Weapon = new MeleeEquipment {
                    Name = "Blunt Club",
                    DamageRange = "1D6"
                }
            };
            //Return object to fulfill method return type
            return newHobgoblin;
        }

        //Hobgoblin specific override for AI
        public override void PerformAction(CommandSystem commandSystem, PathfindingSystem pathfindingSystem) {
            //If health is less than (or at) 25% then run away
            if(Health <= MaxHealth / 4) {
                RunAway behaviour = new RunAway();
                behaviour.Act(this, commandSystem, pathfindingSystem);
            } else {
                //Otherwise it should engage the player
                base.PerformAction(commandSystem, pathfindingSystem);
            }
        }

    }
}
