using Roguelike.Core;
using System;

namespace Roguelike.Systems {
    public class DiceSystem {

        //This method takes a string in the form "XDY + Z"
        //and rolls X dice of max value Y and adds a number Z
        //the additional number is optional
        public int Roll(string toRoll) {
            int value = 0;
            //Index of "D" value
            int positionOfD = toRoll.IndexOf("D");
            //The number of dice to roll
            int numberOfDice = Convert.ToInt16(toRoll.Substring(0, positionOfD));
            //Does dice have an addition to it
            if (!toRoll.Contains(" + ")) {
                //For every dice, generate a number between 1 and the 
                //size of dice + 1 (inclusive, exclusive respectively)
                int sizeOfDice = Convert.ToInt16(toRoll.Substring(positionOfD + 1));
                for (int i = 0; i < numberOfDice; i++) {
                    value += Game.random.Next(1, sizeOfDice + 1);
                }
            } else {
                int positionOfS = toRoll.IndexOf(" ");
                //For every dice, generate a number between 1 and the 
                //size of dice + 1 (inclusive, exclusive respectively)
                int sizeOfDice = Convert.ToInt16(toRoll.Substring(positionOfD + 1, positionOfS - 1));
                for (int i = 0; i < numberOfDice; i++) {
                    value += Game.random.Next(1, sizeOfDice + 1);
                }
                //Add Z value to final output value
                value += Convert.ToInt16(toRoll.Substring(positionOfS + 2));
            }
            //Return final value (which is 0 by default)
            return value;
        }

        //This method takes a string in the form "XDY + Z"
        //and returns the minimum value that roll could have
        public int MinRoll(string toRoll) {
            int value = 0;
            //Index of "D" value
            int positionOfD = toRoll.IndexOf("D");
            //The number of dice to roll
            int numberOfDice = Convert.ToInt16(toRoll.Substring(0, positionOfD));
            //Does dice have an addition to it
            if (!toRoll.Contains(" + ")) {
                //For every dice, add 1
                for (int i = 0; i < numberOfDice; i++) {
                    value += 1;
                }
            } else {
                int positionOfS = toRoll.IndexOf(" ");
                //For every dice, add 1
                for (int i = 0; i < numberOfDice; i++) {
                    value += 1;
                }
                //Add Z value to final output value
                value += Convert.ToInt16(toRoll.Substring(positionOfS + 2));
            }
            //Return final value (which is 0 by default)
            return value;
        }

    }
}
