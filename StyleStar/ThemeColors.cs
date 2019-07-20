using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public static class ThemeColors
    {
        public static Color FolderBGColor = new Color(136, 0, 21);
        public static Color FolderBGColorSub = new Color(221, 0, 33);

        public static Color InactiveSelectionColor = new Color(64, 64, 64);

        public static Color Purple = new Color(170, 71, 186);
        public static Color Pink = new Color(249, 79, 142);
        public static Color Yellow = new Color(234, 237, 53);
        public static Color Green = new Color(28, 206, 40);
        public static Color Blue = new Color(97, 224, 225);
        public static Color BrightGreen = new Color(117, 252, 49);

        public static Color NullColor = new Color(0, 0, 0, 0);

        public static Color Stylish = new Color(255, 252, 104);
        public static Color Good = new Color(39, 234, 175);
        public static Color Bad = new Color(183, 120, 255);
        public static Color Miss = new Color(255, 70, 70);

        public static Color ClearedFont = new Color(159, 243, 255);
        public static Color ClearedStroke = new Color(0, 58, 138);

        public static Color FailedFont = new Color(255, 55, 164);
        public static Color FailedStroke = new Color(118, 19, 79);

        public static Color FullComboFont = new Color(207, 255, 138);
        public static Color FullComboStroke = new Color(0, 112, 19);

        private static Color[] colorArray = new Color[] { Purple, Pink, Yellow, Green, Blue };

        public static Color GetColor(int index)
        {
            return colorArray[index % colorArray.Length];
        }
    }
}
