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
    public class Note
    {
        public bool IsLoaded { get; private set; }
        public double BeatLocation { get; private set; }
        public int LaneIndex { get; private set; }
        public int Width { get; set; }
        public NoteType Type { get; set; } = NoteType.Step;
        public Side Side { get; set; } = Side.NotSet;
        public Motion Motion { get; set; } = Motion.NotSet;
        public HitResult HitResult { get; set; } = new HitResult();

        private Model model;
        private Matrix world;
        private MidNoteTexture bgTexture;
        private NoteTextureBase noteTexture;
        
        public Note(double beatLoc, int laneIndex, int width, Side side)
        {
            BeatLocation = beatLoc;
            LaneIndex = laneIndex;
            Width = width;
            Side = side;
        }

        public Note(double beatLoc, int laneIndex, int width, Motion motion)
        {
            BeatLocation = beatLoc;
            LaneIndex = laneIndex;
            Width = width;
            Motion = motion;
            Type = NoteType.Motion;
        }

        public void PreloadTexture(UserSettings settings)
        {
            PreloadTexture(settings, null);
        }

        public void PreloadTexture(UserSettings settings, Note prevNote)
        {
            switch (Type)
            {
                case NoteType.Step:
                    if (noteTexture == null)
                        noteTexture = new StepNoteTexture(settings, this);
                    return;
                case NoteType.Motion:
                    if (noteTexture == null)
                        noteTexture = new MotionTexture(settings, this);
                    return;
                case NoteType.Hold:
                    if (bgTexture == null)
                        bgTexture = new MidNoteTexture(settings, this, prevNote);
                    break;
                case NoteType.Slide:
                    if (bgTexture == null)
                        bgTexture = new MidNoteTexture(settings, this, prevNote);
                    break;
                case NoteType.Shuffle:
                    if (bgTexture == null)
                        bgTexture = new MidNoteTexture(settings, this, prevNote);
                    if (noteTexture == null)
                        noteTexture = new ShuffleNoteTexture(settings, this, prevNote);
                    break;
                default:
                    break;
            }
        }

        public void Draw(double currentBeat, Matrix view, Matrix projection)
        {
            // Don't draw if this was hit
            if (HitResult.WasHit)
                return;

            if (Type == NoteType.Step)
            {
                //if (noteTexture == null)
                //    noteTexture = new StepNoteTexture(this);
                noteTexture.Draw(currentBeat, view, projection);
            }
            else if (Type == NoteType.Motion)
            {
                //if (noteTexture == null)
                //    noteTexture = new MotionTexture(this);
                noteTexture.Draw(currentBeat, view, projection);
            }
        }

        public void Draw(double currentBeat, Matrix view, Matrix projection, Note prevNote, int overlapIndex = 0, NoteType type = NoteType.All)
        {
            switch (Type)
            {
                case NoteType.Step:
                case NoteType.Motion:
                    Draw(currentBeat, view, projection);
                    return;
                case NoteType.Hold:
                    //if (bgTexture == null)
                    //    bgTexture = new MidNoteTexture(this, prevNote);
                    bgTexture.Draw(currentBeat, view, projection, overlapIndex);
                    break;
                case NoteType.Slide:
                    //if (bgTexture == null)
                    //    bgTexture = new MidNoteTexture(this, prevNote);
                    bgTexture.Draw(currentBeat, view, projection, overlapIndex);
                    break;
                case NoteType.Shuffle:
                    //if (bgTexture == null)
                    //    bgTexture = new MidNoteTexture(this, prevNote);
                    if (type == NoteType.All || type == NoteType.Hold || type == NoteType.Slide)
                        bgTexture.Draw(currentBeat, view, projection, overlapIndex);
                    //if (noteTexture == null)
                    //    noteTexture = new ShuffleNoteTexture(this, prevNote);
                    if (type == NoteType.All || type == NoteType.Shuffle)
                        ((ShuffleNoteTexture)noteTexture).Draw(currentBeat, view, projection, overlapIndex);
                    break;
                default:
                    break;
            }
        }
    }
}
