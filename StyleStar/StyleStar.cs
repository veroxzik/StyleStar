using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace StyleStar
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class StyleStar : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Vector3 cameraPos = new Vector3(0, -44, 23);
        Vector3 cameraTarget = new Vector3(0, 36, 2);

        private Matrix view;
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800 / 480f, 0.1f, 300f);

        QuadTexture gradeZone;
        QuadTexture noteLanes;
        Texture2D background;
        Rectangle bgRect;

        SpriteFont debugFont;
        float updateRate;
        float drawRate;
        float drawRateMin = float.PositiveInfinity;

        private Mode currentMode = Mode.SongSelect;

        // Music
        MusicManager musicManager = new MusicManager();

        NoteCollection currentSongNotes;
        SongMetadata currentSongMeta;

        // Song Selection
        List<SongMetadata> songlist = new List<SongMetadata>();
        int currentSongIndex = 0;

        KeyboardState prevState;

        TouchCollection touchCollection = new TouchCollection();
        MotionCollection motionCollection = new MotionCollection();
        GradeCollection gradeCollection = new GradeCollection();

        // Hit Debug
        double hitBeat = 0;
        double closestNoteBeat = 0;
        Keys[] touchkeys = new Keys[] { Keys.A, Keys.S, Keys.D, Keys.F, Keys.J, Keys.K, Keys.L, Keys.OemSemicolon };
        Dictionary<Keys, int> KeyDictionary = new Dictionary<Keys, int>();
        Random random = new Random();

        // DEBUG
        bool enableProfiling = false;
        bool drawFpsCounter = true;
        Stopwatch stopwatch = new Stopwatch();
        List<DrawCycleEventLog> DrawCycleLog = new List<DrawCycleEventLog>();
        List<Tuple<long, string>> timeVals = new List<Tuple<long, string>>();

        public StyleStar()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Globals.ContentManager = Content;
            Globals.GraphicsManager = graphics;

            //this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 60.0f);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 640 * 2;
            graphics.PreferredBackBufferHeight = 360 * 2;
            graphics.PreferMultiSampling = false;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            GraphicsDevice.PresentationParameters.MultiSampleCount = 8;
            graphics.ApplyChanges();

            IsFixedTimeStep = false;

            view = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.UnitY);

            bgRect = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Globals.LoadTextures();

            gradeZone = new QuadTexture(Content.Load<Texture2D>("GradeZone"));
            gradeZone.SetVerts(Globals.GradeZoneWidth / 2, -(Globals.GradeZoneWidth / 2), (float)-Globals.StepNoteHeightOffset, Globals.GradeZoneWidth / 2, -(Globals.GradeZoneWidth / 2), (float)Globals.StepNoteHeightOffset, -0.12f);
            noteLanes = new QuadTexture(Content.Load<Texture2D>("NoteLanes"));
            noteLanes.SetVerts(Globals.GradeZoneWidth / 2, -(Globals.GradeZoneWidth / 2), -20, Globals.GradeZoneWidth / 2, -(Globals.GradeZoneWidth / 2), 300, -0.15f);
            background = Content.Load<Texture2D>("Background");

            debugFont = Content.Load<SpriteFont>("DebugFont");

            // Load songs
            DirectoryInfo di = new DirectoryInfo("Songs");
            var folders = di.EnumerateDirectories();
            foreach (var folder in folders)
            {
                var files = folder.EnumerateFiles();
                var chart = files.FirstOrDefault(f => f.FullName.EndsWith(".sus"));
                if(chart != null)
                    songlist.Add(new SongMetadata(chart.FullName));
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState kbState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || kbState.IsKeyDown(Keys.Escape))
                Exit();

            if ((kbState.IsKeyDown(Keys.RightAlt) || kbState.IsKeyDown(Keys.LeftAlt)) && kbState.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter))
                graphics.ToggleFullScreen();

            switch (currentMode)
            {
                case Mode.MainMenu:
                    break;
                case Mode.Options:
                    break;
                case Mode.SongSelect:
                    if (kbState.IsKeyDown(Keys.Down) && !prevState.IsKeyDown(Keys.Down))
                        currentSongIndex = currentSongIndex > 0 ? --currentSongIndex : 0;

                    if (kbState.IsKeyDown(Keys.Up) && !prevState.IsKeyDown(Keys.Up))
                        currentSongIndex = currentSongIndex < (songlist.Count - 1) ? ++currentSongIndex : songlist.Count - 1;

                    if (kbState.IsKeyDown(Keys.Enter) && kbState.IsKeyUp(Keys.LeftAlt) && kbState.IsKeyUp(Keys.RightAlt) && !prevState.IsKeyDown(Keys.Enter))
                    {
                        LoadSong(songlist[currentSongIndex]);
                        currentMode = Mode.GamePlay;
                    }
                        
                    break;
                case Mode.Loading:
                    break;
                case Mode.GamePlay:
                    // Steps, which will hopefully move to an HID event class later
                    var currentBeat = hitBeat = musicManager.GetCurrentBeat();
                    var stepList = new List<Note>(currentSongNotes.Steps.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit));
                    stepList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

                    var holdList = new List<Hold>(
                        currentSongNotes.Holds.Where(x =>
                        Math.Abs(x.StartNote.BeatLocation - currentBeat) < 2 || (x.StartNote.BeatLocation < currentBeat && x.Notes.Last().BeatLocation > currentBeat)));

                    var motionList = new List<Note>(currentSongNotes.Motions.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit));
                    motionList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

                    // Temporary keyboard inputs
                    for (int i = 0; i < 8; i++)
                    {
                        if (kbState.IsKeyDown(touchkeys[i]) && !prevState.IsKeyDown(touchkeys[i]))
                        {
                            if (stepList.Count > 0)
                                closestNoteBeat = stepList.First().BeatLocation;
                            int id = random.Next(0, int.MaxValue);
                            touchCollection.Points.Add(new TouchPoint(currentBeat) { RawX = (int)(1024 / 8 * (i + 0.5)), RawY = 500, RawWidth = 128, RawHeight = 20, ID = id });
                            KeyDictionary.Add(touchkeys[i], id);

                            motionCollection.JumpBeat = double.NaN;
                        }
                        else if (!kbState.IsKeyDown(touchkeys[i]) && prevState.IsKeyDown(touchkeys[i]))
                        {
                            touchCollection.RemoveID(KeyDictionary[touchkeys[i]]);
                            KeyDictionary.Remove(touchkeys[i]);

                            if (touchCollection.Points.Count == 0)
                                motionCollection.JumpBeat = currentBeat;
                        }
                    }
                    if (kbState.IsKeyDown(Keys.Space) && !prevState.IsKeyDown(Keys.Space))
                        motionCollection.DownBeat = currentBeat;


                    // Check if we've hit any steps
                    foreach (var step in stepList)
                    {
                        // First check to see if they've passed the miss mark
                        var stepTimeMS = ((step.BeatLocation - currentBeat) * 60 / Globals.CurrentBpm);
                        if (stepTimeMS < -NoteTiming.Bad)
                        {
                            step.HitResult.WasHit = true;   // Let everyone else know this note has been resolved
                            step.HitResult.Difference = Timing.MissFlag;
                        }
                        else if (touchCollection.Points.Count > 0)
                        {
                            if (touchCollection.CheckHit(step))
                                gradeCollection.Set(gameTime, step);
                        }
                    }

                    // Check if we've hit or are still hitting any holds
                    foreach (var hold in holdList)
                    {
                        // Check start note if necessary
                        if(!hold.StartNote.HitResult.WasHit)
                        {
                            var stepTimeMS = ((hold.StartNote.BeatLocation - currentBeat) * 60 / Globals.CurrentBpm);
                            if(stepTimeMS < -NoteTiming.Bad)
                            {
                                hold.StartNote.HitResult.WasHit = true; // Let everyone else know this note has been resolved
                                hold.StartNote.HitResult.Difference = Timing.MissFlag;
                            }
                            else if (touchCollection.Points.Count > 0)
                            {
                                if (touchCollection.CheckHit(hold.StartNote))
                                    gradeCollection.Set(gameTime, hold.StartNote);
                            }
                        }

                        // Check any shuffles separately
                        var shuffles = hold.Notes.Where(x => x.Type == NoteType.Shuffle);
                        foreach (var shuffle in shuffles)
                        {
                            // Check window around shuffle and see if the foot is moving in the correct direction
                        }

                        // Let the note figure out itself whether it's being held and scoring
                        hold.CheckHold(touchCollection, currentBeat);
                    }

                    // Check if we've hit any motions
                    foreach (var motion in motionList)
                    {
                        var motionTimeMS = ((motion.BeatLocation - currentBeat) * 60 / Globals.CurrentBpm);
                        if(motionTimeMS < -MotionTiming.Miss)
                        {
                            motion.HitResult.WasHit = true;
                            motion.HitResult.Difference = Timing.MissFlag;
                        }
                        else if (!double.IsNaN(motionCollection.JumpBeat)) // Also down
                        {
                            if (motionCollection.CheckHit(motion))
                                gradeCollection.Set(gameTime, motion);
                        }
                    }

                    // Test grade
                    if (kbState.IsKeyDown(Keys.R) && !prevState.IsKeyDown(Keys.R))
                        gradeCollection.Set(gameTime, currentSongNotes.Steps.First());

                    // Mostly Debug things
                    if (kbState.IsKeyDown(Keys.Enter) && kbState.IsKeyUp(Keys.LeftAlt) && kbState.IsKeyUp(Keys.RightAlt) && !prevState.IsKeyDown(Keys.Enter))
                    {
                        if (!musicManager.IsPlaying)
                            musicManager.Play();
                        else
                            musicManager.Pause();
                    }

                    if (kbState.IsKeyDown(Keys.Down) && !prevState.IsKeyDown(Keys.Down))
                        Globals.BeatToWorldYUnits /= 2;

                    if (kbState.IsKeyDown(Keys.Up) && !prevState.IsKeyDown(Keys.Up))
                        Globals.BeatToWorldYUnits *= 2;

                    if (kbState.IsKeyDown(Keys.Delete) && !prevState.IsKeyDown(Keys.Delete))
                        drawRateMin = float.PositiveInfinity;

                    if (kbState.IsKeyDown(Keys.P) && !prevState.IsKeyDown(Keys.P))
                        ExportLog();

                    // Modify Camera
                    bool camChanged = false;
                    if (kbState.IsKeyDown(Keys.NumPad9) && !prevState.IsKeyDown(Keys.NumPad9))
                    {
                        cameraTarget.Y += 1;
                        camChanged = true;
                    }
                    else if (kbState.IsKeyDown(Keys.NumPad6) && !prevState.IsKeyDown(Keys.NumPad6))
                    {
                        cameraTarget.Y -= 1;
                        camChanged = true;
                    }
                    if (kbState.IsKeyDown(Keys.NumPad3) && !prevState.IsKeyDown(Keys.NumPad3))
                    {
                        cameraTarget.Z += 1;
                        camChanged = true;
                    }
                    else if (kbState.IsKeyDown(Keys.NumPad2) && !prevState.IsKeyDown(Keys.NumPad2))
                    {
                        cameraTarget.Z -= 1;
                        camChanged = true;
                    }
                    if (kbState.IsKeyDown(Keys.NumPad7) && !prevState.IsKeyDown(Keys.NumPad7))
                    {
                        cameraPos.Z += 1;
                        camChanged = true;
                    }
                    else if (kbState.IsKeyDown(Keys.NumPad4) && !prevState.IsKeyDown(Keys.NumPad4))
                    {
                        cameraPos.Z -= 1;
                        camChanged = true;
                    }
                    if (kbState.IsKeyDown(Keys.NumPad8) && !prevState.IsKeyDown(Keys.NumPad8))
                    {
                        cameraPos.Y += 1;
                        camChanged = true;
                    }
                    else if (kbState.IsKeyDown(Keys.NumPad5) && !prevState.IsKeyDown(Keys.NumPad5))
                    {
                        cameraPos.Y -= 1;
                        camChanged = true;
                    }

                    if (camChanged)
                        view = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.UnitY);
                    break;
                case Mode.Results:
                    break;
                default:
                    break;
            }
            
            // TODO: Add your update logic here
            updateRate = 1.0f / (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);


            base.Update(gameTime);

            prevState = kbState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if(enableProfiling)
                stopwatch.Restart();
            var log = new DrawCycleEventLog();

            drawRate = 1.0f / (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
            drawRateMin = Math.Min(drawRate, drawRateMin);

            switch (currentMode)
            {
                case Mode.MainMenu:
                    break;
                case Mode.Options:
                    break;
                case Mode.SongSelect:
                    GraphicsDevice.Clear(Color.PaleVioletRed);
                    spriteBatch.Begin();
                    for (int i = 0; i < songlist.Count; i++)
                    {
                        spriteBatch.Draw(songlist[i].Thumbnail, new Rectangle(50, graphics.PreferredBackBufferHeight / 2 - 60 + 120 * (i - currentSongIndex), 200, 120), Color.White);
                    }
                    spriteBatch.End();
                    break;
                case Mode.Loading:
                    break;
                case Mode.GamePlay:
                    GraphicsDevice.Clear(Color.Black);

                    // Draw 2D BG
                    //spriteBatch.Begin();
                    //spriteBatch.Draw(background, bgRect, Color.White);
                    //spriteBatch.End();


                    noteLanes.Draw(view, projection);
                    gradeZone.Draw(view, projection);

                    if (enableProfiling)
                        log.AddEvent(stopwatch.ElapsedMilliseconds, "GradeZone Drawn");

                    if (currentSongNotes != null)
                    {
                        var currentBeat = musicManager.GetCurrentBeat();
                        var motions = currentSongNotes.Motions.Where(p => p.BeatLocation > currentBeat - 6 && p.BeatLocation < currentBeat + 16);
                        var holds = currentSongNotes.Holds.Where(p => p.StartNote.BeatLocation > currentBeat - 6 && p.StartNote.BeatLocation < currentBeat + 16);
                        var notes = currentSongNotes.Steps.Where(p => p.BeatLocation > currentBeat - 6 && p.BeatLocation < currentBeat + 16);

                        if (enableProfiling)
                            log.AddEvent(stopwatch.ElapsedMilliseconds, "Lists generated");

                        // Feet width lines are drawn first
                        // Then the feet icons
                        foreach (var touchPt in touchCollection.Points)
                            touchPt.Draw(view, projection);

                        foreach (var motion in motions)
                            motion.Draw(currentBeat, view, projection);

                        if (enableProfiling)
                            log.AddEvent(stopwatch.ElapsedMilliseconds, "Motions Drawn");

                        var holdStart = holds.Count() - 1;
                        for (int i = 0; i < holds.Count(); i++)
                            holds.ElementAt(i).Draw(currentBeat, view, projection, holdStart - i);

                        if (enableProfiling)
                            log.AddEvent(stopwatch.ElapsedMilliseconds, "Holds Drawn");

                        foreach (var note in notes)
                            note.Draw(currentBeat, view, projection);

                        if (enableProfiling)
                            log.AddEvent(stopwatch.ElapsedMilliseconds, "Steps Drawn");
                    }

                    // Draw FPS counters
                    if (drawFpsCounter)
                    {
                        spriteBatch.Begin();
                        spriteBatch.DrawString(debugFont, "Update Rate: " + updateRate.ToString("F2"), new Vector2(10, 10), Color.Black);
                        spriteBatch.DrawString(debugFont, "Draw Rate: " + drawRate.ToString("F2"), new Vector2(10, 30), Color.Black);
                        spriteBatch.DrawString(debugFont, "Draw Rate: " + drawRateMin.ToString("F2"), new Vector2(10, 50), Color.Black);
                        spriteBatch.End();
                    }

                    // Draw Hit debugging
                    if(true)
                    {
                        spriteBatch.Begin();
                        spriteBatch.DrawString(debugFont, "HitBeat: " + hitBeat.ToString("F4"), new Vector2(1000, 10), Color.Black);
                        spriteBatch.DrawString(debugFont, "ClosestNote: " + closestNoteBeat.ToString("F4"), new Vector2(1000, 30), Color.Black);
                        spriteBatch.End();
                    }

                    // Draw Grades
                    spriteBatch.Begin();
                    gradeCollection.Draw(spriteBatch, gameTime);
                    spriteBatch.End();

                    if (enableProfiling)
                        log.AddEvent(stopwatch.ElapsedMilliseconds, "FPS counters drawn");
                    break;
                case Mode.Results:
                    break;
                default:
                    break;
            }

            base.Draw(gameTime);

            if (enableProfiling)
            {
                log.AddEvent(stopwatch.ElapsedMilliseconds, "Base Method");
                DrawCycleLog.Add(log);
            }
        }

        private void ExportLog()
        {
            if (DrawCycleLog.Count == 0)
                return;

            using (StreamWriter fs = new StreamWriter(new FileStream("./log.csv", FileMode.Append)))
            {
                if (fs.BaseStream.Position == 0)
                {
                    foreach (var header in DrawCycleLog[0].Events)
                    {
                        fs.Write(header.Item2 + ",");
                    }
                    fs.Write("\n");
                }
                foreach (var item in DrawCycleLog)
                {
                    foreach (var time in item.Events)
                    {
                        fs.Write(time.Item1 + ",");
                    }
                    fs.Write("\n");
                }
                DrawCycleLog.Clear();
            }
        }

        private void LoadSong(SongMetadata meta)
        {
            currentSongNotes = new NoteCollection();
            currentSongMeta = currentSongNotes.ParseFile(meta.ChartFullPath);

            musicManager.LoadSong(currentSongMeta.FilePath + currentSongMeta.SongFilename, currentSongMeta.BPM);
            musicManager.Offset = currentSongMeta.PlaybackOffset * 1000;

            // Preload all textures
            currentSongNotes.PreloadTextures();
        }
    }

    public class DrawCycleEventLog
    {
        public List<Tuple<long, string>> Events;
        
        public DrawCycleEventLog()
        {
            Events = new List<Tuple<long, string>>();
        }

        public void AddEvent(long time, string name)
        {
            Events.Add(new Tuple<long, string>(time, name));
        }
    }
}
