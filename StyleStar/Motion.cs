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

            float diffMS = -999.0f;
            var beatCheck = note.Motion == Motion.Up ? JumpBeat : DownBeat;
            diffMS = (float)(((note.BeatLocation - beatCheck) * 60 / Globals.CurrentBpm));
            if (diffMS > MotionTiming.EarlyPerfect) // Too soon to hit
                return false;

            // All other are valid
            note.HitResult.WasHit = true;
            note.HitResult.Difference = diffMS;
            return true;
        }
    }
}
