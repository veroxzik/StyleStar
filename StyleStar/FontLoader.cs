using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StbTrueTypeSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StyleStar
{
    public static class FontLoader
    {
        public static int FontBitmapWidth = 8192;
        public static int FontBitmapHeight = 8192;

        // Copied / Adapted from: 
        // https://github.com/StbSharp/StbTrueTypeSharp/tree/master/samples/StbTrueTypeSharp.MonoGame.Test
        public static SpriteFont LoadFont(string path, float fontHeight, FontRange fontRange = FontRange.Latin)
        {
            var fontBaker = new FontBaker();

            fontBaker.Begin(FontBitmapWidth, FontBitmapHeight);
            if (fontRange == FontRange.Latin)
            {
                fontBaker.Add(File.ReadAllBytes(path), fontHeight, new[]
                {
                    CharacterRange.BasicLatin,
                    CharacterRange.Latin1Supplement,
                    CharacterRange.LatinExtendedA,
                    CharacterRange.LatinExtendedB,
                    CharacterRange.Greek
                });
            }
            else if (fontRange == FontRange.Japanese)
            {
                fontBaker.Add(File.ReadAllBytes(path), fontHeight, new[]
                {
                    CharacterRange.BasicLatin,
                    CharacterRange.Hiragana,
                    CharacterRange.Katakana,
                    CharacterRange.CjkSymbolsAndPunctuation,
                    CharacterRange.CjkUnifiedIdeographs
                });
            }

            var _charData = fontBaker.End();

            // Offset by minimal offset
            int minimumOffsetY = 10000;
            foreach (var pair in _charData.Glyphs)
            {
                if (pair.Value.YOffset < minimumOffsetY)
                {
                    minimumOffsetY = pair.Value.YOffset;
                }
            }

            var keys = _charData.Glyphs.Keys.ToArray();
            foreach (var key in keys)
            {
                var pc = _charData.Glyphs[key];
                pc.YOffset -= minimumOffsetY;
                _charData.Glyphs[key] = pc;
            }

            var rgb = new Color[FontBitmapWidth * FontBitmapHeight];
            for (var i = 0; i < _charData.Bitmap.Length; ++i)
            {
                var b = _charData.Bitmap[i];
                rgb[i].R = b;
                rgb[i].G = b;
                rgb[i].B = b;

                rgb[i].A = b;
            }

            var fontTexture = new Texture2D(Globals.GraphicsManager.GraphicsDevice, FontBitmapWidth, FontBitmapHeight);
            fontTexture.SetData(rgb);

            var glyphBounds = new List<Rectangle>();
            var cropping = new List<Rectangle>();
            var chars = new List<char>();
            var kerning = new List<Vector3>();

            var orderedKeys = _charData.Glyphs.Keys.OrderBy(a => a);
            foreach (var key in orderedKeys)
            {
                var character = _charData.Glyphs[key];

                var bounds = new Rectangle(character.X, character.Y,
                                        character.Width,
                                        character.Height);

                glyphBounds.Add(bounds);
                cropping.Add(new Rectangle(character.XOffset, character.YOffset, bounds.Width, bounds.Height));

                chars.Add((char)key);

                kerning.Add(new Vector3(0, bounds.Width, character.XAdvance - bounds.Width));
            }

            var constructorInfo = typeof(SpriteFont).GetTypeInfo().DeclaredConstructors.First();
            var spacing = cropping.Max(x => x.Height);
            return (SpriteFont)constructorInfo.Invoke(new object[]
            {
                fontTexture, glyphBounds, cropping,
                chars, spacing, 0, kerning, ' '
            });
        }

        public enum FontRange
        {
            Latin,
            Japanese
        }
    }

    public static class FontTools
    {
        public static bool ContainsJP(string s)
        {
            if (s.Any(c => c >= CharacterRange.Hiragana.Start && c <= CharacterRange.Hiragana.End))
                return true;
            if (s.Any(c => c >= CharacterRange.Katakana.Start && c <= CharacterRange.Katakana.End))
                return true;
            if (s.Any(c => c >= CharacterRange.CjkUnifiedIdeographs.Start && c <= CharacterRange.CjkUnifiedIdeographs.End))
                return true;
            if (s.Any(c => c >= CharacterRange.CjkSymbolsAndPunctuation.Start && c <= CharacterRange.CjkSymbolsAndPunctuation.End))
                return true;

            return false;
        }
    }

}
