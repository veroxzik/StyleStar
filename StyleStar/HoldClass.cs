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
    public class Hold
    {
        public Note StartNote { get; private set; }
        public List<Note> Notes { get; private set; }

        public bool IsLoaded { get; private set; }

        public bool IsPlayerHolding { get; private set; }
        private QuadTexture HitTexture;
        
        public Hold(double beatLoc, int laneIndex, int width, Side side)
        {
            StartNote = new Note(beatLoc, laneIndex, width, side);
            Notes = new List<Note>();
        }

        public void AddNote(Note note)
        {
            // If this note is marked as hold, determine if it should be reclassified as a slide
            if(note.Type == NoteType.Hold)
            {
                var lastNote = Notes.Count == 0 ? StartNote : Notes.Last();
                if (lastNote.LaneIndex != note.LaneIndex || lastNote.Width != note.Width)
                    note.Type = NoteType.Slide;
            }

            Notes.Add(note);
        }

        public void Draw(double currentBeat, Matrix view, Matrix projection)
        {
            // Draw everything but the start note
            for (int i = 0; i < Notes.Count; i++)
            {
                var prevNote = i == 0 ? StartNote : Notes[i - 1];
                Notes[i].Draw(currentBeat, view, projection, prevNote);
            }

            // Draw start note
            StartNote.Draw(currentBeat, view, projection);

            // Draw hit texture if necessary
            if (IsPlayerHolding)
                HitTexture.Draw(view, projection);
        }

        public void Draw(double currentBeat, Matrix view, Matrix projection, int overlapIndex)
        {
            // Draw everything but the start note
            for (int i = 0; i < Notes.Count; i++)
            {
                var prevNote = i == 0 ? StartNote : Notes[i - 1];
                Notes[i].Draw(currentBeat, view, projection, prevNote, overlapIndex);
            }

            // Draw start note
            StartNote.Draw(currentBeat, view, projection);

            // Draw hit texture if necessary
            if (Notes.Last().BeatLocation < currentBeat)    // Sanity check first
                IsPlayerHolding = false;
            if (IsPlayerHolding)
                HitTexture.Draw(view, projection);
        }

        public void PreloadTexture()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                var prevNote = i == 0 ? StartNote : Notes[i - 1];
                Notes[i].PreloadTexture(prevNote);
            }
            StartNote.PreloadTexture();
            HitTexture = new QuadTexture(Globals.Textures["HitTexture"]);
        }

        public void CheckHold(TouchCollection tc, double currentBeat)
        {
            if (currentBeat < StartNote.BeatLocation)
            {
                IsPlayerHolding = false;
                return;
            }

            Note firstNote, secondNote;
            if (Notes[0].BeatLocation > currentBeat)
            {
                firstNote = StartNote;
                secondNote = Notes[0];
            }
            else if (Notes.Count < 2 || Notes.Last().BeatLocation < currentBeat)
            {
                IsPlayerHolding = false;
                return;
            }
            else
            {
                firstNote = Notes.Reverse<Note>().First(x => x.BeatLocation <= currentBeat);
                secondNote = Notes.First(x => x.BeatLocation >= currentBeat);
            }

            if(firstNote == null || secondNote == null)
            {
                IsPlayerHolding = false;
                return;
            }

            bool useFirstNote = false;
            double noteMin = 0, noteMax = 0;

            // Interpolate based on currentBeat
            switch (secondNote.Type)
            {
                case NoteType.Step:         // Shouldn't be possible
                case NoteType.Motion:
                    IsPlayerHolding = false;
                    return;
                case NoteType.Hold:         // Lane index and width is the same as the first note
                case NoteType.Shuffle:      // Actual shuffle motion is not calculated here (but perhaps we need to figure in some dead space?)
                    useFirstNote = true;
                    break;
                case NoteType.Slide:        // Lane index and width must be interpolated
                    double fNoteMin = Globals.CalcTransX(firstNote, Side.Left);
                    double fNoteMax = Globals.CalcTransX(firstNote, Side.Right);
                    double sNoteMin = Globals.CalcTransX(secondNote, Side.Left);
                    double sNoteMax = Globals.CalcTransX(secondNote, Side.Right);
                    double ratio = (currentBeat - firstNote.BeatLocation) / (secondNote.BeatLocation - firstNote.BeatLocation);
                    noteMin = (sNoteMin - fNoteMin) * ratio + fNoteMin;
                    noteMax = (sNoteMax - fNoteMax) * ratio + fNoteMax;
                    break;
                default:
                    break;
            }

            if(useFirstNote)
            {
                noteMin = Globals.CalcTransX(firstNote, Side.Left);
                noteMax = Globals.CalcTransX(firstNote, Side.Right);
            }

            var validPoints = tc.Points.Where(x => x.MinX < noteMax && x.MaxX > noteMin).ToList();
            if (validPoints.Count == 0)
            {
                IsPlayerHolding = false;
            }
            else
            {
                IsPlayerHolding = true;
                HitTexture.SetVerts((float)noteMax, (float)noteMin, (float)-Globals.StepNoteHeightOffset, (float)Globals.StepNoteHeightOffset, 0.1f);
            }
        }
    }
}
