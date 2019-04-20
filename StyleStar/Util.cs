using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public static class Util
    {
        /// This snippit modified from: http://bluelinegamestudios.com/posts/drawstring-to-fit-text-to-a-rectangle-in-xna/

        /// Draws the given string as large as possible inside the boundaries Rectangle without going
        /// outside of it.  This is accomplished by scaling the string (since the SpriteFont has a specific
        /// size).
        /// 
        /// If the string is not a perfect match inside of the boundaries (which it would rarely be), then
        /// the string will be absolutely-centered inside of the boundaries.
        static public void DrawString(SpriteBatch spriteBatch, SpriteFont font, string strToDraw, Rectangle boundaries, Color color, Justification just = Justification.Center)
        {
            Vector2 size = font.MeasureString(strToDraw);

            float xScale = (boundaries.Width / size.X);
            float yScale = (boundaries.Height / size.Y);

            // Taking the smaller scaling value will result in the text always fitting in the boundaires.
            float scale = Math.Min(xScale, yScale);

            // Figure out the location to absolutely-center it in the boundaries rectangle.
            int strWidth = (int)Math.Round(size.X * scale);
            int strHeight = (int)Math.Round(size.Y * scale);
            Vector2 position = new Vector2();
            switch (just)
            {
                case Justification.Left:
                    position.X = boundaries.X;
                    position.Y = boundaries.Y;
                    break;
                case Justification.Right:
                    break;
                case Justification.Center:
                    position.X = (((boundaries.Width - strWidth) / 2) + boundaries.X);
                    position.Y = (((boundaries.Height - strHeight) / 2) + boundaries.Y);
                    break;
                default:
                    break;
            }

            // A bunch of settings where we just want to use reasonable defaults.
            float rotation = 0.0f;
            Vector2 spriteOrigin = new Vector2(0, 0);
            float spriteLayer = 0.0f; // all the way in the front
            SpriteEffects spriteEffects = new SpriteEffects();

            // Draw the string to the sprite batch!
            spriteBatch.DrawString(font, strToDraw, position, color, rotation, spriteOrigin, scale, spriteEffects, spriteLayer);
        } // end DrawString()

        public static Rectangle Shift (this Rectangle rect, int x, int y)
        {
            return new Rectangle(rect.X + x, rect.Y + y, rect.Width, rect.Height);
        }

        public static double Dot(this Vector3 vect1, Vector3 vect2)
        {
            return vect1.X * vect2.X + vect1.Y * vect2.Y + vect1.Z * vect2.Z;
        }
    }

    public enum Justification
    {
        Left,
        Right,
        Center
    }
}
