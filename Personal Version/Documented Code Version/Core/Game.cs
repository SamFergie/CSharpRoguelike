using RLNET;
using Roguelike.Core.Entity_System;
using Roguelike.Core.Inventory.Items;
using Roguelike.Generation;
using Roguelike.Generation.Cave;
using Roguelike.Generation.Custom_Maps;
using Roguelike.Systems;
using Roguelike.Utilities;
using Rougelike.Generation.Dungeon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace Roguelike.Core {
    public static class Game {

        //Random number generator
        public static Random random;

        //Readonly variables for the width and height of the console
        public static readonly int screenWidth = 140;
        public static readonly int screenHeight = 120;

        //Readonly variables for the width and height of the map subconsole
        //this is the largest of the subconsoles since this is where
        //most of the game happens
        private static readonly int mapWidth = 100;
        private static readonly int mapHeight = 100;
        private static RLConsole mapConsole;

        //Readonly variables for the width and height of the message subconsole
        //this appears in the bottom right of the screen
        private static readonly int messageWidth = 40;
        private static readonly int messageHeight = 40;
        private static RLConsole messageConsole;

        //Readonly variables for the width and height of the stats subconsole
        //this appears along the right column of the screen
        private static readonly int statWidth = 40;
        private static readonly int statHeight = 80;
        private static RLConsole statConsole;

        //Readonly variables for the width and height of the inventory subconsole
        //this appears along the top of the screen
        private static readonly int inventoryWidth = 100;
        private static readonly int inventoryHeight = 20;
        private static RLConsole inventoryConsole;

        private static readonly int menuWidth = screenWidth;
        private static readonly int menuHeight = screenHeight;
        private static RLConsole menuConsole;

        //This is the player object
        public static Player player { get; set; }

        //This is the dungeon map that will be displayed on the screen
        //if a new level were to be generated, this object would be updated
        public static DungeonMap dungeonMap { get; private set; }

        //Main console for containing sub consoles
        private static RLRootConsole rootConsole;

        //This is the command system, which will control the actions and 
        //behaviours of all entities
        public static CommandSystem commandSystem { get; private set; }

        //This is the message log which handles drawing
        //messages onto the message console
        public static MessageLog messageLog { get; private set; }

        //This is the pathfinding system, which can determine
        //shortest paths from any 2 given cells
        public static PathfindingSystem pathfindingSystem { get; private set; }

        //This is the scheduling system, which determines
        //which entity next has a turn
        public static SchedulingSystem schedulingSystem { get; private set; }

        //This is the dice system which rolls dice
        //used in describing combat actions
        public static DiceSystem diceSystem { get; private set; }

        //This is the inventory system which handles unequipping
        //and equipping items from the players inventory
        public static InventorySystem inventorySystem { get; private set; }

        //This is the system that parses the encounters file and 
        //translates that into generatable encounters
        public static EncounterSystem encounterSystem { get; private set; }

        //Sets up a new equipment generator object used by the dungeon
        //generator for giving monsters random pieces of equipment
        public static EquipmentGenerator equipmentGenerator;

        //This is the object used for detecting mouse input
        private static RLMouse mousePress;

        //Stores the number of updates until
        //the next mouse press becomes valid
        private static int pressCount = -1;

        private static ViewType currentView = ViewType.Menu;

        private static void AlternateMenuView() {
            if(currentView == ViewType.Game) {
                currentView = ViewType.Menu;
            }else if(currentView == ViewType.Menu) {
                currentView = ViewType.Game;
            }
        }

        //Main method (start point of whole program)
        static void Main(string[] args) {
            //Inroitialise the random number generator
            //before anything else so all systems can use it
            random = new Random();

            //Initialise the command system
            commandSystem = new CommandSystem();

            //Initialise the scheduling system
            schedulingSystem = new SchedulingSystem();

            //Initialise the dice system
            diceSystem = new DiceSystem();

            //Initialise the inventory system
            inventorySystem = new InventorySystem();

            //Initialise the encounter system
            encounterSystem = new EncounterSystem();

            //Sets up the new console with the file of ASCII characters at the correct width and height
            //also sets up the width and height of each character and the scale of those characters
            //in addition to the name of the console to be created
            rootConsole = new RLRootConsole("bitmapfile.png", screenWidth, screenHeight, 8, 8, 1, "Roguelike Game");

            //This sets up the mouse input detector
            mousePress = rootConsole.Mouse;

            //Sets up the new subconsoles with the correct width and height
            mapConsole = new RLConsole(mapWidth, mapHeight);
            statConsole = new RLConsole(statWidth, statHeight);
            messageConsole = new RLConsole(messageWidth, messageHeight);
            inventoryConsole = new RLConsole(inventoryWidth, inventoryHeight);

            menuConsole = new RLConsole(menuWidth, menuHeight);

            //Initialise the message log
            messageLog = new MessageLog(messageConsole);

            //Sets up the player object as a
            //new instance of the player class
            player = new Player();

            player.Inventory.Add(new Key());

            //Creates a new instance of the custom map loader
            MapGenerator mapGenerator = new MapGenerator(mapConsole);

            //Creates a new instance of the cave generator
            DungeonGenerator caveSystemGenerator = new DungeonGenerator(100, 100, 8, 12, 8, 12, 20, 20);

            //Create a new instance of the equipment generator
            equipmentGenerator = new EquipmentGenerator();

            dungeonMap = caveSystemGenerator.CreateMap(equipmentGenerator);

            //Loads a new level
            LoadNewLevel();

            //Adds the necessary methods to the update and render events
            //These are called everytime the console is updated and the render method 30/60 times per second
            rootConsole.Update += OnRootConsoleUpdate;
            rootConsole.Render += OnRootConsoleRender;

            StartPauseConsoleIncrement();

            //This starts the process of running the main console
            rootConsole.Run();
        }

        //This method loads a new level
        public static void LoadNewLevel() {
            //Clears the old map to make way for a new one
            //schedulingSystem.Clear();
            //This sets up a new dungeon generate and uses said generator
            //to create a new dungeon map and the sets up the initial
            //field of view for the player to use

            //DungeonGenerator dungeonGenerator = new DungeonGenerator(mapWidth, mapHeight, 8, 8, 14, 14, 18, 18);
            //dungeonMap = dungeonGenerator.CreateMap(equipmentGenerator);
            
            //Add a messgae stating the player has arrived
            messageLog.Add(player.Name + " enters a new level!");

            //Initialise the pathfinding system
            pathfindingSystem = new PathfindingSystem(dungeonMap.GetAllCells().ToList(), dungeonMap);

            //Sets the player's initial field of view
            dungeonMap.UpdatePlayerFieldOfView();
        }

        //This method handles a mouse press being detected
        private static bool MousePress() {
            //If the player clicked within the inventory window
            if (mousePress.X < inventoryWidth && mousePress.Y < inventoryHeight) {
                if (mousePress.LeftPressed) {
                    //Left mouse button was pressed
                    inventorySystem.InventoryLeftClick(mousePress.X, mousePress.Y);
                } else if (mousePress.RightPressed) {
                    //Right mouse button was pressed
                    inventorySystem.InventoryRightClick(mousePress.X, mousePress.Y);
                }
            } else if(mousePress.X < mapWidth && mousePress.Y > inventoryHeight){
                //The player clicked elsewhere probably
                //need to perform a ranged attack
                if (mousePress.LeftPressed) {
                    //Find the entity at the clicked position
                    Entity entityAtPosition = null;
                    foreach (Monster monster in dungeonMap.monsters) {
                        if(monster.monsterSize != 1) {
                            //Monster has a size larger than 1 so check every cell
                            for (int x = 0; x < monster.monsterSize; x++) {
                                for (int y = 0; y < monster.monsterSize; y++) {
                                    if (monster.X + x == mousePress.X && monster.Y + y == mousePress.Y - inventoryHeight) {
                                        entityAtPosition = monster;
                                        break;
                                    }
                                }
                            }
                        } else {
                            //Monster has a size of 1 so only check that cell
                            if (monster.X == mousePress.X && monster.Y == mousePress.Y - inventoryHeight) {
                                entityAtPosition = monster;
                                break;
                            }
                        }
                    }
                    //Trigger a ranged event with the player attacking the other entity
                    pressCount = -1;
                    return commandSystem.RangedEvent(player, entityAtPosition, mousePress.X, mousePress.Y - inventoryHeight);
                }
            }
            return false;
        }

        //Called whenever the console is updated
        private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e) {
            if(currentView == ViewType.Game) {
                OnRootConsoleUpdateGame(sender, e);
            } else {
                OnRootConsoleUpdatePause(sender, e);
            }
        }

        //Called whenever the console is updated whilst in game view
        private static void OnRootConsoleUpdateGame(object sender, UpdateEventArgs e) {
            //Bool to represent if the player has
            //performed a valid action on their turn yet
            bool didPlayerAct = false;

            //If either the left or right mouse buttons are pressed then the mouse
            //press method is called to handle any inventory operations
            if (mousePress.LeftPressed || mousePress.RightPressed) {
                if (pressCount == -1) {
                    //Alternate the menu view (used for showing a settings menu)
                    didPlayerAct = MousePress();
                }
            }
            //Increment the press counter
            pressCount++;
            //Detecs a key press
            RLKeyPress keyPress = rootConsole.Keyboard.GetKeyPress();
            //If it is the player's turn
            if (commandSystem.isPlayerTurn) {
                //If a key press was found then check what key it was
                //also checks that the player hasn't already acted
                if (keyPress != null && !didPlayerAct) {
                    //The key press was a movement key so
                    //therefore move the player
                    if (keyPress.Key == RLKey.Up || keyPress.Key == RLKey.W) {
                        didPlayerAct = commandSystem.MovePlayer(Direction.Up);
                    } else if (keyPress.Key == RLKey.Down || keyPress.Key == RLKey.S) {
                        didPlayerAct = commandSystem.MovePlayer(Direction.Down);
                    } else if (keyPress.Key == RLKey.Right || keyPress.Key == RLKey.D) {
                        didPlayerAct = commandSystem.MovePlayer(Direction.Right);
                    } else if (keyPress.Key == RLKey.Left || keyPress.Key == RLKey.A) {
                        didPlayerAct = commandSystem.MovePlayer(Direction.Left);
                    } else if (keyPress.Key == RLKey.Escape) {
                        //The key press was the escaoe key so
                        //close the main console
                        rootConsole.Close();
                    } else if (keyPress.Key == RLKey.Space) {
                        //Does nothing, lets the player skip a turn
                        didPlayerAct = true;
                    }else if (keyPress.Key == RLKey.Z) {
                        AlternateMenuView();
                    }
                }

                //If the player has performed a valid
                //action, end their turn
                if (didPlayerAct) {
                    commandSystem.EndPlayerTurn();
                }
            } else {
                //It isn't the player's turn so activate the
                //next entity/schedulable to get a turn
                commandSystem.ActivateNextSchedulable();
            }
        }

        //Called whenever the console is updated whilst in pause view
        private static void OnRootConsoleUpdatePause(object sender, UpdateEventArgs e) {
            //Bool to represent if the player has
            //performed a valid action on their turn yet
            bool didPlayerAct = false;

            //Detecs a key press
            RLKeyPress keyPress = rootConsole.Keyboard.GetKeyPress();
            //If it is the player's turn
            if (commandSystem.isPlayerTurn) {
                //If a key press was found then check what key it was
                //also checks that the player hasn't already acted
                if (keyPress != null && !didPlayerAct) {
                    //The key press was a movement key so
                    //therefore move the player
                    if (keyPress.Key == RLKey.Z) {
                        AlternateMenuView();
                    }
                }

                //If the player has performed a valid
                //action, end their turn
                if (didPlayerAct) {
                    commandSystem.EndPlayerTurn();
                }
            } else {
                //It isn't the player's turn so activate the
                //next entity/schedulable to get a turn
                commandSystem.ActivateNextSchedulable();
            }
        }

        public static bool shouldLoadLevel = false;

        //Called whenever the console is to be rendered 30/60 times per second
        private static void OnRootConsoleRender(object sender, UpdateEventArgs e) {
            if (currentView == ViewType.Game) {
                OnRootConsoleRenderGame(sender, e);
            }else if (currentView == ViewType.Pause) {
                OnRootConsoleRenderPause(sender, e);
            } else if  (currentView == ViewType.Menu) {
                OnRootConsoleRenderMenu(sender, e);
            }
        }

        public static int permYOffset = 1;

        private static void StartPauseConsoleIncrement() {
            TimeSpan start = TimeSpan.Zero;
            TimeSpan period = TimeSpan.FromSeconds(1);
            Timer timer = new Timer((e) => {
                IncrementYOffset();
            }, null, start, period);
        }

        private static void IncrementYOffset() {
            permYOffset += 1;
            if(permYOffset > 33) {
                permYOffset = 1;
            }
        }

        //Called whenever the console is to be rendered 30/60 times per second and in pause view
        private static void OnRootConsoleRenderPause(object sender, UpdateEventArgs e) {
            rootConsole.Clear();

            List<List<Color>> colours = ImageParser.ParseImage("Title");
            int xOffset = 1;
            int yOffset = 1;
            for (int x = 0; x < colours.Count; x++) {
                for (int y = 0; y < colours[0].Count; y++) {
                    menuConsole.Print(x + xOffset, y + yOffset, "#", new RLColor(colours[x][y].R, colours[x][y].G, colours[x][y].B));
                }
            }

            colours = ImageParser.ParseImage("Sword Logo");
            xOffset = menuWidth - colours.Count;
            permYOffset = 1;
            for (int x = 0; x < colours.Count; x++) {
                for (int y = 0; y < colours[0].Count; y++) {
                    menuConsole.Print(x + xOffset, y + permYOffset, "#", new RLColor(colours[x][y].R, colours[x][y].G, colours[x][y].B));
                }
            }

            colours = ImageParser.ParseImage("Shield Logo");
            xOffset = menuWidth - colours.Count - 5;
            yOffset = 33;
            for (int x = 0; x < colours.Count; x++) {
                for (int y = 0; y < colours[0].Count; y++) {
                    if(colours[x][y].R != 0 && colours[x][y].G != 0 && colours[x][y].B != 0) {
                        menuConsole.Print(x + xOffset, y + yOffset, "#", new RLColor(colours[x][y].R, colours[x][y].G, colours[x][y].B));
                    }
                }
            }

            RLConsole.Blit(menuConsole, 0, 0, menuWidth, menuHeight, rootConsole, 0, 0);
            rootConsole.Draw();
        }

        //Called whenever the console is to be rendered 30/60 times per second and in menu view
        private static void OnRootConsoleRenderMenu(object sender, UpdateEventArgs e) {
            rootConsole.Clear();

            List<List<Color>> colours = ImageParser.ParseImage("Title");
            int xOffset = 1;
            int yOffset = 1;
            for (int x = 0; x < colours.Count; x++) {
                for (int y = 0; y < colours[0].Count; y++) {
                    menuConsole.Print(x + xOffset, y + yOffset, "#", new RLColor(colours[x][y].R, colours[x][y].G, colours[x][y].B));
                }
            }

            colours = ImageParser.ParseImage("Sword Logo");
            xOffset = menuWidth - colours.Count;
            yOffset = 1;
            for (int x = 0; x < colours.Count; x++) {
                for (int y = 0; y < colours[0].Count; y++) {
                    menuConsole.Print(x + xOffset, y + yOffset, "#", new RLColor(colours[x][y].R, colours[x][y].G, colours[x][y].B));
                }
            }

            menuConsole.Print(1, colours.Count + 25, "Continue", RLColor.White);

            RLConsole.Blit(menuConsole, 0, 0, menuWidth, menuHeight, rootConsole, 0, 0);
            rootConsole.Draw();
        }

        //Called whenever the console is to be rendered 30/60 times per second and in game view
        private static void OnRootConsoleRenderGame(object sender, UpdateEventArgs e) {
            //Clears the inventory console to remove any lingering preview messages
            inventoryConsole.Clear();

            //Once the inventory console has been cleared, the preview window
            //should be checked for and drawn, if the cursor is within the inventory window
            if (mousePress.X < inventoryWidth && mousePress.Y < inventoryHeight) {
                inventorySystem.InventoryPreview(mousePress.X, mousePress.Y, inventoryConsole);
            }
            
            //Draws the player's stats onto the stat
            //console, also draws enemy stat information
            player.DrawStats(statConsole);

            //Draws the dungeon map onto the map console
            dungeonMap.Draw(mapConsole, statConsole);

            //Draws the player onto the map console
            player.Draw(mapConsole, dungeonMap);

            //Draws the players equipment
            player.DrawEquipment(inventoryConsole);

            //Draws the players inventory
            player.DrawInventory(inventoryConsole);

            //Draws the message log onto the message console
            messageLog.Draw();

            //Blits the subconsoles onto the main consoles and sets them in the
            //correct positions using the positions of other subconsoles as reference
            RLConsole.Blit(mapConsole, 0, 0, mapWidth, mapHeight, rootConsole, 0, inventoryHeight);
            RLConsole.Blit(statConsole, 0, 0, statWidth, statHeight, rootConsole, mapWidth, 0);
            RLConsole.Blit(messageConsole, 0, 0, messageWidth, messageHeight, rootConsole, mapWidth, statHeight);
            RLConsole.Blit(inventoryConsole, 0, 0, inventoryWidth, inventoryHeight, rootConsole, 0, 0);

            //Resets the counter when the user has
            //pressed the button for 3 update cycles
            if (pressCount >= 3) {
                pressCount = -1;
            }

            //Draws the information stored inside the root console object to the screen
            rootConsole.Draw();

            //If flag for loading a new level is set
            //then reset flag and load a new level
            if (shouldLoadLevel) {
                //Reset flag
                shouldLoadLevel = false;
                //Load new level
                LoadNewLevel();
            }
        }

    }
}
