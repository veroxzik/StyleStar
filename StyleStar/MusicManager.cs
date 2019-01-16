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


        public bool IsPlaying { get { return Bass.BASS_ChannelIsActive(streamHandle) == BASSActive.BASS_ACTIVE_PLAYING; } }
        public long SongLengthBytes { get; private set; }
        public double SongLengthSec { get; private set; }
        public double SongLengthBeats { get; private set; }
        public double SongBpm { get; set; }
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

        public bool LoadSong(string filename, double bpm)
        {
            streamHandle = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_STREAM_PRESCAN);
            SongBpm = bpm;
            Globals.CurrentBpm = SongBpm;
            SongLengthBytes = Bass.BASS_ChannelGetLength(streamHandle);
            SongLengthSec = Bass.BASS_ChannelBytes2Seconds(streamHandle, SongLengthBytes);
            SongLengthBeats = SongBpm * SongLengthSec / 60;
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
            return SongBpm * GetCurrentSec() / 60;
        }
    }
}
