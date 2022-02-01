using Roguelike.Core.Entity_System;
using System.Linq;

namespace Roguelike.Core.Inventory.Items {
    public class Key : Item {

        //Each key has the same properties, since
        //the constructor takes no parameters
        public Key() {
            //Assigned properties
            Name = "Key";
            isUseable = true;
            //Use message simply states how the player unlocked a door.
            //Would need to be modified if a key could be used for other purposes
            useMessage = Game.player.Name + " used a key and unlocked a door.";
            //Preview message instructs the player that this key is very important
            previewMessage = "An ancient and rusted key that looks very important. Do not dispose!";
        }

        //This static method determines if an
        //entity has a key in their inventory
        public static bool DoesEntityHaveKey(Entity entity) {
            //Uses a predicate to find an enumerable of all items in the
            //inventory called "Key" if one such item exists then return true
            if(entity.Inventory.Where(i => i.Name == "Key").Count() > 0) {
                return true;
            } else {
                //Entity doesn't have a key so return false
                return false;
            }
        }

    }
}
