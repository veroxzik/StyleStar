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
    public static class Globals
    {
        public static ContentManager ContentManager { get; set; }
        public static GraphicsDeviceManager GraphicsManager { get; set; }


        public static double BeatToWorldXUnits { get; set; } = 3;
        public static double Beat120ToWorldYUnits { get; set; } = 32;
        public static double SpeedScale { get; set; } = 1.0;
        public static double StepNoteHeightOffset { get; set; } = 5;
        public static double ShuffleNoteHeightOffset { get; set; } = 7.5;
        public static double ShuffleXOffset { get; set; } = 0.7;
        public static double YOffset { get; set; } = 1;
        public static float OverlapMultplier { get; set; } = -0.02f;
        public static int NumLanes = 16;
        public static float GradeZoneWidth { get; set; } = 48f;
        public static float NoteLaneAccentWidth { get; set; } = 3f;

        public static float FootWidth { get; set; } = 4f;

        public static Vector2 Origin = new Vector2(0, 0);
        public static Vector2 ItemOrigin = new Vector2(144, 212);
        public static Vector2 ItemOffset = new Vector2(-75, 130);

        public static Dictionary<SpriteFont, Tuple<float,float>> FontScalingFactor = new Dictionary<SpriteFont, Tuple<float, float>>();

        // public static double CurrentBpm { get; set; }
        public static List<BpmChangeEvent> BpmEvents { get; set; }

        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        public static BasicEffect Effect;

        public static Dictionary<string, SpriteFont> Font { get; set; }

        public static double CalcTransX(Note note)
        {
            return CalcTransX(note, Side.NotSet);
        }

        public static double CalcTransX(Note note, Side side)
        {
            double del = note.Width / 2;
            switch (side)
            {
                case Side.Left:
                    del = 0;
                    break;
                case Side.Right:
                    del = note.Width;
                    break;
                case Side.NotSet:
                default:
                    break;
            }

            return (note.LaneIndex - NumLanes / 2 + del) * BeatToWorldXUnits;
        }

        public static double CalcTransX(float index, float width, Side side)
        {
            double del = width / 2;
            switch (side)
            {
                case Side.Left:
                    del = 0;
                    break;
                case Side.Right:
                    del = width;
                    break;
                case Side.NotSet:
                default:
                    break;
            }

            return (index - NumLanes / 2 + del) * BeatToWorldXUnits;
        }

        public static double GetDistAtBeat(double beat)
        {
            // Determine which BPM change we're on (if any)
            var evt = BpmEvents.Where(x => beat >= x.StartBeat).LastOrDefault();
            if (evt == null)
                evt = BpmEvents[0];

            double curDist = 0;
            if(evt.StartBeat > 0)
            {
                // Add up all the distance prior
                for (int i = 0; i < BpmEvents.Count; i++)
                {
                    if (BpmEvents[i].StartBeat == evt.StartBeat)
                        i = BpmEvents.Count;
                    else
                    {
                        curDist += (BpmEvents[i + 1].StartBeat - BpmEvents[i].StartBeat) * Beat120ToWorldYUnits * 120 / BpmEvents[i].BPM * SpeedScale;
                    }
                }
            }

            return curDist + (beat - evt.StartBeat) * Beat120ToWorldYUnits * 120 / evt.BPM * SpeedScale;
        }

        public static double GetSecAtBeat(double beat)
        {
            // Determine which BPM change we're on (if any)
            var evt = BpmEvents.Where(x => beat >= x.StartBeat).LastOrDefault();
            if (evt == null)
                evt = BpmEvents[0];

            //double curTime = 0;
            //if (evt.StartBeat > 0)
            //{
            //    // Add up all the time prior
            //    for (int i = 0; i < BpmEvents.Count; i++)
            //    {
            //        if (BpmEvents[i].StartBeat == evt.StartBeat)
            //            i = BpmEvents.Count;
            //        else
            //            curTime = BpmEvents[i + 1].StartSeconds;
            //    }
            //}

            return evt.StartSeconds + ((beat - evt.StartBeat) / evt.BPM * 60);
        }

        public static void LoadTextures()
        {
            Textures["StepLeft"] = ContentManager.Load<Texture2D>("StepLeft");
            Textures["StepRight"] = ContentManager.Load<Texture2D>("StepRight");
            Textures["HoldLeft"] = ContentManager.Load<Texture2D>("HoldLeft");
            Textures["HoldRight"] = ContentManager.Load<Texture2D>("HoldRight");
            Textures["SlideLeft"] = ContentManager.Load<Texture2D>("SlideLeft");
            Textures["SlideRight"] = ContentManager.Load<Texture2D>("SlideRight");
            Textures["MotionUp"] = ContentManager.Load<Texture2D>("MotionUp");
            Textures["MotionDown"] = ContentManager.Load<Texture2D>("MotionDown");
            Textures["ShuffleLeft_R"] = ContentManager.Load<Texture2D>("ShuffleLeft_R");
            Textures["ShuffleLeft_L"] = ContentManager.Load<Texture2D>("ShuffleLeft_L");
            Textures["ShuffleRight_R"] = ContentManager.Load<Texture2D>("ShuffleRight_R");
            Textures["ShuffleRight_L"] = ContentManager.Load<Texture2D>("ShuffleRight_L");
            Textures["FootLeft"] = ContentManager.Load<Texture2D>("FootLeft");
            Textures["FootRight"] = ContentManager.Load<Texture2D>("FootRight");
            Textures["FootHold"] = ContentManager.Load<Texture2D>("FootHold");
            Textures["BeatMark"] = ContentManager.Load<Texture2D>("BeatMarker");

            Textures["PerfectGrade"] = ContentManager.Load<Texture2D>("PerfectGrade");
            Textures["GreatGrade"] = ContentManager.Load<Texture2D>("GreatGrade");
            Textures["GoodGrade"] = ContentManager.Load<Texture2D>("GoodGrade");
            Textures["BadGrade"] = ContentManager.Load<Texture2D>("BadGrade");
            Textures["MissGrade"] = ContentManager.Load<Texture2D>("MissGrade");

            Textures["HitTexture"] = ContentManager.Load<Texture2D>("HitTexture");

            Textures["SsActive"] = ContentManager.Load<Texture2D>("SongSelection_Active");
            Textures["SsBgLine"] = ContentManager.Load<Texture2D>("SongSelection_BgLine");
            Textures["SsFrame"] = ContentManager.Load<Texture2D>("SongSelection_Frame");
            Textures["SsAlbumFrame"] = ContentManager.Load<Texture2D>("SongSelection_AlbumFrame");
            Textures["SsItemBg"] = ContentManager.Load<Texture2D>("SongSelection_BG");
            Textures["SsAccentStar"] = ContentManager.Load<Texture2D>("SongSelection_AccentStar");
            Textures["SsAccentAlbum"] = ContentManager.Load<Texture2D>("SongSelection_AccentAlbum");
            Textures["SsDifficultyBg"] = ContentManager.Load<Texture2D>("SongSelection_DifficultyBG");

            Textures["SsFolderSelect"] = ContentManager.Load<Texture2D>("SongSelection_FolderSelect");
            Textures["SsSongSelect"] = ContentManager.Load<Texture2D>("SongSelection_SongSelect");
            Textures["SsGoBack"] = ContentManager.Load<Texture2D>("SongSelection_GoBack");

            Textures["FallbackJacket"] = ContentManager.Load<Texture2D>("Fallback_Jacket");

            Textures["GpLowerBG"] = ContentManager.Load<Texture2D>("Gameplay_LowerBG");

            Effect = new BasicEffect(GraphicsManager.GraphicsDevice);
        }
    }

    public enum Motion
    {
        NotSet,
        Up,
        Down
    }

    public enum Side
    {
        NotSet,
        Left,
        Right
    }

    public enum NoteType
    {
        Step,
        Hold,
        Slide,
        Shuffle,
        Motion
    }

    public enum Mode
    {
        MainMenu,
        Options,
        SongSelect,
        Loading,
        GamePlay,
        Results
    }

    public enum Difficulty
    {
        Easy = 0,
        Normal,
        Hard
    }
}
