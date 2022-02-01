using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Systems {
    public class MessageLog {
        
        //Tge maximum number of lines that
        //the message log can show at once
        private int maxLines;

        //The width of the current console
        private int consoleWidth;
        //A tuple of each message and the colour
        //that they should be shown in
        private readonly Queue<Tuple<string, RLColor>> lines;

        //The console that messages
        //are to be shown on
        private RLConsole defaultConsole;

        //Default constructor only requires a console,
        //for messages to be shown on, be specified
        public MessageLog(RLConsole console) {
            //Assign variable from the console property
            defaultConsole = console;
            consoleWidth = console.Width - 2;
            maxLines = console.Height - 2;
            //Initialise list
            lines = new Queue<Tuple<string, RLColor>>();
        }

        //This method draws all messages to
        //the message log
        public void Draw() {
            defaultConsole.Clear();
            //Iterates through each message and print
            //using string and colour elements of tuple
            for (int i = 0; i < lines.Count; i++) {
                defaultConsole.Print(1, i + 1, lines.ElementAt(i).Item1, lines.ElementAt(i).Item2);
            }
        }

        //This method adds a message
        //to the message log
        public void Add(string message) {
            //If string can fit on one line
            if(message.Length <= consoleWidth) {
                //Add the message into the list of messages to be displayed
                lines.Enqueue(new Tuple<string, RLColor>(message, RLColor.White));
                //Add a blank message
                lines.Enqueue(new Tuple<string, RLColor>("", RLColor.Black));
            } else {
                //String can't fit on one line,
                //so find the number of lines required
                int numberOfLines = Convert.ToInt16(Math.Ceiling((double)message.Length / consoleWidth));
                //Split the overall message into separate messages
                //where the length of the smaller string is the console
                //width or the remaining message
                for (int i = 0; i < message.Length; i += consoleWidth) {
                    string line = message.Substring(i, Math.Min(consoleWidth, message.Length - i));
                    lines.Enqueue(new Tuple<string, RLColor>(line, RLColor.White));
                }
                //Add a blank message
                lines.Enqueue(new Tuple<string, RLColor>("", RLColor.Black));
            }
            //Truncate the list of messages is longer
            //than the max length, remove some lines
            while(lines.Count > maxLines) {
                lines.Dequeue();
            }
        }

        //This method adds a message
        //to the message log and takes
        //a colour paramter
        public void Add(string message, RLColor colour) {
            //If string can fit on one line
            if (message.Length <= consoleWidth) {
                //Add the message into the list of messages to
                //to displayed and uses the colour provided
                lines.Enqueue(new Tuple<string, RLColor>(message, colour));
                //Add a blank message
                lines.Enqueue(new Tuple<string, RLColor>("", RLColor.Black));
            } else {
                //String can't fit on one line,
                //so find the number of lines required
                int numberOfLines = Convert.ToInt16(Math.Ceiling((double)message.Length / consoleWidth));
                //Split the overall message into separate messages
                //where the length of the smaller string is the console
                //width or the remaining message
                for (int i = 0; i < message.Length; i += consoleWidth) {
                    string line = message.Substring(i, Math.Min(consoleWidth, message.Length - i));
                    lines.Enqueue(new Tuple<string, RLColor>(line, colour));
                }
                //Add a blank message
                lines.Enqueue(new Tuple<string, RLColor>("", RLColor.Black));
            }
            //Truncate the list of messages is longer
            //than the max length, remove some lines
            while (lines.Count > maxLines) {
                lines.Dequeue();
            }
        }
    }
}
