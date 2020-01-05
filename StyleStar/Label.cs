using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public class Label
    {
        private static bool _drawBoundingBox = false;

        public string Text { get; set; }

        protected SpriteFont _font;
        protected Vector2 _drawLocation;
        protected Color _color;
        protected float _scale = -1.0f;
        protected Rectangle _boundingBox;

        protected List<Vector2> _strokeOffsets = new List<Vector2>();

        protected Vector2 _lastLocation;
        protected Justification _inputJustification;
        protected Rectangle _inputBoundingBox;
        protected LabelType _inputType;
        protected float _inputParameter = -1f;
        protected Stroke _inputStroke;

        public Label() { }

        public Label(SpriteFont font, string text)
        {
            Text = text;
            _font = font;
        }

        public Label(SpriteFont font, string text, Vector2 location, Color color, Justification justification, LabelType type, float parameter = 0.0f, Stroke stroke = null)
            : this(font, text)
        {
            _lastLocation = location;
            _inputJustification = justification;
            _inputType = type;
            _inputParameter = parameter;
            _inputStroke = stroke;
            GenerateStrokeOffsets();
            Update(location, color, justification, type, parameter);
        }

        public Label(SpriteFont font, string text, Rectangle boundingBox, Color color, Justification justification = Justification.Center | Justification.Middle, Stroke stroke = null)
            : this(font, text)
        {
            _inputJustification = justification;
            _inputType = LabelType.BoundingBox;
            _inputBoundingBox = boundingBox;
            _inputStroke = stroke;
            GenerateStrokeOffsets();
            Update(boundingBox, color, justification);
        }

        protected void GenerateStrokeOffsets()
        {
            if (_inputStroke == null)
                return;

            _strokeOffsets.Clear();
            _strokeOffsets.Add(new Vector2(-_inputStroke.Width, -_inputStroke.Width));
            _strokeOffsets.Add(new Vector2(0, -_inputStroke.Width));
            _strokeOffsets.Add(new Vector2(_inputStroke.Width, -_inputStroke.Width));
            _strokeOffsets.Add(new Vector2(-_inputStroke.Width, 0));
            _strokeOffsets.Add(new Vector2(_inputStroke.Width, 0));
            _strokeOffsets.Add(new Vector2(-_inputStroke.Width, _inputStroke.Width));
            _strokeOffsets.Add(new Vector2(0, -_inputStroke.Width));
            _strokeOffsets.Add(new Vector2(_inputStroke.Width, _inputStroke.Width));
        }

        public bool Draw(SpriteBatch sb)
        {
            // Return if no scale defined (Must call Update first)
            if (_scale == -1.0f)
                return false;

            if (_drawBoundingBox)
                sb.Draw(Globals.Textures["BeatMark"], _boundingBox, Color.Red);

            if(_inputStroke != null)
            {
                foreach (var offset in _strokeOffsets)
                {
                    sb.DrawString(_font, Text, _drawLocation + offset, _inputStroke.Color, 0.0f, Vector2.Zero, _scale, SpriteEffects.None, 0.0f);
                }
            }

            sb.DrawString(_font, Text, _drawLocation, _color, 0.0f, Vector2.Zero, _scale, SpriteEffects.None, 0.0f);

            return true;
        }

        public void Update(Vector2 location, Color color, Justification justification, LabelType type, float parameter = 0.0f)
        {
            _drawLocation = new Vector2();
            _color = color;
            
            switch (type)
            {
                case LabelType.Default:
                    _drawLocation = location;
                    if (parameter > 0.0f)
                        _scale = parameter;
                    var size = Util.MeasureString(_font, Text);
                    var width = size.X * _scale;
                    var height = size.Y * _scale;
                    _boundingBox.X = (int)location.X;
                    _boundingBox.Y = (int)location.Y;
                    if(justification.HasFlag(Justification.Right))
                    {
                        _boundingBox.X = (int)location.X - (int)width;
                        _drawLocation.X -= width;
                    }
                    else if (justification.HasFlag(Justification.Center))
                    {
                        _boundingBox.X = (int)location.X - (int)width / 2;
                        _drawLocation.X -= width / 2;
                    }
                    if (justification.HasFlag(Justification.Bottom))
                    {
                        _boundingBox.Y = (int)location.Y - (int)height;
                        _drawLocation.Y -= height;
                    }
                    else if (justification.HasFlag(Justification.Middle))
                    {
                        _boundingBox.Y = (int)location.Y - (int)height / 2;
                        _drawLocation.Y -= height / 2;
                    }             
                    _drawLocation.Y -= Util.GetTextOffset(_font, Text) * _scale;
                    break;
                case LabelType.FixedHeight:
                    _drawLocation = Util.GetStringFixedHeight(_font, Text, location, parameter, justification, out _scale, out _boundingBox);
                    break;
                case LabelType.FixedWidth:
                    break;
                case LabelType.BoundingBox:
                    Update(_inputBoundingBox.Shift((int)location.X, (int)location.Y), color, justification);
                    break;
                default:
                    break;
            }
        }

        public void Update(Rectangle boundingBox, Color color, Justification justification)
        {
            _drawLocation = new Vector2();
            _color = color;

            _drawLocation = Util.GetStringFromBoundingBox(_font, Text, boundingBox, justification, out _scale, out _boundingBox);
            _lastLocation.X = _boundingBox.X;
            _lastLocation.Y = _boundingBox.Y;
        }

        public void UpdateLoction(float x, float y)
        {
            // Only run update if the location is different
            if (_lastLocation.X != x || _lastLocation.Y != y)
            {
                _lastLocation.X = x;
                _lastLocation.Y = y;
                Update(_lastLocation, _color, _inputJustification, _inputType, _inputParameter);
            }
        }

        public class Stroke
        {
            public float Width { get; set; }
            public Color Color { get; set; }

            public Stroke(float width, Color color)
            {
                Width = width;
                Color = color;
            }
        }
    }

    public enum LabelType
    {
        Default,
        FixedHeight,
        FixedWidth,
        BoundingBox
    }
}
