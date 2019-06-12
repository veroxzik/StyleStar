using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace StyleStar
{
    public class SongMetadata
    {
        public string FilePath { get; set; }
        public string ChartFullPath { get; set; }
        public string SongFilename { get; set; }
        public Dictionary<int, double> BpmIndex = new Dictionary<int, double>();
        public List<BpmChangeEvent> BpmEvents = new List<BpmChangeEvent>();
        public double PlaybackOffset { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Designer { get; set; }
        public Texture2D AlbumImage { get; set; }
        public string Jacket { get; set; }
        public Difficulty Difficulty { get; set; }
        public int Level { get; set; }
        public Color ColorFore { get; set; }
        public Color ColorBack { get; set; }
        public Color ColorAccent { get; set; }

        public SongMetadata() { }

        public SongMetadata(string fileName)
        {
            ChartFullPath = Path.GetFullPath(fileName);
            FilePath = Path.GetDirectoryName(fileName) + @"\";
            using (StreamReader sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    string parse;
                    if (StringExtensions.TrySearchTag(line, "WAVE", out parse))
                        SongFilename = parse;
                    if (StringExtensions.TrySearchTag(line, "WAVEOFFSET", out parse))
                        PlaybackOffset = Convert.ToDouble(parse);
                    if (StringExtensions.TrySearchTag(line, "TITLE", out parse))
                        Title = parse;
                    if (StringExtensions.TrySearchTag(line, "ARTIST", out parse))
                        Artist = parse;
                    if (StringExtensions.TrySearchTag(line, "DESIGNER", out parse))
                        Designer = parse;
                    if (StringExtensions.TrySearchTag(line, "DIFFICULTY", out parse))
                        Difficulty = (Difficulty)Convert.ToInt32(parse);
                    if (StringExtensions.TrySearchTag(line, "PLAYLEVEL", out parse))
                        Level = Convert.ToInt32(parse);
                    if (StringExtensions.TrySearchTag(line, "JACKET", out parse))
                        Jacket = parse;
                    if (StringExtensions.TrySearchTag(line, "COLORFORE", out parse))
                        ColorFore = Util.ParseFromHex(parse);
                    if (StringExtensions.TrySearchTag(line, "COLORBACK", out parse))
                        ColorBack = Util.ParseFromHex(parse);
                    if (StringExtensions.TrySearchTag(line, "COLORACCENT", out parse))
                        ColorAccent = Util.ParseFromHex(parse);

                    if (Regex.IsMatch(line, "(#BPM)"))
                    {
                        string[] bpmParse = line.Split(new string[] { "#BPM", ": " }, StringSplitOptions.RemoveEmptyEntries);
                        BpmIndex.Add(Convert.ToInt32(bpmParse[0]), Convert.ToDouble(bpmParse[1]));
                    }
                }
            }
            if (Jacket == "")
                AlbumImage = Globals.Textures["FallbackJacket"];
            else
            {
                using (FileStream fs = new FileStream(FilePath + Jacket, FileMode.Open))
                {
                    AlbumImage = Texture2D.FromStream(Globals.GraphicsManager.GraphicsDevice, fs);
                }
            }

        }

        public void Draw(SpriteBatch sb, int index)
        {
            // Calc revised point
            int x = SongSelection.StartPoint.X - index * (SongSelection.RowOffset + SongSelection.BgBuffer);
            int y = SongSelection.StartPoint.Y + index * (SongSelection.BgHeight + SongSelection.BgBuffer);

            // Title bound
            Rectangle tempTitleRect = new Rectangle(
                x + SongSelection.TitleRect.X, 
                y + SongSelection.TitleRect.Y, 
                SongSelection.TitleRect.Width, 
                SongSelection.TitleRect.Height);

            Rectangle tempArtistRect = new Rectangle(
                x + SongSelection.ArtistRect.X,
                y + SongSelection.ArtistRect.Y,
                SongSelection.ArtistRect.Width - SongSelection.BgBuffer,
                SongSelection.ArtistRect.Height);

            Rectangle levelTextRect = new Rectangle(
                x + 278,
                y + 32,
                40,
                30);

            sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            //sb.Draw(Globals.Textures["SongBG"], new Rectangle(x, y, SongSelection.BgWidth, SongSelection.BgHeight), Color.DarkGray);
            if (index == 0)
                sb.Draw(Globals.Textures["SsOuterFrame"], new Rectangle(x, y, SongSelection.BgWidth, SongSelection.BgHeight), Color.Yellow);
            else
                sb.Draw(Globals.Textures["SsOuterFrame"], new Rectangle(x, y, SongSelection.BgWidth, SongSelection.BgHeight), ThemeColors.FolderBGColor);
            sb.Draw(Globals.Textures["SsMainFrame"], new Rectangle(x, y, SongSelection.BgWidth, SongSelection.BgHeight), ThemeColors.FolderBGColor);
            sb.Draw(AlbumImage, new Rectangle(x + SongSelection.AlbumPoint.X, y + SongSelection.AlbumPoint.Y, SongSelection.AlbumSize, SongSelection.AlbumSize), Color.White);
            //sb.Draw(Globals.Textures["SongDifficulty"], new Rectangle(x + SongSelection.DifficultRect.X, y + SongSelection.DifficultRect.Y, SongSelection.DifficultRect.Width, SongSelection.DifficultRect.Height), Color.Green);
            sb.Draw(Globals.Textures["SsAlbumFrame"], new Rectangle(x, y, SongSelection.BgWidth, SongSelection.BgHeight), ThemeColors.FolderBGColor);
            sb.Draw(Globals.Textures["SsDifficultyFrame"], new Rectangle(x, y, SongSelection.BgWidth, SongSelection.BgHeight), Color.Green);
            //Util.DrawString(sb, Globals.Font["Bold"], Level.ToString(), levelTextRect.Shift(1, 1), Color.Black);
            Util.DrawString(sb, Globals.Font["Bold"], Level.ToString(), levelTextRect, Color.White);
            //sb.DrawString(Globals.Font["Bold"], "LEVEL", new Vector2(levelTextRect.X + 5 + 1, levelTextRect.Y + 105 - 1), Color.Black, (float)-Math.PI / 2, new Vector2(0, 0), 0.12f, new SpriteEffects(), 0);
            sb.DrawString(Globals.Font["Bold"], "LVL", new Vector2(levelTextRect.X, levelTextRect.Y + 75), Color.White, (float)-Math.PI / 2, new Vector2(0, 0), 0.12f, new SpriteEffects(), 0);
            //Util.DrawString(sb, Globals.Font["Regular"], Title, tempTitleRect.Shift(1, 1), Color.Black);
            Util.DrawString(sb, Globals.Font["Regular"], Title, tempTitleRect, Color.White);
            //Util.DrawString(sb, Globals.Font["Regular"], Artist, tempArtistRect.Shift(1, 1), Color.Black);
            Util.DrawString(sb, Globals.Font["Regular"], Artist, tempArtistRect, Color.White);
            sb.End();
        }
    }
}
