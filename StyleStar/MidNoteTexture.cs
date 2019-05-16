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
    public class MidNoteTexture : NoteTextureBase
    {
        public MidNoteTexture(Note _parent, Note _prevNote) : base(_parent, _prevNote)
        {
            if (!IsLoaded)
            {
                string tex = "";
                if (parent.Type == NoteType.Hold || parent.Type == NoteType.Shuffle)
                    tex = parent.Side == Side.Left ? "HoldLeft" : "HoldRight";
                else if (parent.Type == NoteType.Slide)
                    tex = parent.Side == Side.Left ? "SlideLeft" : "SlideRight";

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
            //var y1 = (parent.BeatLocation - currentBeat) * Globals.BeatToWorldYUnits;
            //var y2 = (prevNote.BeatLocation - currentBeat) * Globals.BeatToWorldYUnits;
            var curDist = Globals.GetDistAtBeat(currentBeat);
            var y1 = Globals.GetDistAtBeat(parent.BeatLocation) - curDist;
            var y2 = Globals.GetDistAtBeat(prevNote.BeatLocation) - curDist;

            // Offset y's if the note is a step note
            //if (prevNote.Type == NoteType.Step)
            //    y2 += Globals.StepNoteHeightOffset;
            /*else*/ if(prevNote.Type == NoteType.Shuffle)
                y2 += Globals.ShuffleNoteHeightOffset;
            if (parent.Type == NoteType.Shuffle)
                y1 -= Globals.ShuffleNoteHeightOffset;

            var topNote = parent.Type == NoteType.Hold || parent.Type == NoteType.Shuffle ? prevNote : parent;

            SetVerts(
                (float)Globals.CalcTransX(topNote, Side.Left),
                (float)Globals.CalcTransX(topNote, Side.Right),
                (float)y1,
                (float)Globals.CalcTransX(prevNote, Side.Left),
                (float)Globals.CalcTransX(prevNote, Side.Right),
                (float)y2);
            
        }
    }
}
