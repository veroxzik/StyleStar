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
    public class MotionTexture : NoteTextureBase
    {
        public MotionTexture(Note _parent) : base(_parent)
        {
            if (!IsLoaded)
            {
                string tex = "";
                if (parent.Type == NoteType.Motion)
                    tex = parent.Motion == Motion.Up ? "MotionUp" : "MotionDown";
                else
                    return;

                //texture = Globals.ContentManager.Load<Texture2D>(tex);
                texture = Globals.Textures[tex];
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
            var y1 = (parent.BeatLocation - currentBeat) * Globals.BeatToWorldYUnits + Globals.StepNoteHeightOffset;
            var y2 = (parent.BeatLocation - currentBeat) * Globals.BeatToWorldYUnits - Globals.StepNoteHeightOffset;

            SetVerts(
                (float)Globals.CalcTransX(parent, Side.Left),
                (float)Globals.CalcTransX(parent, Side.Right),
                (float)y1,
                (float)Globals.CalcTransX(parent, Side.Left),
                (float)Globals.CalcTransX(parent, Side.Right),
                (float)y2);
            
        }
    }
}
