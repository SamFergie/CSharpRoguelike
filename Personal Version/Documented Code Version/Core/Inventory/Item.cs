using RLNET;
using Roguelike.Core.Inventory.Items;

namespace Roguelike.Core.Inventory {
    public class Item : Equipment {

        //This string is the message shown
        //when the player uses the item
        public string useMessage;

        //This string is the message shown
        //when the player previews the item
        public string previewMessage;

        //This flag determines if the
        //the player can use the item
        public bool isUseable = true;

        //This default use method simply removed from the
        //player's inventory and outputs the use message
        public virtual void Use() {
            //Validates that the player's inventory contains 
            //the current item object and that the item can
            //be used(i.e. doesn't need special requirements)
            if(Game.player.Inventory.Contains(this) && isUseable) {
                Game.player.Inventory.Remove(this);
                //Validates that the item has a use message
                //before trying to output one
                if(useMessage != null) {
                    Game.messageLog.Add(useMessage);
                }
            }
        }

        //This default method simply shows the preview string
        //for the current item object onto the provided console
        public virtual void Preview(RLConsole console, int x) {
            int yPos = 1;
            //Show a generic "Preview:" message at the top 
            console.Print(x, yPos, "Preview: ", RLColor.White, RLColor.Black);
            yPos += 2;
            //Adds a given number of lines whilst
            //outputting the message to the console
            yPos += console.Print(x, yPos, previewMessage, RLColor.White, RLColor.Black, 30);
            //Change the background colour for the preview box
            console.SetBackColor(x, 1, 30, yPos - 1, RLColor.Blend(RLColor.Gray, RLColor.Black, 0.25f));
        }
    }
}
