using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Roguelike.Utilities {
    public static class ImageParser {

        public static List<List<Color>> ParseImage(string fileToParse) {
            foreach (string file in Directory.EnumerateFiles("_Resources/Images", fileToParse + ".png")) {
                Bitmap bmp = new Bitmap(file);
                int width = bmp.Width;
                int height = bmp.Height;
                List<List<Color>> colours = new List<List<Color>>();
                for (int x = 0; x < width; x++) {
                    colours.Add(new List<Color>());
                    for (int y = 0; y < height; y++) {
                        colours[x].Add(bmp.GetPixel(x, y));
                    }
                }
                return colours;
            }
            return null;
        }


    }
}
