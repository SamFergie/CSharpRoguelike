using Roguelike.Interfaces;
using Roguelike.Utilities;

namespace Roguelike.Core.Inventory {
    public class Equipment : IEntity {

        //Represents the rariety of the equipment
        //(will be overriden to weapon variety in weapon classes)
        public ArmourVariety Variety { get; set; }

        //Non combat properties
        public int Awareness { get; set; }
        public int Gold { get; set; }
        public string Name { get; set; }

        //Combat properties
        public int ArmourRating { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Speed { get; set; }

        //Stat modifiers
        public int Strength { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Dexterity { get; set; }
    }
}
