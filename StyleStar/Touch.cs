using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StyleStar
{
    public class TouchCollection
    {
        public ConcurrentDictionary<uint, TouchPoint> Points = new ConcurrentDictionary<uint, TouchPoint>();
        //public List<TouchPoint> Points = new List<TouchPoint>();

        public bool UpdateID(uint id, Point rawPt)
        {
            var pt = Points.FirstOrDefault(x => x.Value.ID == id);
            pt.Value.RawX = rawPt.X;
            pt.Value.RawY = rawPt.Y;
            return true;
            //if (pt == null)
            //    return false;
            //else
            //{
            //    pt.RawX = rawPt.X;
            //    pt.RawY = rawPt.Y;
            //    return true;
            //}
        }

        public bool RemoveID(uint id)
        {
            TouchPoint temp;
            return Points.TryRemove(id, out temp);
        }

        public bool CheckHit(Note note)
        {
            var noteMin = Globals.CalcTransX(note, Side.Left);
            var noteMax = Globals.CalcTransX(note, Side.Right);

            var validPoints = Points.Where(x => x.Value.MinX < noteMax && x.Value.MaxX > noteMin).ToList();
            if (validPoints.Count == 0)
                return false;   // No need to modify hit result-- defaults to false

            validPoints.Sort((x, y) => Math.Abs(x.Value.Beat - note.BeatLocation).CompareTo(Math.Abs(y.Value.Beat - note.BeatLocation)));

            // Use the closest point and get the time difference
            //float diffMS = (float)(((note.BeatLocation - validPoints.First().Beat) * 60 / Globals.CurrentBpm));
            float diffMS = (float)(Globals.GetSecAtBeat(note.BeatLocation) - Globals.GetSecAtBeat(validPoints.First().Value.Beat));
            if (diffMS > NoteTiming.Bad) // Too soon to hit, just leave
                return false;
    
            // All other times are valid
            note.HitResult.WasHit = true;
            note.HitResult.Difference = diffMS;
            return true;
        }
    }

    public class TouchPoint
    {
        public int RawX { get; set; }  // Absolute value from the controller (0-1023)
        public int RawY { get; set; } // Absolute value from the controller (0-1023)
        public float X { get { return RawX * Globals.GradeZoneWidth / 1023 - Globals.GradeZoneWidth / 2; } }
        public int RawWidth { get; set; } // 0-1023
        public int RawHeight { get; set; } // 0-1023
        public float Width { get { return RawWidth * Globals.GradeZoneWidth / 1023; } }
        public float MinX { get { return X - Width / 2; } }
        public float MaxX { get { return X + Width / 2; } }
        public double Beat { get; private set; }
        public uint ID { get; set; } // 32bit ID (from Windows Message, etc.)

        private QuadTexture footTexture;
        private QuadTexture laneTexture;

        public TouchPoint(double beat)
        {
            Beat = beat;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            if (laneTexture == null)
                laneTexture = new QuadTexture(Globals.Textures["FootHold"]);
            laneTexture.SetVerts(MaxX, MinX, -(float)Globals.StepNoteHeightOffset, 300);
            laneTexture.Draw(view, projection);

            if (footTexture == null)
                footTexture = new QuadTexture(RawX <= 1024 / 2 ? Globals.Textures["FootLeft"] : Globals.Textures["FootRight"]);
            footTexture.SetVerts(X + Globals.FootWidth / 2, X - Globals.FootWidth / 2, -(float)Globals.StepNoteHeightOffset, (float)Globals.StepNoteHeightOffset, -0.05f);

            footTexture.Draw(view, projection);
        }
    }
}
