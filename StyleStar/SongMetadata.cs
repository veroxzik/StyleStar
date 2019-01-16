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
        public double BPM { get; set; }
        public double PlaybackOffset { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Designer { get; set; }
        public Texture2D Thumbnail { get; set; }

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

                    if (Regex.IsMatch(line, "(#BPM)"))
                        BPM = Convert.ToDouble(StringExtensions.ParseString(line, "#BPM01: "));
                }
            }
            using (FileStream fs = new FileStream(FilePath + "thumb.png", FileMode.Open))
            {
                Thumbnail = Texture2D.FromStream(Globals.GraphicsManager.GraphicsDevice, fs);
            }
        }
    }
}
