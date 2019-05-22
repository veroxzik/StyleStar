using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public class MotionCollection
    {
        public Motion CurrentMotion { get; set; }
        public double JumpBeat { get; set; }
        public double DownBeat { get; set; }

        public bool CheckHit(Note note)
        {
            if (note.Motion == Motion.NotSet)
                return false;

            if (note.Motion == Motion.Up && double.IsNaN(JumpBeat))
                return false;   // We can't check an UP against no jump
            else if (note.Motion == Motion.Down && double.IsNaN(DownBeat))
                return false;   // We can't check a DOWN against no down

            float diffMS = -999.0f;
            var beatCheck = note.Motion == Motion.Up ? JumpBeat : DownBeat;
            //diffMS = (float)(((note.BeatLocation - beatCheck) * 60 / Globals.CurrentBpm));
            diffMS = (float)(Globals.GetSecAtBeat(note.BeatLocation) - Globals.GetSecAtBeat(beatCheck));
            if (diffMS > MotionTiming.EarlyPerfect) // Too soon to hit
                return false;

            // All other are valid
            note.HitResult.WasHit = true;
            note.HitResult.Difference = diffMS;
            return true;
        }
    }
}
