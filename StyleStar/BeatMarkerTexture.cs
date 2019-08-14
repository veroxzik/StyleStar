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
    public class BeatMarkerTexture : NoteTextureBase
    {
        private new BeatMarker parent;

        public BeatMarkerTexture(BeatMarker _parent) : base(null)
        {
            parent = _parent;
            if (!IsLoaded)
            {
                texture = Globals.Textures["BeatMark"];
                IsLoaded = true;
            }
        }

        public override void Draw(double currentBeat, Matrix view, Matrix projection)
        {
            SetVerts(currentBeat);

            base.Draw(currentBeat, view, projection);
        }

        private void SetVerts(double currentBeat)
        {
            var curDist = Globals.GetDistAtBeat(currentBeat);
            var y1 = Globals.GetDistAtBeat(parent.BeatLocation) - curDist - Globals.StepNoteHeightOffset + 1;
            var y2 = Globals.GetDistAtBeat(parent.BeatLocation) - curDist - Globals.StepNoteHeightOffset;

            SetVerts(
                (float)Globals.CalcTransX(0, 16, Side.Left),
                (float)Globals.CalcTransX(0, 16, Side.Right),
                (float)y1,
                (float)Globals.CalcTransX(0, 16, Side.Left),
                (float)Globals.CalcTransX(0, 16, Side.Right),
                (float)y2,
                0.05f);
            
        }
    }
}
