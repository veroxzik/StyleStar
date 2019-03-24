using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace StyleStar
{
    public class MusicManager
    {
        private int streamHandle = -1;
        private int bpmChangeIndex;


        public bool IsPlaying { get { return Bass.BASS_ChannelIsActive(streamHandle) == BASSActive.BASS_ACTIVE_PLAYING; } }
        public bool IsFinished { get { return GetCurrentSec() >= (SongLengthSec - (Offset / 1000)); } }
        public long SongLengthBytes { get; private set; }
        public double SongLengthSec { get; private set; }
        public List<BpmChangeEvent> BpmEvents { get; private set; }
        
        // Offset in milliseconds
        public double Offset { get; set; }

        public MusicManager()
        {
            bool success = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (!success)
                throw new Exception("BASS Library failed to initialize.");
        }

        ~MusicManager()
        {
            Bass.BASS_Free();
        }

        public bool LoadSong(string filename, List<BpmChangeEvent> bpmChanges)
        {
            streamHandle = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_STREAM_PRESCAN);
            BpmEvents = bpmChanges;
            BpmEvents = BpmEvents.OrderBy(x => x.StartBeat).ToList();
            for (int i = 0; i < BpmEvents.Count; i++)
            {
                if (i > 0)
                {
                    BpmEvents[i].StartSeconds = (BpmEvents[i].StartBeat - BpmEvents[i - 1].StartBeat) / BpmEvents[i - 1].BPM * 60 + BpmEvents[i - 1].StartSeconds;
                }
            }

            //SongBpm = bpm;
            Globals.CurrentBpm = BpmEvents[0].BPM;
            SongLengthBytes = Bass.BASS_ChannelGetLength(streamHandle);
            SongLengthSec = Bass.BASS_ChannelBytes2Seconds(streamHandle, SongLengthBytes);
            return streamHandle == 0 ? false : true;
        }

        public void Play()
        {
            Bass.BASS_ChannelPlay(streamHandle, false);
        }

        public void Pause()
        {
            Bass.BASS_ChannelPause(streamHandle);
        }

        public double GetCurrentSec()
        {
            var pos = Bass.BASS_ChannelGetPosition(streamHandle);
            return Bass.BASS_ChannelBytes2Seconds(streamHandle, pos) - (Offset / 1000);
        }

        public double GetCurrentBeat()
        {
            double sec = GetCurrentSec();
            var evt = BpmEvents.Where(x => sec >= x.StartSeconds).LastOrDefault();
            if (evt == null)
                evt = BpmEvents[0];
            //if (evt.Count() == 0)
            //{
            //    Globals.CurrentBpm = BpmEvents[0].BPM;
            //    return Globals.CurrentBpm * sec / 60;
            //}
            //else
            //{
                Globals.CurrentBpm = evt.BPM;    // This should never yield multiple results
                return (Globals.CurrentBpm * (sec - evt.StartSeconds) / 60) + evt.StartBeat;
            //}
        }       
    }
}
