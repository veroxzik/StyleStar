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
        private static SpriteEffects effects = new SpriteEffects();

        public static void Draw(SpriteBatch sb, NoteCollection song)
        {
            sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            Util.DrawString(sb, Globals.Font["BoldItalic"], "STAGE RESULT", stageResultRect.Shift(2, 2), Color.Black);
            Util.DrawString(sb, Globals.Font["BoldItalic"], "STAGE RESULT", stageResultRect, Color.White);
            sb.Draw(song.Metadata.AlbumImage, albumRect, Color.White);
            Util.DrawString(sb, Globals.Font["Bold"], song.Metadata.Title, songTitleRect.Shift(2, 2), Color.Black, Justification.Left);
            Util.DrawString(sb, Globals.Font["Bold"], song.Metadata.Title, songTitleRect, Color.White, Justification.Left);
            Util.DrawString(sb, Globals.Font["Bold"], song.Metadata.Artist, artistTitleRect.Shift(2, 2), Color.Black, Justification.Left);
            Util.DrawString(sb, Globals.Font["Bold"], song.Metadata.Artist, artistTitleRect, Color.White, Justification.Left);
            sb.End();
        }
    }
}
