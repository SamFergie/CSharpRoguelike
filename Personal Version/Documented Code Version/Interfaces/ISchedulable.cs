namespace Roguelike.Interfaces {
    public interface ISchedulable {

        //How long it takes for this schedulable
        //to get a turn (number of ticks inbetween turns)
        int Time { get; }

    }
}
