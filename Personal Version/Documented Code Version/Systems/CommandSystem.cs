using RLNET;
using Roguelike.Core;
using Roguelike.Core.Entity_System;
using Roguelike.Core.Inventory;
using Roguelike.Core.Inventory.EquipmentTypes;
using Roguelike.Core.Inventory.Items;
using Roguelike.Interfaces;
using Roguelike.Utilities;
using RogueSharp;
using System;
using System.Collections.Generic;

namespace Roguelike.Systems {
    public class CommandSystem {

        //Flag if it is the player's turn
        public bool isPlayerTurn { get; set; }
        
        //This simply resets the flag regarding
        //if it is the player's turn
        public void EndPlayerTurn() {
            isPlayerTurn = false;
        }

        //This method is used to active the next entity/
        //schedulable that gets to have a turn
        public void ActivateNextSchedulable() {
            //Get the next schedulable to have an action
            ISchedulable schedulable = Game.schedulingSystem.Get();
            if(schedulable is Player) {
                //Player takes their turn
                isPlayerTurn = true;
                //Add the player back to the scheduling system
                Game.schedulingSystem.Add(Game.player);
            }else if(schedulable is Monster) {
                //Monster takes their action and this method is
                //recursively called again until it isplayer's turn
                Monster monster = schedulable as Monster;
                monster.PerformAction(this, Game.pathfindingSystem);
                Game.schedulingSystem.Add(monster);
                ActivateNextSchedulable();
            } else {
                //The schedulable isn't in the correct form
                //so an error is raised
                FormatException exception = new FormatException("The schedulable isn't in the correct format");
            }
        }

        //This method is used to move the player in a given direction
        //and allows the player to perform melee attacks on monsters
        public bool MovePlayer(Direction direction) {
            //Save the current position of the player
            //into 2 local variables to reduce how often
            //other objects are being accessed
            int x = Game.player.X;
            int y = Game.player.Y;

            //Alter the variables X and Y to reflect the
            //position that the player should move into
            switch (direction) {
                case Direction.Up:
                    y = Game.player.Y - 1;
                    break;
                case Direction.Down:
                    y = Game.player.Y + 1;
                    break;
                case Direction.Left:
                    x = Game.player.X - 1;
                    break;
                case Direction.Right:
                    x = Game.player.X + 1;
                    break;
                default:
                    return false;
            }

            //Return true if the player managed to
            //move into the new position, if not 
            //check if an attack could be made
            if (Game.dungeonMap.SetEntityPosition(Game.player, x, y)) {
                return true;
            } else {
                //Checks if a monster is present at
                //possible movement location
                Monster monster = null;
                foreach (Monster m in Game.dungeonMap.monsters) {
                    if(m.monsterSize != 1) {
                        if (Game.dungeonMap.IsCellPartOfMonster(x, y, m)) {
                            monster = m;
                            break;
                        }
                    }else if(m.monsterSize == 1 && m.X == x && m.Y == y) {
                        monster = m;
                        break;
                    }
                }
                //Returns the outcome of the 
                //attack on the found monster
                if (monster != null && Game.player.Weapon is MeleeEquipment) {
                    return Attack(Game.player, monster);
                }
                return false;
            }
        }

        //This method is used to move a monster to a given cell
        //and returns false if the move couldn't be made
        public bool MoveMonster(Monster monster, Cell cell) {
            if(monster.monsterSize == 1) {
                //Simply return the result of trying to move the monster
                return Game.dungeonMap.SetEntityPosition(monster, cell.X, cell.Y);
            } else {
                //If can't be moved, return false
                if (!Game.dungeonMap.SetEntityPosition(monster, cell.X, cell.Y, monster.monsterSize)) {
                    return false;
                }
                //Next to player so return false
                if (Game.dungeonMap.IsMonsterAdjacentToPlayer(monster)) {
                    return false;
                }
                //Otherwise return true has been moved
                return true;
            }
        }

        //This method completes the necessary checks and calculations
        //for a ranged attack before calling the 'Attack' method
        public bool RangedEvent(Entity attacker, Entity defender, int x, int y) {
            //Check that both entities aren't null and that an entity isn't trying
            //to attack itself, which could happen if the player clicked on itself
            if (attacker == null || defender == null && attacker != defender) {
                return false;
            }
            //Get distance to monster
            int dist = Convert.ToInt16(Math.Sqrt(Math.Pow((attacker.X - x), 2)) * Math.Pow((attacker.Y - y), 2));
            //Get the attackers weapon as a ranged weapon
            RangedEquipment weapon = attacker.Weapon as RangedEquipment;
            //Check the attacker has a ranged weapon
            if(weapon == null) {
                return false;
            }
            //Find the FOV for the attacker
            FieldOfView attackerFOV = new FieldOfView(Game.dungeonMap);
            attackerFOV.ComputeFov(attacker.X, attacker.Y, attacker.Awareness, true);
            //Check if within range
            if(dist <= weapon.Range) {
                if (Game.dungeonMap.IsInFov(x, y)) {
                    bool result = Attack(attacker, defender);
                    if(attacker is Player) {
                        return true;
                    }
                    return result;
                } else {
                    //Not within FOV
                }
            } else {
                //Not within range
                if(attacker is Player) {
                    Game.messageLog.Add(defender.Name + " is too far away. Try moving closer!", RLColor.Blend(RLColor.Red, RLColor.White));
                }
            }
            return false;
        }

