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

        public static Color NullColor = new Color(0, 0, 0, 0);

        private static Color[] colorArray = new Color[] { Purple, Pink, Yellow, Green, Blue };

        public static Color GetColor(int index)
        {
            return colorArray[index % colorArray.Length];
        }
    }
}
