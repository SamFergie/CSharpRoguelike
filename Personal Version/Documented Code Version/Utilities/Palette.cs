using RLNET;

namespace Roguelike.Utilities {
    class Palette {

        //Primary colour palette, mostly dark-light browns
        //could be used for colours of interfaces or brown
        //objects in game
        public static RLColor primaryColourLightest = new RLColor(210, 207, 198);
        public static RLColor primaryColourLighter = new RLColor(176, 171, 155);
        public static RLColor primaryColour = new RLColor(143, 136, 112);
        public static RLColor primaryColourDarker = new RLColor(114, 109, 90);
        public static RLColor primaryColourDarkest = new RLColor(86, 82, 67);

        //Specific colours with specific purposes
        //and their name identifies their puspose
        public static RLColor DbWood = new RLColor(133, 76, 48);
        public static RLColor DbBrightWood = new RLColor(210, 125, 44);
        public static RLColor DbGold = new RLColor(218, 212, 94);
        public static RLColor DbPlayer = new RLColor(222, 238, 214);

        //Colour used by the player character
        public static RLColor Player = DbPlayer;
    }
}
