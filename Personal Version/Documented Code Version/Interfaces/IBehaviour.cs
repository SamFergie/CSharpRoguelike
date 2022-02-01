using Roguelike.Core.Entity_System;
using Roguelike.Systems;

namespace Roguelike.Interfaces {
    public interface IBehaviour {

        //Each behaviour must contain an 'Act' method which must 
        //take the monster to act upon, the command system for combat actions
        //and the pathfinding system so the monster could move to a different cell
        bool Act(Monster monster, CommandSystem commandSystem, PathfindingSystem pathfindingSystem);

    }
}
