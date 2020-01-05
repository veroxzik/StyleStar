using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public class SelectableLabel : Label
    {
        public static Color SelectionColor { get; set; } = ThemeColors.Pink;
        public static Color InactiveColor { get; set; } = new Color(172, 172, 172);

        public List<string> Options { get; set; } = new List<string>();
        public int SelectedOption { get; set; } = 0;

        private string separator = "·";
        private List<Vector2> optionOffset = new List<Vector2>();

        public SelectableLabel(SpriteFont font, string[] options, Vector2 location, Justification justification, LabelType type, float parameter = 0.0f, Stroke stroke = null)
        {
            _font = font;
            _lastLocation = location;
            _inputJustification = justification;
            _inputType = type;
            _inputParameter = parameter;
            _inputStroke = stroke;
            GenerateStrokeOffsets();
            Options.AddRange(options);
            for (int i = 0; i < Options.Count; i++)
            {
                Text += Options[i];
                if (i < (Options.Count - 1))
                    Text += " " + separator + " ";
            }
            Update(location, InactiveColor, justification, type, parameter);

            string temp = "";
            optionOffset.Add(new Vector2(0f, 0f));
            for (int i = 0; i < Options.Count-1; i++)
            {
                temp += Options[i] + " " + separator + " ";
                var size = Util.MeasureString(_font, temp);
                optionOffset.Add(new Vector2(size.X * _scale, 0f));
            }
        }

        public new bool Draw(SpriteBatch sb)
        {
            bool ret = base.Draw(sb);
            if (!ret)
                return false;

            sb.DrawString(_font, Options[SelectedOption], _drawLocation + optionOffset[SelectedOption], SelectionColor, 0.0f, Vector2.Zero, _scale, SpriteEffects.None, 0.0f);

            return true;
        }

        public void ScrollRight()
        {
            SelectedOption++;
            if (SelectedOption >= Options.Count)
                SelectedOption = 0;
        }

        public void ScrollLeft()
        {
            SelectedOption--;
            if (SelectedOption < 0)
                SelectedOption = (Options.Count - 1);
        }
    }
}
