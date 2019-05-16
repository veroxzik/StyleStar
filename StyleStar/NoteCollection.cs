using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace StyleStar
{
    public class NoteCollection
    {
        public SongMetadata Metadata;

        public List<Note> Steps { get; private set; } = new List<Note>();
        public List<Hold> Holds { get; private set; } = new List<Hold>();
        public List<Note> Motions { get; private set; } = new List<Note>();
        public List<BeatMarker> Markers { get; private set; } = new List<BeatMarker>();

        public NoteCollection()
        {

        }

        public SongMetadata ParseFile(string fileName)
        {
            Metadata = new SongMetadata(fileName);

            Dictionary<int, int> holdIDlist = new Dictionary<int, int>();

            using (StreamReader sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    // Whatever metadata stuff isn't covered in SongMetadata

                    // Regex match for standard steps
                    if (Regex.Match(line, "[#][0-9]{3}[1]").Success)
                    {
                        var parsed = ParseLine(line);
                        double noteSub = 1.0 / parsed.Notes.Count;
                        for (int i = 0; i < parsed.Notes.Count; i++)
                        {
                            switch (parsed.Notes[i].Item1)
                            {
                                case 1: // Left Step
                                    Steps.Add(new Note(4 * (parsed.Measure + i * noteSub), parsed.LaneIndex, parsed.Notes[i].Item2, Side.Left));
                                    break;
                                case 2: // Right Step
                                    Steps.Add(new Note(4 * (parsed.Measure + i * noteSub), parsed.LaneIndex, parsed.Notes[i].Item2, Side.Right));
                                    break;
                                case 3: // Motion Up
                                    // Motions.Add(new Note(4 * (parsed.Measure + i * noteSub), parsed.LaneIndex, parsed.Notes[i].Item2, Motion.Up));
                                    Motions.Add(new Note(4 * (parsed.Measure + i * noteSub), 0, 16, Motion.Up));
                                    break;
                                case 4: // Motion Down
                                    //Motions.Add(new Note(4 * (parsed.Measure + i * noteSub), parsed.LaneIndex, parsed.Notes[i].Item2, Motion.Down));
                                    Motions.Add(new Note(4 * (parsed.Measure + i * noteSub), 0, 16, Motion.Down));
                                    break;
                                default:    // Rest notes / spacers (0) are ignored
                                    break;
                            }
                        }
                    }
                    // Regex match for hold/slides
                    else if (Regex.IsMatch(line, "[#][0-9]{3}[2-3]"))
                    {
                        var parsed = ParseLine(line);
                        double noteSub = 1.0 / parsed.Notes.Count;
                        for (int i = 0; i < parsed.Notes.Count; i++)
                        {
                            Side side = parsed.NoteClass == 2 ? Side.Left : Side.Right;

                            switch (parsed.Notes[i].Item1)
                            {
                                case 1: // Start a new note
                                    Holds.Add(new Hold(4 * (parsed.Measure + i * noteSub), parsed.LaneIndex, parsed.Notes[i].Item2, side));
                                    holdIDlist[parsed.NoteIdentifier] = Holds.Count - 1;
                                    break;
                                case 2: // End a hold note with no shuffle
                                    Holds[holdIDlist[parsed.NoteIdentifier]].AddNote(
                                        new Note(4 * (parsed.Measure + i * noteSub), parsed.LaneIndex, parsed.Notes[i].Item2, side) { Type = NoteType.Hold });
                                    holdIDlist.Remove(parsed.NoteIdentifier);
                                    break;
                                case 3: // End a hold note with a shuffle
                                    Holds[holdIDlist[parsed.NoteIdentifier]].AddNote(
                                        new Note(4 * (parsed.Measure + i * noteSub), parsed.LaneIndex, parsed.Notes[i].Item2, side) { Type = NoteType.Shuffle });
                                    holdIDlist.Remove(parsed.NoteIdentifier);   // Can't remove hold notes until all lines of that measure has been parsed
                                    break;
                                case 4: // Add a midpoint with no shuffle
                                    Holds[holdIDlist[parsed.NoteIdentifier]].AddNote(
                                        new Note(4 * (parsed.Measure + i * noteSub), parsed.LaneIndex, parsed.Notes[i].Item2, side) { Type = NoteType.Hold });
                                    break;
                                case 5: // Add a midpoint with a shuffle
                                    Holds[holdIDlist[parsed.NoteIdentifier]].AddNote(
                                        new Note(4 * (parsed.Measure + i * noteSub), parsed.LaneIndex, parsed.Notes[i].Item2, side) { Type = NoteType.Shuffle });
                                    break;
                                default:    // Rest notes / spacers (0) are ignored
                                    break;
                            }
                        }

                    }
                    // Parse BPM changes
                    else if (Regex.IsMatch(line, "[#][0-9]{3}(08)"))
                    {
                        var parsed = ParseLine(line);
                        double noteSub = 1.0 / parsed.Notes.Count;
                        for (int i = 0; i < parsed.Notes.Count; i++)
                        {
                            if (parsed.Notes[i].Item2 == 0)
                                break;
                            else
                                Metadata.BpmEvents.Add(new BpmChangeEvent(Metadata.BpmIndex[parsed.Notes[i].Item2], 4 * (parsed.Measure + i * noteSub)));
                        }
                    }
                }
            }

            // Add Beat Markers
            var noteLast = Steps.Count > 0 ? Steps.Max(x => x.BeatLocation) : 0;
            var holdLast = Holds.Count > 0 ? Holds.Max(x => x.Notes.Max(y => y.BeatLocation)) : 0;
            var motionLast = Motions.Count > 0 ? Motions.Max(x => x.BeatLocation) : 0;
            double lastBeat = Math.Max(noteLast, holdLast);
            lastBeat = Math.Ceiling(Math.Max(lastBeat, motionLast));
            for (int i = 0; i <= (int)lastBeat; i+= 4)
                Markers.Add(new BeatMarker(i));

            return Metadata;
        }

        public void PreloadTextures()
        {
            foreach (var motion in Motions)
                motion.PreloadTexture();
            foreach (var hold in Holds)
                hold.PreloadTexture();
            foreach (var step in Steps)
                step.PreloadTexture();
            foreach (var marker in Markers)
                marker.PreloadTexture();
        }

        private NoteParse ParseLine(string line)
        {
            NoteParse parse;
            parse.Notes = new List<Tuple<int, int>>();

            string[] split = line.Split(':');
            string meta = split[0];
            string notes = split[1].Replace(" ", "");

            parse.Measure = Convert.ToDouble(meta.Substring(1, 3));
            parse.NoteClass = Convert.ToInt32(meta.Substring(4, 1));
            parse.LaneIndex = Convert.ToInt32(meta.Substring(5, 1), 16);
            if (meta.Length == 7)
                //parse.NoteIdentifier = Convert.ToInt32(line.Substring(6, 1)[0]) - 'A';
                parse.NoteIdentifier = ParseAlphanumeric(line.Substring(6, 1));
            else
                parse.NoteIdentifier = -1;

            for (int i = 0; i < notes.Length; i += 2)
            {
                parse.Notes.Add(new Tuple<int, int>(Convert.ToInt32(notes.Substring(i, 1)), ParseNoteWidth(notes.Substring(i + 1, 1))));
            }

            return parse;
        }

        struct NoteParse
        {
            public double Measure;
            public int NoteClass;
            public int LaneIndex;
            public int NoteIdentifier;
            public List<Tuple<int, int>> Notes;
        }

        private int ParseAlphanumeric(string s)
        {
            if (Regex.IsMatch(s, "[0-9]"))
                return Convert.ToInt32(s);
            else
                return Convert.ToInt32(s.ToLower()[0]) - 'a' + 10;
        }

        private int ParseNoteWidth(string s)
        {
            if (Regex.IsMatch(s, "[0-9]"))
                return Convert.ToInt32(s);
            else
                return Convert.ToInt32(s[0]) - 'a' + 10;
        }
    }
}
