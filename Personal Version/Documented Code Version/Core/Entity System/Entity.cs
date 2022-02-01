using RLNET;
using Roguelike.Core.Inventory;
using Roguelike.Core.Inventory.EquipmentTypes;
using Roguelike.Interfaces;
using Roguelike.Utilities;
using RogueSharp;
using System;
using System.Collections.Generic;

namespace Roguelike.Core.Entity_System {
    public class Entity : IEntity, IDrawable, ISchedulable {

        //Private fields that will contain
        //information that is essentially readonly
        //such as the name of an entity/the amount of gold
        //etc... Dont' access these directly!
        private int awareness;
        private int gold;
        private string name;

        private int armourRating;
        private int health;
        private int maxHealth;
        private int speed;
        private int strength;
        private int constitution;
        private int intelligence;
        private int dexterity;
    
        //Actual properties that can be access from outside the entity object
        //This is where any modifiers should be applied, such as equipment etc...
        //These can be accessed and used to modify the private fields indriectly
        public int Awareness { get { return awareness; } set { awareness = value; } }
        public int Gold { get { return gold; } set { gold = value; } }
        public string Name { get { return name; } set { name = value; } }

        public int ArmourRating { get { return armourRating + Helm.ArmourRating + Chest.ArmourRating + Gloves.ArmourRating + Legs.ArmourRating + Boots.ArmourRating + Ring.ArmourRating; } set { armourRating = value; } }
        public int Health { get { return health + Helm.Health + Chest.Health + Gloves.Health + Legs.Health + Boots.Health + Ring.Health; } set { health = value; } }
        public int MaxHealth { get { return maxHealth + Helm.MaxHealth + Chest.MaxHealth + Gloves.MaxHealth + Legs.MaxHealth + Boots.MaxHealth + Ring.MaxHealth; } set { maxHealth = value; } }
        public int Speed { get { return speed + Helm.Speed + Chest.Speed + Gloves.Speed + Legs.Speed + Boots.Speed + Ring.Speed; } set { speed = value; } }

        public int Strength { get { return strength + Helm.Strength + Chest.Strength + Gloves.Strength + Legs.Strength + Boots.Strength + Ring.Strength; } set { strength = value; } }
        public int Constitution { get { return constitution + Helm.Constitution + Chest.Constitution + Gloves.Constitution + Legs.Constitution + Boots.Constitution + Ring.Constitution; } set { constitution = value; } }
        public int Intelligence { get { return intelligence + Helm.Intelligence + Chest.Intelligence + Gloves.Intelligence + Legs.Intelligence + Boots.Intelligence + Ring.Intelligence; } set { intelligence = value; } }
        public int Dexterity { get { return dexterity + Helm.Dexterity + Chest.Dexterity + Gloves.Dexterity + Legs.Dexterity + Boots.Dexterity + Ring.Dexterity; } set { dexterity = value; } }

        //Modifiers are defined as (stat - 10) / 2 and rounded down
        public int StrengthMod { get { return Convert.ToInt16(Math.Floor((double)(Strength - 10) / 2)); } }
        public int ConstitutionMod { get { return Convert.ToInt16(Math.Floor((double)(Constitution - 10) / 2)); } }
        public int IntelligenceMod { get { return Convert.ToInt16(Math.Floor((double)(Intelligence - 10) / 2)); } }
        public int DexterityMod { get { return Convert.ToInt16(Math.Floor((double)(Dexterity - 10) / 2)); } }

        //These are the properties used for rendering an entity
        //modifying these will alter the appearance of the entity
        //object on screen
        public RLColor colour { get; set; }
        public char symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        //The time it takes for an entity to get it's turn is internally
        //refered to as the speed of an entity
        public int Time { get { return speed; } }

        //The inventory equipment, not currently
        //being used by the entity
        public List<Equipment> Inventory { get; set; }

        //The equipment that is being used by the entity
        public HelmEquipment Helm { get; set; }
        public ChestEquipment Chest { get; set; }
        public GloveEquipment Gloves { get; set; }
        public LegEquipment Legs { get; set; }
        public BootEquipment Boots { get; set; }
        public RingEquipment Ring { get; set; }
        public WeaponEquipment Weapon { get; set; }

        //Constructor for the entity class
        //which handles creation of an
        //empty inventory and a series of 
        //empty equipment slots
        public Entity() {
            Inventory = new List<Equipment>();

            Helm = HelmEquipment.None();
            Chest = ChestEquipment.None();
            Gloves = GloveEquipment.None();
            Legs = LegEquipment.None();
            Boots = BootEquipment.None();
            Ring = RingEquipment.None();
            Weapon = WeaponEquipment.None();
        }

        //This is the method that draws the entity onto the screen
        public void Draw(RLConsole console, IMap map) {
            //If the cell that the entity is on hasn't been explored then the
            //entity will not be drawn
            if (!map.GetCell(X, Y).IsExplored) {
                return;
            }
            //If the entity is not within FOV then have the cell look like
            //a typical floor cell, otherwise draw the entity
            if (map.IsInFov(X, Y)) {
                console.Set(X, Y, colour, DungeonColours.floorBackground, symbol);
            } else {
                console.Set(X, Y, DungeonColours.floor, DungeonColours.floorBackground, '.');
            }
        }

    }
}
