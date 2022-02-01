namespace Roguelike.Interfaces {
    public interface IEntity {

        //Generic
        int Awareness { get; set; }
        int Gold { get; set; }
        string Name { get; set; }

        //Combat
        int ArmourRating { get; set; }
        int Health { get; set; }
        int MaxHealth { get; set; }
        int Speed { get; set; }

        //Abilities
        int Strength { get; set; }
        int Constitution { get; set; }
        int Intelligence { get; set; }
        int Dexterity { get; set; }

    }
}
