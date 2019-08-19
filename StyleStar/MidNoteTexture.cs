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
        private int repetitionY = 80;

        public MidNoteTexture(UserSettings settings, Note _parent, Note _prevNote) : base(_parent, _prevNote)
        {
            if (!IsLoaded)
            {
                string tex = "";
                if (parent.Type == NoteType.Hold || parent.Type == NoteType.Shuffle)
                    tex = parent.Side == Side.Left ? settings.GetHoldLeftString() : settings.GetHoldRightString();
                else if (parent.Type == NoteType.Slide)
                    tex = parent.Side == Side.Left ? settings.GetSlideLeftString() : settings.GetSlideRightString();

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
            var y1 = Globals.GetDistAtBeat(parent.BeatLocation) - curDist;
            var y2 = Globals.GetDistAtBeat(prevNote.BeatLocation) - curDist;

            int reps = (int)(Math.Ceiling(Math.Abs(y1 - y2) / repetitionY));

            // Offset y's if the note is a step note
            //if (prevNote.Type == NoteType.Step)
            //    y2 += Globals.StepNoteHeightOffset;
            /*else*/
            if (prevNote.Type == NoteType.Shuffle)
                //y2 += Globals.ShuffleNoteHeightOffset;
                y2 -= Globals.ShuffleNoteHeightOffset;
            if (parent.Type == NoteType.Shuffle)
                y1 -= Globals.ShuffleNoteHeightOffset;

            var topNote = parent.Type == NoteType.Hold || parent.Type == NoteType.Shuffle ? prevNote : parent;

            SetVerts(
                Globals.CalcTransX(topNote, Side.Left),
                Globals.CalcTransX(topNote, Side.Right),
                y1,
                Globals.CalcTransX(prevNote, Side.Left),
                Globals.CalcTransX(prevNote, Side.Right),
                y2,
                reps);
            
        }
    }
}
