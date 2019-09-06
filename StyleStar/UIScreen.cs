using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StyleStar
{
    public static class UIScreen
    {
        public static float TopRowLocation { get; set; } = 615f;
        public static float BottomRowLocation { get; set; } = 700f;

        private static Vector2 plotLocation = new Vector2();

        public static void Draw(SpriteBatch sb, NoteCollection song)
        {
            if (song == null)
                return;

            sb.Begin();
            sb.Draw(Globals.Textures["GpLowerBG"], new Vector2(0, 599), Color.Black);
            string titleFont = FontTools.ContainsJP(song.Metadata.Title) ? "JP" : "Franklin";
            string artistFont = FontTools.ContainsJP(song.Metadata.Artist) ? "JP" : "Franklin";

            plotLocation.X = 60;
            plotLocation.Y = TopRowLocation;
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], "SCROLL", plotLocation, Color.White, 10.0f, Justification.Center);
            plotLocation.X = 150;
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], "ACCURACY", plotLocation, Color.White, 10.0f, Justification.Left);
            plotLocation.X = 1140;
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], song.Metadata.Title, plotLocation, Color.White, 40.0f, Justification.Right);
            plotLocation.X = 1200;
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], Enum.GetName(typeof(Difficulty), song.Metadata.Difficulty).ToUpper(), plotLocation, Color.White, 10.0f, Justification.Center);

            plotLocation.X = 60;
            plotLocation.Y = BottomRowLocation;
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], Globals.SpeedScale.ToString("F1"), plotLocation, Color.White, 50.0f, Justification.Center | Justification.Bottom);
            plotLocation.X = 150;
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], (song.CurrentScore / song.TotalNotes * 100.0).ToString("000.000"), plotLocation, Color.White, 40.0f, Justification.Left | Justification.Bottom);
            plotLocation.X = 395;
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], "/ 100.000%", plotLocation, Color.White, 20.0f, Justification.Left | Justification.Bottom);
            plotLocation.X = 1140;
            sb.DrawStringFixedHeight(Globals.Font[titleFont], song.Metadata.Artist, plotLocation, Color.White, 30.0f, Justification.Right | Justification.Bottom);
            plotLocation.X = 1200;
            sb.DrawStringFixedHeight(Globals.Font[artistFont], song.Metadata.Level.ToString("D2"), plotLocation, Color.White, 50.0f, Justification.Center | Justification.Bottom);

            if (Globals.IsAutoModeEnabled)
            {
                plotLocation.X = Globals.WindowSize.X - 10;
                plotLocation.Y = 10;
                sb.DrawStringJustify(Globals.Font["Franklin"], "AUTO MODE ENABLED", plotLocation, Color.White, 0.1f, Justification.Top | Justification.Right);
            }

            sb.End();
        }
    }
}
