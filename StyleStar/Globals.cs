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
        public static double BeatToWorldYUnits { get; set; } = 32;
        public static double StepNoteHeightOffset { get; set; } = 5;
        public static double ShuffleNoteHeightOffset { get; set; } = 7.5;
        public static double ShuffleXOffset { get; set; } = 0.7;
        public static double YOffset { get; set; } = 1;
        public static float OverlapMultplier { get; set; } = -0.02f;
        public static int NumLanes = 16;
        public static float GradeZoneWidth { get; set; } = 48f;

        public static float FootWidth { get; set; } = 4f;

        public static double CurrentBpm { get; set; }

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

            Textures["PerfectGrade"] = ContentManager.Load<Texture2D>("PerfectGrade");
            Textures["GreatGrade"] = ContentManager.Load<Texture2D>("GreatGrade");
            Textures["GoodGrade"] = ContentManager.Load<Texture2D>("GoodGrade");
            Textures["BadGrade"] = ContentManager.Load<Texture2D>("BadGrade");
            Textures["MissGrade"] = ContentManager.Load<Texture2D>("MissGrade");

            Textures["HitTexture"] = ContentManager.Load<Texture2D>("HitTexture");

            Textures["SongSelectionBG"] = ContentManager.Load<Texture2D>("SongSelectionBG");
            Textures["SongBG"] = ContentManager.Load<Texture2D>("SongBG");
            Textures["SongDifficulty"] = ContentManager.Load<Texture2D>("SongDifficulty");
            Textures["FolderOverlay"] = ContentManager.Load<Texture2D>("FolderOverlay");

            Textures["SsOuterFrame"] = ContentManager.Load<Texture2D>("SongSelection_OuterFrame");
            Textures["SsMainFrame"] = ContentManager.Load<Texture2D>("SongSelection_MainFrame");
            Textures["SsDifficultyFrame"] = ContentManager.Load<Texture2D>("SongSelection_DifficultyFrame");
            Textures["SsAlbumFrame"] = ContentManager.Load<Texture2D>("SongSelection_AlbumFrame");
            Textures["SsFolderFrame"] = ContentManager.Load<Texture2D>("SongSelection_FolderFrame");
            Textures["SsArrow"] = ContentManager.Load<Texture2D>("SongSelection_Arrow");
            Textures["SsMask"] = ContentManager.Load<Texture2D>("SongSelection_Mask");

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
}
