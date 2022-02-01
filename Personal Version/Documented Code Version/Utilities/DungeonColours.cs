using RLNET;

namespace Roguelike.Utilities {
    class DungeonColours {

        //Floor colours
        public static RLColor floorBackground = RLColor.Black;
        public static RLColor floor = Palette.DbBrightWood;
        public static RLColor floorBackgroundFov = RLColor.Blend(RLColor.Black, RLColor.White, .99f);
        public static RLColor floorFov = Palette.DbWood;

        //Door colours
        public static RLColor doorColourBackground = RLColor.Black;
        public static RLColor doorColour = new RLColor(140, 88, 41);

        public static RLColor doorColourBackgroundFov = RLColor.Black;
        public static RLColor doorColourFov = new RLColor(213, 165, 122);

        public static RLColor doorLockedColour = new RLColor(205, 177, 152);
        public static RLColor doorLockedColourFov = new RLColor(239, 230, 222);

        //Wall colours
        public static RLColor wallBackground = RLColor.Black;
        public static RLColor wall = RLColor.Gray;
        public static RLColor wallBackgroundFov = RLColor.Black;
        public static RLColor wallFov = RLColor.Blend(RLColor.Black, RLColor.White, .45f);

        //Monster colours
        public static RLColor koboldColour = new RLColor(226, 150, 131);
        public static RLColor goblinColour = new RLColor(41, 163, 41);
        public static RLColor dragonColour = RLColor.Blend(RLColor.Green, RLColor.White, .5f);
        public static RLColor lichColour = new RLColor(41, 47, 56);
    }
}
