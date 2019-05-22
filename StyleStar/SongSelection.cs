using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public static class SongSelection
    {
        // Constants for song selection
        public static int BgHeight = 120;
        public static int BgWidth = 345;
        public static int BgBuffer = 5;
        public static int AlbumSize = 82;

        public static int RowOffset = 32;

        public static Point StartPoint = new Point(360, 250);
        public static Point AlbumPoint = new Point(189, 30);

        public static Rectangle TitleRect = new Rectangle(35, 55, 140, 80);
        public static Rectangle ArtistRect = new Rectangle(50, 20, 130, 40);

        public static Rectangle FolderCatRect = new Rectangle(56, 31, 130, 28);
        public static Rectangle FolderNameRect = new Rectangle(60, 64, 230, 52);

        //public static Rectangle DifficultRect = new Rectangle(252, BgBuffer, 72, BgHeight - BgBuffer * 2);

        public static void DrawSelectionFrame(SpriteBatch sb)
        {
            sb.Begin();
            sb.Draw(Globals.Textures["SongBG"], new Rectangle(StartPoint.X - BgBuffer, StartPoint.Y - BgBuffer, BgWidth + BgBuffer * 2, BgHeight + BgBuffer * 2), Color.Yellow);
            sb.End();
        }

        public static void DrawFolder(SpriteBatch sb, string category, string name, int index, bool active)
        {
            // Calc revised point
            int x = StartPoint.X - index * (RowOffset + BgBuffer);
            x -= active ? 0 : BgWidth + (int) (BgBuffer * 2.5);
            int y = StartPoint.Y + index * (BgHeight + BgBuffer);

            Rectangle tempFolderCatRect = new Rectangle(
                x + FolderCatRect.X,
                y + FolderCatRect.Y,
                FolderCatRect.Width,
                FolderCatRect.Height);

            Rectangle tempFolderNameRect = new Rectangle(
                x + FolderNameRect.X,
                y + FolderNameRect.Y,
                FolderNameRect.Width - BgBuffer,
                FolderNameRect.Height);

            sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            if (category == null || name == null)
            {
                //sb.Draw(Globals.Textures["SongBG"], new Rectangle(x, y, BgWidth, BgHeight), ThemeColors.InactiveSelectionColor);
                sb.Draw(Globals.Textures["SsOuterFrame"], new Rectangle(x, y, BgWidth, BgHeight), ThemeColors.InactiveSelectionColor);
                sb.Draw(Globals.Textures["SsMainFrame"], new Rectangle(x, y, BgWidth, BgHeight), ThemeColors.InactiveSelectionColor);

            }
            else
            {
                //sb.Draw(Globals.Textures["SongBG"], new Rectangle(x, y, BgWidth, BgHeight), ThemeColors.FolderBGColor);
                //sb.Draw(Globals.Textures["FolderOverlay"], new Rectangle(x, y, BgWidth, BgHeight), ThemeColors.FolderBGColorSub);
                if(index == 0)
                    sb.Draw(Globals.Textures["SsOuterFrame"], new Rectangle(x, y, BgWidth, BgHeight), Color.Yellow);
                else
                    sb.Draw(Globals.Textures["SsOuterFrame"], new Rectangle(x, y, BgWidth, BgHeight), ThemeColors.FolderBGColor);
                sb.Draw(Globals.Textures["SsMainFrame"], new Rectangle(x, y, BgWidth, BgHeight), ThemeColors.FolderBGColor);
                sb.Draw(Globals.Textures["SsFolderFrame"], new Rectangle(x, y, BgWidth, BgHeight), ThemeColors.FolderBGColorSub);
                //Util.DrawString(sb, Globals.Font["Bold"], category, tempFolderCatRect.Shift(2, 2), Color.Black);
                Util.DrawString(sb, Globals.Font["Bold"], category, tempFolderCatRect, Color.White);
                //Util.DrawString(sb, Globals.Font["Bold"], name, tempFolderNameRect.Shift(2, 2), Color.Black);
                Util.DrawString(sb, Globals.Font["Bold"], name, tempFolderNameRect, Color.White);
            }
            // If this is inactive, add a grayed out overlay to all nonzero index entries
            if (!active && index != 0)
                //sb.Draw(Globals.Textures["SongBG"], new Rectangle(x - 1, y, BgWidth + 2, BgHeight), Color.Lerp(Color.Black, Color.Transparent, 0.1f));
                sb.Draw(Globals.Textures["SsMask"], new Rectangle(x, y, BgWidth, BgHeight), Color.Lerp(Color.Black, Color.Transparent, 0.1f));

            sb.End();
        }
    }

    public class FolderParams
    {
        public SortType Type;
        public int Value;
        public string Category;
        public string Name;
    }

    public enum SortType
    {
        Title,
        Artist,
        Level,
        Genre
    }
}
