using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public class HitResult
    {
        public bool WasHit;
        public float Difference; // In milliseconds, + is early and - is late
    }

    public enum HitGrade
    {
        // All timing is guesses based on DDR
        Bad,    // 8 frames @ 60fps (133.3ms)
        Good,   // 6 frames @ 60fps (100ms)
        Great,  // 4 frames @ 60fps (66.6ms)
        Perfect // 2 frames @ 60fps (33.3ms)
    }

    public static class Timing
    {
        public static readonly float MissFlag = -999.0f;
        public static readonly float Bad = 1 / 60.0f * 8;
        public static readonly float Good = 1 / 60.0f * 6;
        public static readonly float Great = 1 / 60.0f * 4;
        public static readonly float Perfect = 1 / 60.0f * 2;
    }
}
