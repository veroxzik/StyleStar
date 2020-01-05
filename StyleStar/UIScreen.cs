using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StyleStar
{
    public static class UIScreen
    {
        public static float TopRowLocation { get; set; } = 615f;
        public static float BottomRowLocation { get; set; } = 700f;

        private static Vector2 plotLocation = new Vector2();

        private static List<Label> labels = new List<Label>();

        public static void GenerateLabels(NoteCollection song)
        {
            labels.Clear();

            string titleFont = FontTools.ContainsJP(song.Metadata.Title) ? "JP" : "Franklin";
            string artistFont = FontTools.ContainsJP(song.Metadata.Artist) ? "JP" : "Franklin";

            // Top Row
            plotLocation.X = 60;
            plotLocation.Y = TopRowLocation;
            labels.Add(new Label(Globals.Font["Franklin"], "SCROLL", plotLocation, Color.White, Justification.Center, LabelType.FixedHeight, 10.0f));
            plotLocation.X = 150;
            labels.Add(new Label(Globals.Font["Franklin"], "ACCURACY", plotLocation, Color.White, Justification.Left, LabelType.FixedHeight, 10.0f));
            plotLocation.X = 1140;
            labels.Add(new Label(Globals.Font[titleFont], song.Metadata.Title, plotLocation, Color.White, Justification.Right, LabelType.FixedHeight, 40.0f));
            plotLocation.X = 1200;
            labels.Add(new Label(Globals.Font["Franklin"], Enum.GetName(typeof(Difficulty), song.Metadata.Difficulty).ToUpper(), plotLocation, Color.White, Justification.Center, LabelType.FixedHeight, 10.0f));

            // Bottom row
            plotLocation.X = 60;
            plotLocation.Y = BottomRowLocation;
            labels.Add(new Label(Globals.Font["Franklin"], Globals.SpeedScale.ToString("F1"), plotLocation, Color.White, Justification.Center | Justification.Bottom, LabelType.FixedHeight, 50.0f));
            plotLocation.X = 150;
            labels.Add(new Label(Globals.Font["Franklin"], (song.CurrentScore / song.TotalNotes * 100.0).ToString("000.000"), plotLocation, Color.White, Justification.Left | Justification.Bottom, LabelType.FixedHeight, 40.0f));
            plotLocation.X = 395;
            labels.Add(new Label(Globals.Font["Franklin"], "/ 100.000%", plotLocation, Color.White, Justification.Left | Justification.Bottom, LabelType.FixedHeight, 20.0f));
            plotLocation.X = 1140;
            labels.Add(new Label(Globals.Font[artistFont], song.Metadata.Artist, plotLocation, Color.White, Justification.Right | Justification.Bottom, LabelType.FixedHeight, 30.0f));
            plotLocation.X = 1200;
            labels.Add(new Label(Globals.Font["Franklin"], song.Metadata.Level.ToString("D2"), plotLocation, Color.White, Justification.Center | Justification.Bottom, LabelType.FixedHeight, 50.0f));
        }

        public static void UpdateSpeed()
        {
            labels[4].Text = Globals.SpeedScale.ToString("F1");
        }

        public static void Draw(SpriteBatch sb, NoteCollection song)
        {
            if (song == null)
                return;

            var log = Globals.DrawTempLog;

            sb.Begin();
            sb.Draw(Globals.Textures["GpLowerBG"], new Vector2(0, 599), Color.Black);
            string titleFont = FontTools.ContainsJP(song.Metadata.Title) ? "JP" : "Franklin";
            string artistFont = FontTools.ContainsJP(song.Metadata.Artist) ? "JP" : "Franklin";

            if (Globals.DrawProfiling)
                log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "UI: BG");

            foreach (var label in labels)
            {
                label.Draw(sb);

                if (Globals.DrawProfiling)
                {
                    if (labels.IndexOf(label) == 3)
                        log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "UI: Top Row");
                    if (labels.IndexOf(label) == 8)
                        log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "UI: Bottom Row");
                }
            }

            if (Globals.AutoMode != GameSettingsScreen.AutoMode.Off)
            {
                plotLocation.X = Globals.WindowSize.X - 10;
                plotLocation.Y = 10;
                string str = Globals.AutoMode == GameSettingsScreen.AutoMode.Auto ? "AUTO MODE ENABLED" : "AUTO DOWN ENABLED";
                sb.DrawStringJustify(Globals.Font["Franklin"], str, plotLocation, Color.White, 0.1f, Justification.Top | Justification.Right);
            }

            sb.End();
        }
    }
}