        //This method is used when one entity
        //wants to push another entity 1 tile back
        public void Push(Entity pusher, Entity pushed) {
            Direction direction = Game.dungeonMap.GetDirectionBetweenTwoEntities(pusher, pushed);
            //Validates that the direction isn't null
            if(direction == Direction.None) {
                return;
            } else {
                //Calculates the new coordinates
                //based on the direction of pushing
                int x = Game.player.X;
                int y = Game.player.Y;
                if (direction == Direction.Left) {
                    x -= 1;
                }else if(direction == Direction.Right) {
                    x += 1;
                }else if(direction == Direction.Down) {
                    y += 1;
                }else if(direction == Direction.Up) {
                    y -= 1;
                }
                //Attempts to set the new position, and if successfull,
                //adds a message to the message log
                if(Game.dungeonMap.SetEntityPosition(pushed, x, y)) {
                    Game.messageLog.Add(pusher.Name + " pushed " + pushed.Name);
                }
            }
        }

        //This method is used to process a melee attack
        //made by the first parameter towards the second parameter
        public bool Attack(Entity attacker, Entity defender) {
            //Hit chance is a random dice roll of (1-20) + the
            //strength modifier of the attaking entity
            int hitRoll = Game.diceSystem.Roll("1D20") + attacker.StrengthMod;
            //Outcome is determined by the armour of the defender
            //and if minimum potential damage is greater than 0
            if(hitRoll > defender.ArmourRating && (attacker.StrengthMod + Game.diceSystem.MinRoll(attacker.Weapon.DamageRange)) > 0) {
                //Damage is based on weaponm's attack range and 
                //the strength modifier of the attacker
                int damage = Game.diceSystem.Roll(attacker.Weapon.DamageRange) + attacker.StrengthMod;
                if(attacker is Player) {
                    //Output player was attacked message
                    Game.messageLog.Add("Player attacked " + defender.Name + " and did " + damage + " damage", RLColor.Blend(RLColor.Green, RLColor.White));
                } else {
                    //Output monster was attacked message
                    Game.messageLog.Add("Player attacked by " + attacker.Name + " for " + damage + " damage", RLColor.Blend(RLColor.Red, RLColor.White));
                }
                Damage(damage, defender);
                return true;
            } else {
                //Output entity defended an attack message
                Game.messageLog.Add(defender.Name + " defended an attack by " + attacker.Name);
            }
            //Attack didn't hit to return false
            return false;
        }

        //This method deals some damage to the defender
        public void Damage(int damage, Entity defender) {
            defender.Health -= damage;
            if(defender.Health <= 0) {
                //Kill the defender, they're out of health
                Death(defender);
            }
        }

        //This method adds some random loot to the game map and
        //outputs an appropriate message to the command console
        private bool DropLoot(Monster died) {
            if(Game.random.Next(1, 101) < 50) {
                Treasure treasure = Treasure.GetRandomTreasure(died.X, died.Y, died);
                //If they entity has a key they should drop it
                if (Key.DoesEntityHaveKey(died)) {
                    treasure.equipment = new List<Equipment>() { new Key() };
                }
                //Add treasure to the map
                Game.dungeonMap.AddTreasure(treasure);
                //If no equipment is to be dropped then don't output such as message
                if(treasure.equipment == null || treasure.equipment.Count == 0) {
                    Game.messageLog.Add(died.Name + " died and dropped " + treasure.goldAmount + " gold!", RLColor.Blend(RLColor.Yellow, RLColor.Gray, 0.5f));
                } else {
                    //This variable holds the message to be output to the message log
                    string message = died.Name + " died and dropped " + treasure.goldAmount + " gold! They also dropped some equipment. Collect it to find out!";
                    //Output a message stating that some equipment has been dropped
                    Game.messageLog.Add(message, RLColor.Blend(RLColor.Yellow, RLColor.Gray, 0.5f));
                }
                //Return true as a death message was outputted
                return true;
            }
            //Return false as no death message was outputted
            return false;
        }

        //This method is used to kill an entity
        public void Death(Entity defender) {
            if(defender is Player) {
                //Show a game over screen/close the window/exit the program
            } else if (defender is Monster) {
                Game.dungeonMap.RemoveMonster((Monster)defender);
                //If loot has been dropped then no death message is needed
                //Otherwise output a death message
                if (!DropLoot((Monster)defender)) {
                    Game.messageLog.Add(defender.Name + " died");
                }
            }
        }
    }
}
