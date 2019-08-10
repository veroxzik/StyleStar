using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public static class ResultScreen
    {
        private static Rectangle stageResultRect = new Rectangle(150, 50, 300, 60);
        private static Rectangle songTitleRect = new Rectangle(300, 220, 370, 70);
        private static Rectangle artistTitleRect = new Rectangle(300, 290, 370, 40);
        private static Vector2 stringOrigin = new Vector2(0, 0);
        private static Rectangle albumRect = new Rectangle(150, 200, 130, 130);

        private static Vector2 gradeLabelsPoint = new Vector2(836, 552);
        private static Vector2 gradeLabelOffset = new Vector2(-24, 40);

        private static Vector2 gradeTotalPoint = new Vector2(650, 552);

        private static Vector2 accuracyLabel = new Vector2(680, 408);

        private static Vector2 starPoint = new Vector2(222, 180);

        public static void Draw(SpriteBatch sb, NoteCollection song)
        {
            sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            // BG lines
            sb.Draw(Globals.Textures["ResultBg3"], Globals.Origin, song.Metadata.ColorAccent.IfNull(ThemeColors.BrightGreen));
            sb.Draw(Globals.Textures["ResultBg2"], Globals.Origin, song.Metadata.ColorBack.IfNull(ThemeColors.Purple));
            sb.Draw(Globals.Textures["ResultBg1"], Globals.Origin, song.Metadata.ColorFore.IfNull(ThemeColors.Pink));
            sb.Draw(Globals.Textures["ResultBgRight"], new Vector2(731, 0), Color.White);

            // Title
            sb.DrawStringJustify(Globals.Font["RunningStart"], "RESULTS", new Vector2(Globals.WindowSize.X / 2, 50), Color.White, 0.25f, Justification.Center | Justification.Top, 2, Color.Black);

            // Grade Labels
            sb.DrawStringStroke(Globals.Font["Franklin"], "STYLISH", gradeLabelsPoint, 2.0f, Color.Black, 0.15f, StrokeStyle.All);
            sb.DrawString(Globals.Font["Franklin"], "STYLISH", gradeLabelsPoint, ThemeColors.Stylish, 0, Globals.Origin, 0.15f, SpriteEffects.None, 0);
            sb.DrawStringStroke(Globals.Font["Franklin"], "COOL", gradeLabelsPoint + gradeLabelOffset, 2.0f, Color.Black, 0.15f, StrokeStyle.All);
            sb.DrawString(Globals.Font["Franklin"], "COOL", gradeLabelsPoint + gradeLabelOffset, ThemeColors.Good, 0, Globals.Origin, 0.15f, SpriteEffects.None, 0);
            sb.DrawStringStroke(Globals.Font["Franklin"], "GOOD", gradeLabelsPoint + 2* gradeLabelOffset, 2.0f, Color.Black, 0.15f, StrokeStyle.All);
            sb.DrawString(Globals.Font["Franklin"], "GOOD", gradeLabelsPoint + 2* gradeLabelOffset, ThemeColors.Bad, 0, Globals.Origin, 0.15f, SpriteEffects.None, 0);
            sb.DrawStringStroke(Globals.Font["Franklin"], "MISS", gradeLabelsPoint + 3 * gradeLabelOffset, 2.0f, Color.Black, 0.15f, StrokeStyle.All);
            sb.DrawString(Globals.Font["Franklin"], "MISS", gradeLabelsPoint + 3 * gradeLabelOffset, ThemeColors.Miss, 0, Globals.Origin, 0.15f, SpriteEffects.None, 0);

            // Note Counts
            sb.DrawStringStroke(Globals.Font["Franklin"], song.PerfectCount.ToString("D4"), gradeTotalPoint, 2.0f, Color.Black, 0.15f, StrokeStyle.All);
            sb.DrawString(Globals.Font["Franklin"], song.PerfectCount.ToString("D4"), gradeTotalPoint, Color.White, 0, Globals.Origin, 0.15f, SpriteEffects.None, 0);
            sb.DrawStringStroke(Globals.Font["Franklin"], song.GreatCount.ToString("D4"), gradeTotalPoint + gradeLabelOffset, 2.0f, Color.Black, 0.15f, StrokeStyle.All);
            sb.DrawString(Globals.Font["Franklin"], song.GreatCount.ToString("D4"), gradeTotalPoint + gradeLabelOffset, Color.White, 0, Globals.Origin, 0.15f, SpriteEffects.None, 0);
            sb.DrawStringStroke(Globals.Font["Franklin"], song.GoodCount.ToString("D4"), gradeTotalPoint + 2 * gradeLabelOffset, 2.0f, Color.Black, 0.15f, StrokeStyle.All);
            sb.DrawString(Globals.Font["Franklin"], song.GoodCount.ToString("D4"), gradeTotalPoint + 2 * gradeLabelOffset, Color.White, 0, Globals.Origin, 0.15f, SpriteEffects.None, 0);
            sb.DrawStringStroke(Globals.Font["Franklin"], song.MissCount.ToString("D4"), gradeTotalPoint + 3 * gradeLabelOffset, 2.0f, Color.Black, 0.15f, StrokeStyle.All);
            sb.DrawString(Globals.Font["Franklin"], song.MissCount.ToString("D4"), gradeTotalPoint + 3 * gradeLabelOffset, Color.White, 0, Globals.Origin, 0.15f, SpriteEffects.None, 0);

            // Score
            sb.DrawStringStroke(Globals.Font["Franklin"], "Accuracy", accuracyLabel, 2.0f, Color.Black, 0.15f, StrokeStyle.All);
            sb.DrawString(Globals.Font["Franklin"], "Accuracy", accuracyLabel, Color.White, 0, Globals.Origin, 0.15f, SpriteEffects.None, 0);
            sb.DrawStringJustify(Globals.Font["Franklin"], (song.CurrentScore / song.TotalNotes * 100.0f).ToString("F3") + "%", new Vector2(780, 504), Color.White, 0.35f, Justification.Right | Justification.Bottom, 2, Color.Black);

            // Clear/Fail String
            string result = "";
            Color fontCol = Color.White;
            Color strokeCol = Color.Black;
            switch (song.SongEnd)
            {
                case SongEndReason.Undefined:
                    break;
                case SongEndReason.Forfeit:
                    result = "Forfeited";
                    fontCol = Color.LightGray;
                    strokeCol = Color.Gray;
                    break;
                case SongEndReason.Failed:
                    result = "Failed";
                    fontCol = ThemeColors.FailedFont;
                    strokeCol = ThemeColors.FailedStroke;
                    break;
                case SongEndReason.Cleared:
                    if (song.MissCount > 0)
                    {
                        result = "Cleared";
                        fontCol = ThemeColors.ClearedFont;
                        strokeCol = ThemeColors.ClearedStroke;
                    }
                    else
                    {
                        result = "Full Combo";
                        fontCol = ThemeColors.FullComboFont;
                        strokeCol = ThemeColors.FullComboStroke;
                    }
                    break;
                default:
                    break;
            }
            sb.DrawStringJustify(Globals.Font["Franklin"], result, new Vector2(754, 518), fontCol, 0.15f, Justification.Right | Justification.Top, 2, strokeCol);

            // Stars
            int id = GetStarID((float)(song.CurrentScore / song.TotalNotes * 100.0f));
            sb.Draw(Globals.Textures["Star" + id], starPoint, Color.White);

            // Song Info
            sb.DrawStringJustify(Globals.Font["Franklin"], song.Metadata.Title, new Vector2(Globals.WindowSize.X / 2, 90), Color.White, 0.15f, Justification.Center | Justification.Top, 2, Color.Black);
            sb.DrawStringJustify(Globals.Font["Franklin"], song.Metadata.Artist, new Vector2(Globals.WindowSize.X / 2, 130), Color.White, 0.15f, Justification.Center | Justification.Top, 2, Color.Black);
            sb.DrawStringJustify(Globals.Font["Franklin"], "Lv. " + song.Metadata.Level.ToString("D2"), new Vector2(Globals.WindowSize.X / 2, 170), Color.White, 0.15f, Justification.Center | Justification.Top, 2, Color.Black);

            // Album cover?
            // sb.Draw(song.Metadata.AlbumImage, albumRect, Color.White);

            sb.End();
        }

        public static int GetStarID(float score)
        {
            if (score >= 98.0f)
                return 7;
            else if (score >= 95.0f)
                return 6;
            else if (score >= 90.0f)
                return 5;
            else if (score >= 80.0f)
                return 4;
            else if (score >= 60.0f)
                return 3;
            else if (score >= 30.0f)
                return 2;
            else if (score >= 10.0f)
                return 1;
            else
                return 0;
        }
    }
}
