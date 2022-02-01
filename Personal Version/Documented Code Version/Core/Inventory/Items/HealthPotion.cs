using RLNET;

namespace Roguelike.Core.Inventory.Items {
    public class HealthPotion : Item {

        //This stores the amount of health healed
        //when the health potion is used
        public int value { get; private set; }

        //This is the constructor that creates
        //a new health potion object. Only needs
        //1 parameter (v) which is how much health healed
        public HealthPotion(int v) {
            //The name of the potion contains the number healed
            Name = "+" + v.ToString() + " Health Potion";
            //Assigned properties
            value = v;
            isUseable = true;
            //The use message is that the player used a health potion
            useMessage = Game.player.Name + " used a health potion!";
            //The preview message gives a generic description
            //of what the potion would look like
            previewMessage = "A flask of red viscous liquid. Chunks of coagulated blood float gently throughout the solution";
        }

        //This is the overriden use method
        public override void Use() {
            //Adds health value to player's health value
            Game.player.Health += value;
            //If the player has more health then their max health
            //then restric the value to the player's max health
            if(Game.player.Health > Game.player.MaxHealth) {
                Game.player.Health = Game.player.MaxHealth;
            }
            //Call the base item use method (which removes the item
            //from their inventory and outputs the use message)
            base.Use();
        }

        //This is the overriden preview method
        public override void Preview(RLConsole console, int x) {
            int yPos = 1;
            //Show a generic "Preview: " message at the top
            console.Print(x, yPos, "Preview: ", RLColor.White, RLColor.Black);
            yPos += 2;
            //Shows the amount of health the potion restores
            console.Print(x, yPos, "Restores " + value + " health.", RLColor.White, RLColor.Black);
            yPos += 2;
            //Adds a given number of lines whilst
            //outputting the preview message to the console
            yPos += console.Print(x, yPos, previewMessage, RLColor.White, RLColor.Black, 30);
            //Change the background colour for the preview box
            console.SetBackColor(x, 1, 30, yPos - 1, RLColor.Blend(RLColor.Gray, RLColor.Black, 0.25f));
        }

    }
}
