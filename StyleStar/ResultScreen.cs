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

        private static Vector2 gradeLabelsPoint = new Vector2(845, 580);
        private static Vector2 gradeLabelOffset = new Vector2(-24, 40);

        private static Vector2 gradeTotalPoint = new Vector2(730, 580);

        private static Vector2 accuracyLabel = new Vector2(820, 430);

        private static Vector2 starPoint = new Vector2(222, 180);

        private static float fontHeightMain = 25.0f;

        public static void Draw(SpriteBatch sb, NoteCollection song)
        {
            sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            // BG lines
            sb.Draw(Globals.Textures["ResultBg3"], Globals.Origin, song.Metadata.ColorAccent.IfNull(ThemeColors.BrightGreen));
            sb.Draw(Globals.Textures["ResultBg2"], Globals.Origin, song.Metadata.ColorBack.IfNull(ThemeColors.Purple));
            sb.Draw(Globals.Textures["ResultBg1"], Globals.Origin, song.Metadata.ColorFore.IfNull(ThemeColors.Pink));
            sb.Draw(Globals.Textures["ResultBgRight"], new Vector2(731, 0), Color.White);

            // Title
            sb.DrawStringFixedHeight(Globals.Font["RunningStart"], "RESULTS", new Vector2(Globals.WindowSize.X / 2, 30), Color.White, 50.0f, Justification.Center | Justification.Top, 2, Color.Black);

            // Grade Labels
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], "STYLISH", gradeLabelsPoint, ThemeColors.Stylish, fontHeightMain, Justification.Left | Justification.Bottom, 2, Color.Black);
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], "COOL", gradeLabelsPoint + gradeLabelOffset, ThemeColors.Good, fontHeightMain, Justification.Left | Justification.Bottom, 2, Color.Black);
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], "GOOD", gradeLabelsPoint + 2* gradeLabelOffset, ThemeColors.Bad, fontHeightMain, Justification.Left | Justification.Bottom, 2, Color.Black);
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], "MISS", gradeLabelsPoint + 3 * gradeLabelOffset, ThemeColors.Miss, fontHeightMain, Justification.Left | Justification.Bottom, 2, Color.Black);

            // Note Counts
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], song.PerfectCount.ToString("D4"), gradeTotalPoint, Color.White, fontHeightMain, Justification.Right | Justification.Bottom, 2, Color.Black);
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], song.GreatCount.ToString("D4"), gradeTotalPoint + gradeLabelOffset, Color.White, fontHeightMain, Justification.Right | Justification.Bottom, 2, Color.Black);
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], song.GoodCount.ToString("D4"), gradeTotalPoint + 2 * gradeLabelOffset, Color.White, fontHeightMain, Justification.Right | Justification.Bottom, 2, Color.Black);
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], song.MissCount.ToString("D4"), gradeTotalPoint + 3 * gradeLabelOffset, Color.White, fontHeightMain, Justification.Right | Justification.Bottom, 2, Color.Black);

            // Score
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], "Accuracy", accuracyLabel, Color.White, fontHeightMain, Justification.Right | Justification.Bottom, 2, Color.Black);
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], (song.CurrentScore / song.TotalNotes * 100.0f).ToString("F3") + "%", new Vector2(780, 500), Color.White, 50.0f, Justification.Right | Justification.Bottom, 2, Color.Black);

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
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], result, gradeTotalPoint - gradeLabelOffset, fontCol, fontHeightMain, Justification.Right | Justification.Bottom, 2, strokeCol);

            // Stars
            int id = GetStarID((float)(song.CurrentScore / song.TotalNotes * 100.0f));
            sb.Draw(Globals.Textures["Star" + id], starPoint, Color.White);

            // Song Info
            string titleFont = FontTools.ContainsJP(song.Metadata.Title) ? "JP" : "Franklin";
            string artistFont = FontTools.ContainsJP(song.Metadata.Artist) ? "JP" : "Franklin";

            sb.DrawStringFixedHeight(Globals.Font[titleFont], song.Metadata.Title, new Vector2(Globals.WindowSize.X / 2, 100), Color.White, fontHeightMain + 5.0f, Justification.Center | Justification.Top, 2, Color.Black);
            sb.DrawStringFixedHeight(Globals.Font[artistFont], song.Metadata.Artist, new Vector2(Globals.WindowSize.X / 2, 140), Color.White, fontHeightMain, Justification.Center | Justification.Top, 2, Color.Black);
            sb.DrawStringFixedHeight(Globals.Font["Franklin"], "Lv. " + song.Metadata.Level.ToString("D2"), new Vector2(Globals.WindowSize.X / 2, 170), Color.White, fontHeightMain, Justification.Center | Justification.Top, 2, Color.Black);

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
