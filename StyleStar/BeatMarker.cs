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
    public class BeatMarker
    {
        public bool IsLoaded { get; private set; }
        public double BeatLocation { get; private set; }

        private Model model;
        private Matrix world;
        private NoteTextureBase noteTexture;
        
        public BeatMarker(double beatLoc)
        {
            BeatLocation = beatLoc;
        }

        public void PreloadTexture()
        {
            PreloadTexture(null);
        }

        public void PreloadTexture(Note prevNote)
        {
            if (noteTexture == null)
                noteTexture = new BeatMarkerTexture(this);
        }

        public void Draw(double currentBeat, Matrix view, Matrix projection)
        {
            if (noteTexture == null)
                noteTexture = new BeatMarkerTexture(this);
            noteTexture.Draw(currentBeat, view, projection);
        }
    }
}
