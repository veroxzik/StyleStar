using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace StyleStar
{
    public class ShuffleNoteTexture : NoteTextureBase
    {
        private Side shuffleStartSide;

        public ShuffleNoteTexture(UserSettings settings, Note _parent, Note _prevNote) : base(_parent, _prevNote)
        {
            if (!IsLoaded)
            {
                //string tex = "";
                //if (parent.Type == NoteType.Shuffle)
                //    tex = parent.Side == Side.Left ? "ShuffleLeft" : "ShuffleRight";
                //else
                //    return;
                //if (parent.LaneIndex > prevNote.LaneIndex)
                //{
                //    tex += "_R";
                //    shuffleStartSide = Side.Right;
                //}
                //else
                //{
                //    tex += "_L";
                //    shuffleStartSide = Side.Left;
                //}
                string tex = "";
                if (parent.Type != NoteType.Shuffle)
                    return;
                if (parent.LaneIndex > prevNote.LaneIndex)
                {
                    tex = "ShuffleLeft";
                    shuffleStartSide = Side.Right;
                }
                else
                {
                    tex += "ShuffleRight";
                    shuffleStartSide = Side.Left;
                }
                tex += parent.Side == Side.Left ? settings.LeftColorString : settings.RightColorString;

                texture = Globals.Textures[tex];
                IsLoaded = true;
            }
        }

        public void Draw(double currentBeat, Matrix view, Matrix projection, int overlapIndex)
        {
            z = overlapIndex * Globals.OverlapMultplier;
            Draw(currentBeat, view, projection);
        }

        public override void Draw(double currentBeat, Matrix view, Matrix projection)
        {
            SetVerts(currentBeat);

            base.Draw(currentBeat, view, projection);
        }

        private void SetVerts(double currentBeat)
        {
            var curDist = Globals.GetDistAtBeat(currentBeat);
            var y1 = Globals.GetDistAtBeat(parent.BeatLocation) - curDist + Globals.ShuffleNoteHeightOffset;
            var y2 = Globals.GetDistAtBeat(parent.BeatLocation) - curDist - Globals.ShuffleNoteHeightOffset;

            var minXNote = parent.LaneIndex > prevNote.LaneIndex ? prevNote : parent;
            var maxXNote = parent.LaneIndex > prevNote.LaneIndex ? parent : prevNote;

            float leftOffset = shuffleStartSide == Side.Left ? (float)Globals.ShuffleXOffset : 0;
            float rightOffset = shuffleStartSide == Side.Right ? (float)Globals.ShuffleXOffset : 0;

            SetVerts(
                (float)Globals.CalcTransX(minXNote, Side.Left) + rightOffset,
                (float)Globals.CalcTransX(maxXNote, Side.Right) - leftOffset,
                (float)y1,
                (float)Globals.CalcTransX(minXNote, Side.Left) + rightOffset,
                (float)Globals.CalcTransX(maxXNote, Side.Right) - leftOffset,
                (float)y2);
            
        }
    }
}
