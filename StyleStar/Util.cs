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
        static public void DrawString(this SpriteBatch spriteBatch, SpriteFont font, string strToDraw, Rectangle boundaries, Color color, Justification just = Justification.Center)
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

        public static void DrawStringJustify(this SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, float scale, Justification justification)
        {
            DrawStringJustify(sb, font, text, position, color, scale, justification, 0, new Color());
        }

        public static void DrawStringJustify(this SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, float scale, Justification justification, float strokeSize, Color strokeColor)
        {
            Vector2 size = font.MeasureString(text);
            float trueY = size.Y * scale;
            if (Globals.FontScalingFactor.ContainsKey(font))
                trueY = Globals.FontScalingFactor[font].Item1 * scale + Globals.FontScalingFactor[font].Item2;
            float xOffset = 0, yOffset = 0;
            if (justification.HasFlag(Justification.Right))
            {
                xOffset += -size.X * scale;
            }
            else if (justification.HasFlag(Justification.Center))
            {
                xOffset += -size.X * scale / 2;
            }
            if (!justification.HasFlag(Justification.Bottom) && !justification.HasFlag(Justification.Middle))
            {
                yOffset += -trueY;
            }
            else if (justification.HasFlag(Justification.Bottom))
            {
                yOffset += -size.Y * scale + trueY;
            }
            Vector2 offset = new Vector2(xOffset, yOffset);
            if(strokeSize > 0)
            {
                DrawStringStroke(sb, font, text, position + offset, strokeSize, strokeColor, scale, StrokeStyle.All);
            }
            sb.DrawString(font, text, position + offset, color, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
        }

        public static void DrawStringStroke(this SpriteBatch sb, SpriteFont font, string text, Vector2 position, float strokeSize, Color color, float scale)
        {
            DrawStringStroke(sb, font, text, position, strokeSize, color, scale, StrokeStyle.Corners);
        }

        public static void DrawStringStroke(this SpriteBatch sb, SpriteFont font, string text, Vector2 position, float strokeSize, Color color, float scale, StrokeStyle style)
        {
            sb.DrawString(font, text, new Vector2(position.X - strokeSize, position.Y - strokeSize), color, 0, Globals.Origin, scale, SpriteEffects.None, 0);
            sb.DrawString(font, text, new Vector2(position.X + strokeSize, position.Y - strokeSize), color, 0, Globals.Origin, scale, SpriteEffects.None, 0);
            sb.DrawString(font, text, new Vector2(position.X - strokeSize, position.Y + strokeSize), color, 0, Globals.Origin, scale, SpriteEffects.None, 0);
            sb.DrawString(font, text, new Vector2(position.X + strokeSize, position.Y + strokeSize), color, 0, Globals.Origin, scale, SpriteEffects.None, 0);
            if(style == StrokeStyle.All)
            {
                sb.DrawString(font, text, new Vector2(position.X - strokeSize, position.Y), color, 0, Globals.Origin, scale, SpriteEffects.None, 0);
                sb.DrawString(font, text, new Vector2(position.X + strokeSize, position.Y), color, 0, Globals.Origin, scale, SpriteEffects.None, 0);
                sb.DrawString(font, text, new Vector2(position.X, position.Y + strokeSize), color, 0, Globals.Origin, scale, SpriteEffects.None, 0);
                sb.DrawString(font, text, new Vector2(position.X, position.Y - strokeSize), color, 0, Globals.Origin, scale, SpriteEffects.None, 0);
            }
        }

        public static Color LerpBlackAlpha(this Color color, float ratio, float alpha)
        {
            var tempColor = Color.Lerp(Color.Black, Color.Transparent, alpha);
            return Color.Lerp(color, tempColor, ratio);
        }

        public static Color ParseFromHex(string input)
        {
            int r, g, b;
            if (input.StartsWith("#"))
                input = input.Remove(0, 1);
            r = int.Parse(input.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = int.Parse(input.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = int.Parse(input.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color(r, g, b);
        }

        public static Color IfNull(this Color c, Color other)
        {
            if (c == ThemeColors.NullColor)
                return other;
            else
                return c;
        }
    }

    [Flags]
    public enum Justification
    {
        Left    = 0x01,
        Right   = 0x02,
        Center  = 0x04,
        Top     = 0x08,
        Bottom  = 0x10,
        Middle  = 0x20
    }

    public enum StrokeStyle
    {
        Corners,
        All
    }
}
