using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public class BpmChangeEvent
    {
        public double BPM { get; set; }
        public double StartBeat { get; set; }
        public double StartSeconds { get; set; }

        public BpmChangeEvent(double bpm, double beat)
        {
            BPM = bpm;
            StartBeat = beat;
        }
    }
}
