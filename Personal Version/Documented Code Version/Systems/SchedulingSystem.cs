using Roguelike.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Systems {
    public class SchedulingSystem {

        //The current time of the scheduling system
        private int time;

        //A dictionary where each key contains all the schedulables
        //that get a turn when time == their key
        private readonly SortedDictionary<int, List<ISchedulable>> schedulables;

        //Default Constructor
        public SchedulingSystem() {
            time = 0;
            schedulables = new SortedDictionary<int, List<ISchedulable>>();
        }

        //This method adds a new schedulable 
        //object to the scheduling system
        public void Add(ISchedulable schedulable) {
            //Adds a new scheudulable at next possible turn
            int key = time + schedulable.Time;
            if (!schedulables.ContainsKey(key)) {
                //Key doesn't exist so create a new
                //empty list at the new key value
                schedulables.Add(key, new List<ISchedulable>());
            }
            //Add to the list of schedulables
            //at the calculated key value
            schedulables[key].Add(schedulable);
        }

        //This method removed a schedulable
        //from the scheduling system
        public void Remove(ISchedulable schedulable) {
            KeyValuePair<int, List<ISchedulable>> schedulableListFound = new KeyValuePair<int, List<ISchedulable>>(-1, null);
            //Iterate through every key value pair and if the schedulable
            //to be removed is in the value list, it is removed
            foreach (KeyValuePair<int, List<ISchedulable>> schedulableList in schedulables) {
                if (schedulableList.Value.Contains(schedulable)) {
                    schedulableListFound = schedulableList;
                    //Schedulable has been found, stop searching
                    break;
                }
            }
            //Remove the schedulable from the list and, if the 
            //list is empty, delete the entire key-value pair
            if(schedulableListFound.Value != null) {
                schedulableListFound.Value.Remove(schedulable);
                if(schedulableListFound.Value.Count <= 0) {
                    schedulables.Remove(schedulableListFound.Key);
                }
            }
        }

        //This method returns the next
        //schedulable to take their turn
        public ISchedulable Get() {
            //Get the next schedulable to take their turn
            KeyValuePair<int, List<ISchedulable>> firstGroup = schedulables.First();
            ISchedulable firstSchedulable = firstGroup.Value.First();

            //Remove it from the system
            Remove(firstSchedulable);

            //Update the current time of the system
            time = firstGroup.Key;

            //Return found value
            return firstSchedulable;
        }

        //This method returns
        //the current time
        public int GetTime() {
            return time;
        }

        //This method resets
        //the scheduling system
        public void Clear() {
            time = 0;
            schedulables.Clear();
        }

    }
}
