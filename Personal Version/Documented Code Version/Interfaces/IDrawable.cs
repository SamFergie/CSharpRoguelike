using RLNET;
using RogueSharp;

namespace Roguelike.Interfaces {
    public interface IDrawable {

        //The colour of the object to be drawn,
        //the symbol of the object, 
        //along with the X and Y position
        RLColor colour { get; set; }
        char symbol { get; set; }
        int X { get; set; }
        int Y { get; set; }

        //Every object that can be drawn must
        //have an implementation of this draw method
        void Draw(RLConsole console, IMap map);
    }
}
