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

        private Model model;
        private Matrix world;
        
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
        }

        public void PreloadTexture()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                var prevNote = i == 0 ? StartNote : Notes[i - 1];
                Notes[i].PreloadTexture(prevNote);
            }
            StartNote.PreloadTexture();
        }
    }
}
