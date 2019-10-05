using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Nett;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StyleStar
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class StyleStar : Game
    {
        int Width { get { return graphics.PreferredBackBufferWidth; } }
        int Height { get { return graphics.PreferredBackBufferHeight; } }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Vector3 cameraPos = new Vector3(0, -44, 23);
        Vector3 cameraTarget = new Vector3(0, 36, 2);

        Matrix view;
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800 / 480f, 0.1f, 300f);

        QuadTexture gradeZone;
        QuadTexture noteLanes, noteLaneAccent1l, noteLaneAccent1r, noteLaneAccent2l, noteLaneAccent2r;
        Texture2D background;
        Rectangle bgRect;

        SpriteFont debugFont;
        float updateRate;
        float drawRate;
        float drawRateMin = float.PositiveInfinity;

        private Mode currentMode = Mode.SongSelect;

        UserSettings currentUserSettings = new UserSettings();

        // Music
        MusicManager musicManager = new MusicManager();

        NoteCollection currentSongNotes;
        SongMetadata currentSongMeta;

        KeyboardState prevKbState;
        MouseState prevMouseState;
        bool disableMouseClick = true;  // TODO make this a setting

        bool TouchScreenConnected = false;

        TouchCollection touchCollection = new TouchCollection();
        MotionCollection motionCollection = new MotionCollection();
        GradeCollection gradeCollection = new GradeCollection();

        bool enterLoadingScreen;
        bool leavingLoadingScreen;
        double loadingScreenTime;
        int loadingScreenTransition = 400;
        int loadingScreenWait = 1000;
        Texture2D loadingScreenLeft;
        Texture2D loadingScreenRight;

        // Font Info
        private const int fontBitmapWidth = 8192;
        private const int fontBitmapHeight = 8192;

        // Hit Debug
        double hitBeat = 0;
        double closestNoteBeat = 0;
        Keys[] touchkeys = new Keys[] { Keys.A, Keys.S, Keys.D, Keys.F, Keys.J, Keys.K, Keys.L, Keys.OemSemicolon };
        bool[] prevKeys = new bool[8];
        Dictionary<Keys, uint> KeyDictionary = new Dictionary<Keys, uint>();
        Random random = new Random();
        bool horzMotion = false;

        // DEBUG
        bool drawFpsCounter = true;

        bool updateProfiling = true;
        Stopwatch updateStopwatch = new Stopwatch();
        List<DrawCycleEventLog> UpdateCycleLog = new List<DrawCycleEventLog>();

        public StyleStar()
        {
            graphics = new GraphicsDeviceManager(this) { GraphicsProfile = GraphicsProfile.HiDef };
            Content.RootDirectory = "Content";
            Globals.ContentManager = Content;
            Globals.GraphicsManager = graphics;

            //this.graphics.SynchronizeWithVerticalRetrace = false;
            //base.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 60.0f);

            // Load config file
            if (File.Exists(Defines.ConfigFile))
            {
                var configTable = Toml.ReadFile(Defines.ConfigFile).ToDictionary();

                if (configTable.ContainsKey(Defines.KeyConfig))
                    InputMonitor.SetKeys((Dictionary<string, object>)configTable[Defines.KeyConfig]);
                if(configTable.ContainsKey(Defines.TouchConfig))
                    TouchSettings.SetConfig((Dictionary<string, object>)configTable[Defines.TouchConfig]);
            }

            // Save config file (regardless of if there was one already)
            var data = new Dictionary<string, object>()
            {
                {Defines.KeyConfig, InputMonitor.GetConfig() },
                {Defines.TouchConfig, TouchSettings.GetConfig() }
            };
            Toml.WriteFile(data, Defines.ConfigFile);
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

            Globals.WindowSize = new Vector2(1280, 720);

            view = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.UnitY);

            bgRect = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            if (TouchPanel.GetCapabilities().IsConnected)
            {
                TouchScreenConnected = true;
                disableMouseClick = true;
                TouchPanel.EnableMouseTouchPoint = true;
                TouchPanel.EnabledGestures = GestureType.Tap;
                TouchPanel.EnableMouseGestures = true;
            }

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
            noteLaneAccent1l = new QuadTexture(Content.Load<Texture2D>("NoteLaneAccent1"));
            noteLaneAccent1l.SetVerts(Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth, Globals.GradeZoneWidth / 2, -20, Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth, Globals.GradeZoneWidth / 2, 300, -0.15f);
            noteLaneAccent1r = new QuadTexture(Content.Load<Texture2D>("NoteLaneAccent1"));
            noteLaneAccent1r.SetVerts(-Globals.GradeZoneWidth / 2, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth, -20, -Globals.GradeZoneWidth / 2, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth, 300, -0.15f);
            noteLaneAccent2l = new QuadTexture(Content.Load<Texture2D>("NoteLaneAccent2"));
            noteLaneAccent2l.SetVerts(Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth * 2, Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth, -20, Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth * 2, Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth, 300, -0.15f);
            noteLaneAccent2r = new QuadTexture(Content.Load<Texture2D>("NoteLaneAccent2"));
            noteLaneAccent2r.SetVerts(-Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth * 2, -20, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth * 2, 300, -0.15f);
            background = Content.Load<Texture2D>("Background");
            loadingScreenLeft = Content.Load<Texture2D>("LoadingScreen-Left");
            loadingScreenRight = Content.Load<Texture2D>("LoadingScreen-Right");

            debugFont = Content.Load<SpriteFont>("DebugFont");
            Globals.Font = new Dictionary<string, SpriteFont>();

            // Load fonts dynamically
            Globals.Font.Add("Franklin", FontLoader.LoadFont("Content/Fonts/libre-franklin/librefranklin-blackitalic.ttf", 144));
            Globals.Font.Add("RunningStart", FontLoader.LoadFont("Content/Fonts/RunningStart.ttf", 144));
            //Globals.Font.Add("JP", FontLoader.LoadFont("Content/Fonts/mplus-1p-heavy.ttf", 144, FontLoader.FontRange.Japanese));
            //Globals.Font.Add("Franklin", FontLoader.LoadFont("Content/Fonts/librefranklin-mplus.ttf", 144, FontLoader.FontRange.Japanese));
            Globals.Font.Add("JP", FontLoader.LoadFont("Content/Fonts/librefranklin-mplus.ttf", 144, FontLoader.FontRange.Japanese));

            // Load songs
            SongSelection.ImportSongs("Songs");

            // Generate Static Text Labels
            SongSelection.GenerateLabels();
            ResultScreen.GenerateLabels();
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
            if (updateProfiling && currentMode == Mode.GamePlay)
                updateStopwatch.Restart();
            var log = new DrawCycleEventLog();

            var kbState = Keyboard.GetState();
            var mouseState = Mouse.GetState();
            var touchState = TouchPanel.GetState();

            if(touchState.Count > 0)
            {
                var newPts = new List<TouchLocation>();
                var currentBeat = musicManager.GetCurrentBeat();

                foreach (var pt in touchState)
                {
                    if (pt.Id == 1)
                        continue;   // Pt with ID 1 is essentially a "mouse click" and is not helpful

                    Console.WriteLine("ID: {0}\tX: {1}\tY: {2}", pt.Id, pt.Position.X, pt.Position.Y);

                    // Update touch collection
                    if (touchCollection.Points.ContainsKey((uint)pt.Id))
                    {
                        touchCollection.Points[(uint)pt.Id].Update(currentBeat, (int)pt.Position.X, (int)pt.Position.Y);
                    }
                    else
                    { 
                        touchCollection.Points[(uint)pt.Id] = new TouchPoint(currentBeat, (int)pt.Position.X, (int)pt.Position.Y)
                        {
                            ID = (uint)pt.Id,
                            RawWidth = 128,
                            RawHeight = 20
                        };
                    }
                }
                // Remove points that are no longer being held
                var ids = touchCollection.Points.Select(x => x.Key).ToList();
                foreach (var id in ids)
                {
                    if (touchState.Where(x => (uint)x.Id == id).Count() == 0)
                        touchCollection.RemoveID(id);
                }
            }
            else
            {
                touchCollection.Points.Clear();
            }

            InputMonitor.Update(gameTime);

            if(updateProfiling && currentMode == Mode.GamePlay)
            log.AddEvent(updateStopwatch.ElapsedMilliseconds, "StatesFetched");

            if (InputMonitor.Monitors[Inputs.Exit].State == KeyState.Press)
            {
                Exit();
            }

            if ((kbState.IsKeyDown(Keys.RightAlt) || kbState.IsKeyDown(Keys.LeftAlt)) && kbState.IsKeyDown(Keys.Enter) && !prevKbState.IsKeyDown(Keys.Enter))
            {
                graphics.ToggleFullScreen();
                return;
            }

            switch (currentMode)
            {
                case Mode.MainMenu:
                    break;
                case Mode.Options:
                    break;
                case Mode.SongSelect:
                    // If we're entering or leaving a loading screen, block other inputs
                    if (enterLoadingScreen || leavingLoadingScreen)
                    {
                        if (gameTime.TotalGameTime.TotalMilliseconds > (loadingScreenTime + loadingScreenTransition + loadingScreenWait))
                        {
                            if (enterLoadingScreen)
                            {
                                enterLoadingScreen = false;
                                leavingLoadingScreen = true;
                                var meta = SongSelection.GetCurrentSongMeta();
                                LoadSong(meta);
                                UIScreen.GenerateLabels(currentSongNotes);
                                // Set notelane textures
                                noteLaneAccent1l.SetColor(meta.ColorAccent.IfNull(ThemeColors.Purple));
                                noteLaneAccent1r.SetColor(meta.ColorAccent.IfNull(ThemeColors.Purple));
                                noteLaneAccent2l.SetColor(meta.ColorFore.IfNull(ThemeColors.Blue));
                                noteLaneAccent2r.SetColor(meta.ColorFore.IfNull(ThemeColors.Blue));
                                currentMode = Mode.GamePlay;
                                loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                                break;
                            }
                            else
                            {
                                leavingLoadingScreen = false;
                            }
                        }
                        else
                            break;
                    }

                    // TEMP FOR TESTING
                    //LoadSong(songlist[6]);
                    ////currentMode = Mode.GamePlay;
                    //enterLoadingScreen = true;
                    //loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                    // END TEMP

                    if (SongSelection.Songlist.Count > 0)
                    {
                        if (InputMonitor.Monitors[Inputs.Down].State == KeyState.Press || (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue))
                        {
                            SongSelection.ScrollDown();
                        }

                        if (InputMonitor.Monitors[Inputs.Up].State == KeyState.Press || (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue))
                        {
                            SongSelection.ScrollUp();
                        }

                        //if (kbState.IsKeyDown(Keys.Right) && !prevKbState.IsKeyDown(Keys.Right))
                        if (InputMonitor.Monitors[Inputs.Right].State == KeyState.Press)
                        {
                            SongSelection.CycleDifficulty();
                        }

                        if (InputMonitor.Monitors[Inputs.Select].State == KeyState.Press
                            || (!disableMouseClick && (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed)))
                        {
                            if (SongSelection.Select())
                            {
                                enterLoadingScreen = true;
                                loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                            }
                        }

                        if (InputMonitor.Monitors[Inputs.Back].State == KeyState.Press ||
                            InputMonitor.Monitors[Inputs.Back2].State == KeyState.Press ||
                            InputMonitor.Monitors[Inputs.Left].State == KeyState.Press ||
                            (!disableMouseClick && (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton != ButtonState.Pressed)))
                        {
                            SongSelection.GoBack();
                        }

                        if (InputMonitor.Monitors[Inputs.Auto].State == KeyState.Press)
                            Globals.IsAutoModeEnabled = !Globals.IsAutoModeEnabled;
                    }

                    break;
                case Mode.Loading:
                    break;
                case Mode.GamePlay:
                    // If we're entering or leaving a loading screen, block other inputs
                    if (enterLoadingScreen || leavingLoadingScreen)
                    {
                        if (gameTime.TotalGameTime.TotalMilliseconds > (loadingScreenTime + loadingScreenTransition + loadingScreenWait))
                        {
                            if (enterLoadingScreen)
                            {
                                musicManager.Pause();
                                enterLoadingScreen = false;
                                leavingLoadingScreen = true;
                                currentMode = Mode.Results;
                                ResultScreen.UpdateText(currentSongNotes);
                                loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                                break;
                            }
                            else
                            {
                                leavingLoadingScreen = false;
                                musicManager.Play();

                                // Reset inputs
                                motionCollection.DownBeat = double.NaN;
                                motionCollection.JumpBeat = double.NaN;
                            }
                        }
                        else
                            break;
                    }

                    // If user forfeits or song ends, move onto the result screen
                    if (InputMonitor.Monitors[Inputs.Back].State == KeyState.Press)
                    {
                        currentSongNotes.SongEnd = SongEndReason.Forfeit;
                        enterLoadingScreen = true;
                        loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    }
                    if (musicManager.IsFinished)
                    {
                        currentSongNotes.SongEnd = SongEndReason.Cleared;
                        enterLoadingScreen = true;
                        loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    }

                    if (updateProfiling && currentMode == Mode.GamePlay)
                        log.AddEvent(updateStopwatch.ElapsedMilliseconds, "Start getting steps");

                    // Steps, which will hopefully move to an HID event class later
                    var currentBeat = hitBeat = musicManager.GetCurrentBeat();
                    var currentTime = Globals.GetSecAtBeat(currentBeat);
                    var stepList = currentSongNotes.Steps.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit).ToList();
                    stepList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

                    var holdList = currentSongNotes.Holds.Where(x =>
                        Math.Abs(x.StartNote.BeatLocation - currentBeat) < 64 || (x.StartNote.BeatLocation < currentBeat && x.Notes.Last().BeatLocation > currentBeat)).ToList();

                    var motionList = currentSongNotes.Motions.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit).ToList();
                    motionList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

                    if (updateProfiling && currentMode == Mode.GamePlay)
                        log.AddEvent(updateStopwatch.ElapsedMilliseconds, "End getting steps");

                    // Temporary keyboard inputs
                    if (!TouchScreenConnected)
                    {
                        bool horzMotion = false;
                        for (int i = 0; i < 8; i++)
                        {
                            if (kbState.IsKeyDown(touchkeys[i]) && !prevKbState.IsKeyDown(touchkeys[i]))
                            {
                                bool checkLeft = true;
                                bool checkRight = true;
                                if (i == 0)
                                    checkLeft = false;
                                if (i == 7)
                                    checkRight = false;

                                if (checkLeft && prevKeys[i - 1])
                                    horzMotion |= true;
                                else if (checkRight && prevKeys[i + 1])
                                    horzMotion |= true;
                                else
                                    horzMotion |= false;

                                if (stepList.Count > 0)
                                    closestNoteBeat = stepList.First().BeatLocation;
                                uint id = (uint)random.Next(0, int.MaxValue);
                                if (touchCollection.Points.TryAdd(id, new TouchPoint(currentBeat, (int)(1024 / 8 * (i + 0.5)), 500) { RawWidth = 128, RawHeight = 20, ID = id })) ;
                                KeyDictionary.Add(touchkeys[i], id);

                                motionCollection.JumpBeat = double.NaN;
                                prevKeys[i] = true;
                            }
                            else if (!kbState.IsKeyDown(touchkeys[i]) && prevKbState.IsKeyDown(touchkeys[i]))
                            {
                                TouchPoint pt;
                                if (touchCollection.Points.TryRemove(KeyDictionary[touchkeys[i]], out pt))
                                    KeyDictionary.Remove(touchkeys[i]);

                                if (touchCollection.Points.Count == 0)
                                    motionCollection.JumpBeat = currentBeat;

                                prevKeys[i] = false;
                            }
                        }
                    }
                    if (kbState.IsKeyDown(Keys.Space) && !prevKbState.IsKeyDown(Keys.Space))
                        motionCollection.DownBeat = currentBeat;
                    else
                        motionCollection.DownBeat = double.NaN;

                    if (updateProfiling && currentMode == Mode.GamePlay)
                        log.AddEvent(updateStopwatch.ElapsedMilliseconds, "STEPS: Start");

                    // Check if we've hit any steps
                    foreach (var step in stepList)
                    {
                        // First check to see if they've passed the miss mark
                        //var stepTimeMS = ((step.BeatLocation - currentBeat) * 60 / Globals.CurrentBpm);
                        var stepTimeMS = Globals.GetSecAtBeat(step.BeatLocation) - currentTime;
                        if (stepTimeMS < -NoteTiming.Bad)
                        {
                            step.HitResult.WasHit = true;   // Let everyone else know this note has been resolved
                            step.HitResult.Difference = Timing.MissFlag;
                            currentSongNotes.AddToScore(NoteType.Step, step.HitResult.Difference);
                        }
                        else if (Globals.IsAutoModeEnabled)
                        {
                            if (Math.Abs(stepTimeMS) < NoteTiming.AutoTolerance)
                            {
                                step.HitResult.WasHit = true;
                                step.HitResult.Difference = 0;
                                gradeCollection.Set(gameTime, step);
                                currentSongNotes.AddToScore(NoteType.Step, step.HitResult.Difference);
                            }
                        }
                        else if (touchCollection.Points.Count > 0)
                        {
                            
                            if (touchCollection.CheckHit(step))
                            {
                                gradeCollection.Set(gameTime, step);
                                currentSongNotes.AddToScore(NoteType.Step, step.HitResult.Difference);
                            }
                        }
                    }
                    if (updateProfiling && currentMode == Mode.GamePlay)
                    {
                        log.AddEvent(updateStopwatch.ElapsedMilliseconds, "STEPS: End");
                        log.AddEvent(updateStopwatch.ElapsedMilliseconds, "HOLDS: Start");
                    }

                    // Check if we've hit or are still hitting any holds
                    foreach (var hold in holdList)
                    {
                        // Check start note if necessary
                        if (!hold.StartNote.HitResult.WasHit)
                        {
                            //var stepTimeMS = ((hold.StartNote.BeatLocation - currentBeat) * 60 / Globals.CurrentBpm);
                            var stepTimeMS = Globals.GetSecAtBeat(hold.StartNote.BeatLocation) - currentTime;
                            if (stepTimeMS < -NoteTiming.Bad)
                            {
                                hold.StartNote.HitResult.WasHit = true; // Let everyone else know this note has been resolved
                                hold.StartNote.HitResult.Difference = Timing.MissFlag;
                                currentSongNotes.AddToScore(NoteType.Hold, hold.StartNote.HitResult.Difference);
                            }
                            else if (Globals.IsAutoModeEnabled)
                            {
                                if (Math.Abs(stepTimeMS) < NoteTiming.AutoTolerance)
                                {
                                    hold.StartNote.HitResult.WasHit = true;
                                    hold.StartNote.HitResult.Difference = 0;
                                    gradeCollection.Set(gameTime, hold.StartNote);
                                    currentSongNotes.AddToScore(NoteType.Hold, hold.StartNote.HitResult.Difference);
                                }
                            }
                            else if (touchCollection.Points.Count > 0)
                            {
                                if (touchCollection.CheckHit(hold.StartNote))
                                {
                                    gradeCollection.Set(gameTime, hold.StartNote);
                                    currentSongNotes.AddToScore(NoteType.Hold, hold.StartNote.HitResult.Difference);
                                }
                            }
                        }

                        if (updateProfiling && currentMode == Mode.GamePlay)
                        {
                            log.AddEvent(updateStopwatch.ElapsedMilliseconds, "HOLDS: End");
                            log.AddEvent(updateStopwatch.ElapsedMilliseconds, "SHUFFLES: Start");
                        }

                        // Check any shuffles separately
                        var shuffles = hold.Notes.Where(x => x.Type == NoteType.Shuffle && x.HitResult.WasHit == false);
                        foreach (var shuffle in shuffles)
                        {
                            // Check window around shuffle and see if the foot is moving in the correct direction
                            var stepTimeMS = Globals.GetSecAtBeat(shuffle.BeatLocation) - currentTime;
                            if (stepTimeMS < -NoteTiming.Bad)
                            {
                                shuffle.HitResult.WasHit = true; // Let everyone else know this note has been resolved
                                shuffle.HitResult.Difference = Timing.MissFlag;
                                currentSongNotes.AddToScore(NoteType.Hold, shuffle.HitResult.Difference);
                            }
                            else if (Globals.IsAutoModeEnabled)
                            {
                                if (Math.Abs(stepTimeMS) < NoteTiming.AutoTolerance)
                                {
                                    shuffle.HitResult.WasHit = true;
                                    shuffle.HitResult.Difference = 0;
                                    currentSongNotes.AddToScore(NoteType.Hold, shuffle.HitResult.Difference);
                                }
                            }
                            else if (touchCollection.Points.Count > 0)
                            {
                                if (touchCollection.CheckHit(shuffle))
                                {
                                    currentSongNotes.AddToScore(NoteType.Hold, shuffle.HitResult.Difference);
                                }
                            }
                        }

                        if (updateProfiling)
                            log.AddEvent(updateStopwatch.ElapsedMilliseconds, "SHUFFLES: End");

                        // Let the note figure out itself whether it's being held and scoring
                        var holdResult = hold.CheckHold(touchCollection, currentBeat);
                        if (holdResult == HitState.Hit)
                            currentSongNotes.AddToScore(NoteType.Hold, NoteTiming.Perfect);
                        else if (holdResult == HitState.Miss)
                            currentSongNotes.AddToScore(NoteType.Hold, Timing.MissFlag);
                    }

                    // Check if we've hit any motions
                    foreach (var motion in motionList)
                    {
                        //var motionTimeMS = ((motion.BeatLocation - currentBeat) * 60 / Globals.CurrentBpm);
                        var motionTimeMS = Globals.GetSecAtBeat(motion.BeatLocation) - currentTime;
                        if (motionTimeMS < -MotionTiming.Miss)
                        {
                            motion.HitResult.WasHit = true;
                            motion.HitResult.Difference = Timing.MissFlag;
                            currentSongNotes.AddToScore(NoteType.Motion, motion.HitResult.Difference);
                        }
                        else if (Globals.IsAutoModeEnabled)
                        {
                            if (Math.Abs(motionTimeMS) < NoteTiming.AutoTolerance)
                            {
                                motion.HitResult.WasHit = true;
                                motion.HitResult.Difference = 0;
                                gradeCollection.Set(gameTime, motion);
                                currentSongNotes.AddToScore(NoteType.Motion, motion.HitResult.Difference);
                            }
                        }
                        else if (motion.Motion == Motion.Down && !double.IsNaN(motionCollection.DownBeat))
                        {
                            if (motionCollection.CheckHit(motion))
                            {
                                gradeCollection.Set(gameTime, motion);
                                currentSongNotes.AddToScore(NoteType.Motion, motion.HitResult.Difference);
                            }
                        }
                        else if (motion.Motion == Motion.Up && motionTimeMS < MotionTiming.JumpPerfectCheck)
                        {
                            // If there's no feet on the pad within the perfect window, it counts
                            if (touchCollection.Points.Count == 0)
                            {
                                motion.HitResult.WasHit = true;
                                motion.HitResult.Difference = (float)motionTimeMS;
                                gradeCollection.Set(gameTime, motion);
                                currentSongNotes.AddToScore(NoteType.Motion, motion.HitResult.Difference);
                            }
                        }
                    }

                    // Test grade
                    if (kbState.IsKeyDown(Keys.R) && !prevKbState.IsKeyDown(Keys.R))
                        gradeCollection.Set(gameTime, currentSongNotes.Steps.First());

                    // Mostly Debug things
                    if (InputMonitor.Monitors[Inputs.Select].State == KeyState.Press)
                    {
                        if (!musicManager.IsPlaying)
                            musicManager.Play();
                        else
                            musicManager.Pause();
                    }

                    if (InputMonitor.Monitors[Inputs.Down].State == KeyState.Press)
                    {
                        Globals.SpeedScale -= 0.5;
                        UIScreen.UpdateSpeed();
                    }

                    if (InputMonitor.Monitors[Inputs.Up].State == KeyState.Press)
                    {
                        Globals.SpeedScale += 0.5;
                        UIScreen.UpdateSpeed();
                    }

                    if (kbState.IsKeyDown(Keys.Delete) && !prevKbState.IsKeyDown(Keys.Delete))
                        drawRateMin = float.PositiveInfinity;


                    //// Modify Camera
                    //bool camChanged = false;
                    //if (kbState.IsKeyDown(Keys.NumPad9) && !prevState.IsKeyDown(Keys.NumPad9))
                    //{
                    //    cameraTarget.Y += 1;
                    //    camChanged = true;
                    //}
                    //else if (kbState.IsKeyDown(Keys.NumPad6) && !prevState.IsKeyDown(Keys.NumPad6))
                    //{
                    //    cameraTarget.Y -= 1;
                    //    camChanged = true;
                    //}
                    //if (kbState.IsKeyDown(Keys.NumPad3) && !prevState.IsKeyDown(Keys.NumPad3))
                    //{
                    //    cameraTarget.Z += 1;
                    //    camChanged = true;
                    //}
                    //else if (kbState.IsKeyDown(Keys.NumPad2) && !prevState.IsKeyDown(Keys.NumPad2))
                    //{
                    //    cameraTarget.Z -= 1;
                    //    camChanged = true;
                    //}
                    //if (kbState.IsKeyDown(Keys.NumPad7) && !prevState.IsKeyDown(Keys.NumPad7))
                    //{
                    //    cameraPos.Z += 1;
                    //    camChanged = true;
                    //}
                    //else if (kbState.IsKeyDown(Keys.NumPad4) && !prevState.IsKeyDown(Keys.NumPad4))
                    //{
                    //    cameraPos.Z -= 1;
                    //    camChanged = true;
                    //}
                    //if (kbState.IsKeyDown(Keys.NumPad8) && !prevState.IsKeyDown(Keys.NumPad8))
                    //{
                    //    cameraPos.Y += 1;
                    //    camChanged = true;
                    //}
                    //else if (kbState.IsKeyDown(Keys.NumPad5) && !prevState.IsKeyDown(Keys.NumPad5))
                    //{
                    //    cameraPos.Y -= 1;
                    //    camChanged = true;
                    //}

                    //if (camChanged)
                    //    view = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.UnitY);
                    break;
                case Mode.Results:
                    // If we're entering or leaving a loading screen, block other inputs
                    if (enterLoadingScreen || leavingLoadingScreen)
                    {
                        if (gameTime.TotalGameTime.TotalMilliseconds > (loadingScreenTime + loadingScreenTransition + loadingScreenWait))
                        {
                            if (enterLoadingScreen)
                            {
                                enterLoadingScreen = false;
                                leavingLoadingScreen = true;
                                currentMode = Mode.SongSelect;
                                loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                                break;
                            }
                            else
                            {
                                leavingLoadingScreen = false;
                            }
                        }
                        else
                            break;
                    }

                    if (InputMonitor.Monitors[Inputs.Select].State == KeyState.Press)
                    {
                        enterLoadingScreen = true;
                        loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    break;
                default:
                    break;
            }

            // TODO: Add your update logic here
            updateRate = 1.0f / (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);

            if (updateProfiling && currentMode == Mode.GamePlay)
                UpdateCycleLog.Add(log);

            if (kbState.IsKeyDown(Keys.P) && !prevKbState.IsKeyDown(Keys.P))
                ExportLog();

            base.Update(gameTime);

            prevKbState = kbState;
            prevMouseState = mouseState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (Globals.DrawProfiling)
                Globals.DrawStopwatch.Restart();
            var log = Globals.DrawTempLog = new DrawCycleEventLog();

            drawRate = 1.0f / (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
            drawRateMin = Math.Min(drawRate, drawRateMin);

            string titleFont = "";
            string artistFont = "";

            switch (currentMode)
            {
                case Mode.MainMenu:
                    break;
                case Mode.Options:
                    break;
                case Mode.SongSelect:
                    GraphicsDevice.Clear(Color.Black);

                    // Draw order:
                    // 1. BG
                    // 2. Menu Bar
                    // 3. Active Selection
                    // 4. Songs/Folders
                    // 5. UI elements

                    SongSelection.Draw(spriteBatch);

                    if (enterLoadingScreen || leavingLoadingScreen)
                        DrawLoadingTransition(gameTime);
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
                    noteLaneAccent1l.Draw(view, projection);
                    noteLaneAccent1r.Draw(view, projection);
                    noteLaneAccent2l.Draw(view, projection);
                    noteLaneAccent2r.Draw(view, projection);
                    gradeZone.Draw(view, projection);

                    if (Globals.DrawProfiling)
                        log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "GradeZone Drawn");

                    if (currentSongNotes != null)
                    {
                        var currentBeat = musicManager.GetCurrentBeat();
                        var motions = currentSongNotes.Motions.Where(p => p.BeatLocation > currentBeat - 6 && p.BeatLocation < currentBeat + 16);
                        var holds = currentSongNotes.Holds.Where(p => p.StartNote.BeatLocation > currentBeat - 16 && p.StartNote.BeatLocation < currentBeat + 16);
                        var notes = currentSongNotes.Steps.Where(p => p.BeatLocation > currentBeat - 6 && p.BeatLocation < currentBeat + 16);
                        var marks = currentSongNotes.Markers.Where(p => p.BeatLocation > currentBeat - 6 && p.BeatLocation < currentBeat + 16);

                        if (Globals.DrawProfiling)
                            log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "Lists generated");

                        // Draw beat markers
                        foreach (var mark in marks)
                            mark.Draw(currentBeat, view, projection);

                        // Feet width lines are drawn first
                        // Then the feet icons
                        foreach (var touchPt in touchCollection.Points)
                            touchPt.Value.Draw(view, projection);

                        foreach (var motion in motions)
                            motion.Draw(currentBeat, view, projection);

                        if (Globals.DrawProfiling)
                            log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "Motions Drawn");

                        var holdStart = holds.Count() - 1;
                        for (int i = 0; i < holds.Count(); i++)
                            holds.ElementAt(i).Draw(currentBeat, view, projection, holdStart - i);
                        //holds.ElementAt(i).Draw(currentBeat, view, projection);

                        if (Globals.DrawProfiling)
                            log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "Holds Drawn");

                        foreach (var note in notes)
                            note.Draw(currentBeat, view, projection);

                        if (Globals.DrawProfiling)
                            log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "Steps Drawn");
                    }

                    // Draw Grades
                    spriteBatch.Begin();
                    gradeCollection.Draw(spriteBatch, gameTime);
                    spriteBatch.End();

                    if (Globals.DrawProfiling)
                        log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "Grades Drawn");

                    // Draw UI elements
                    UIScreen.Draw(spriteBatch, currentSongNotes);

                    if (Globals.DrawProfiling)
                        log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "UI Drawn");

                    // Draw FPS counters
                    if (drawFpsCounter)
                    {
                        spriteBatch.Begin();
                        spriteBatch.DrawString(debugFont, "Update Rate: " + updateRate.ToString("F2"), new Vector2(10, 10), Color.White);
                        spriteBatch.DrawString(debugFont, "Draw Rate: " + drawRate.ToString("F2"), new Vector2(10, 30), Color.White);
                        spriteBatch.DrawString(debugFont, "Draw Rate: " + drawRateMin.ToString("F2"), new Vector2(10, 50), Color.White);
                        spriteBatch.End();
                    }

                    // Draw Hit debugging
                    if (false)
                    {
                        spriteBatch.Begin();
                        spriteBatch.DrawString(debugFont, "HitBeat: " + hitBeat.ToString("F4"), new Vector2(1000, 10), Color.Black);
                        spriteBatch.DrawString(debugFont, "ClosestNote: " + closestNoteBeat.ToString("F4"), new Vector2(1000, 30), Color.Black);
                        spriteBatch.End();
                    }

                    // Draw beat#
                    if (true)
                    {
                        spriteBatch.Begin();
                        spriteBatch.DrawString(debugFont, "Current Beat: " + musicManager.GetCurrentBeat().ToString("F4"), new Vector2(1000, 10), Color.White);
                        spriteBatch.End();
                    }

                    // Draw hit stats
                    if (true)
                    {
                        var numStepHit = currentSongNotes.Steps.Where(x => x.HitResult.WasHit && x.HitResult.Difference != Timing.MissFlag).Count();
                        var numMotionHit = currentSongNotes.Motions.Where(x => x.HitResult.WasHit && x.HitResult.Difference != Timing.MissFlag).Count();

                        spriteBatch.Begin();
                        spriteBatch.DrawString(debugFont, "Steps: " + numStepHit + " / " + currentSongNotes.Steps.Count, new Vector2(1100, 300), Color.White);
                        spriteBatch.DrawString(debugFont, "Motions: " + numMotionHit + " / " + currentSongNotes.Motions.Count, new Vector2(1100, 320), Color.White);
                        spriteBatch.DrawString(debugFont, "Combo: " + currentSongNotes.CurrentCombo, new Vector2(1100, 340), Color.White);
                        spriteBatch.End();
                    }

                    if (Globals.DrawProfiling)
                        log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "On-screen Debug Drawn");

                    if (enterLoadingScreen || leavingLoadingScreen)
                        DrawLoadingTransition(gameTime);
                    break;
                case Mode.Results:
                    GraphicsDevice.Clear(Color.LightSkyBlue);

                    ResultScreen.Draw(spriteBatch, currentSongNotes);

                    if (enterLoadingScreen || leavingLoadingScreen)
                        DrawLoadingTransition(gameTime);
                    break;
                default:
                    break;
            }

            base.Draw(gameTime);

            if (Globals.DrawProfiling)
            {
                log.AddEvent(Globals.DrawStopwatch.ElapsedMilliseconds, "Base Method");

                Globals.DrawCycleLog.Add(log);
            }
        }

        static private Rectangle loadingRect = new Rectangle();
        private void DrawLoadingTransition(GameTime gt)
        {
            var duration = gt.TotalGameTime.TotalMilliseconds - loadingScreenTime;
            var ratio = enterLoadingScreen ? 1 - duration / loadingScreenTransition : duration / loadingScreenTransition;
            var loadScreenWidth = 1320 / Globals.CurrentScalingFactor;
            var loadScreenHeight = 1080 / Globals.CurrentScalingFactor;

            var xLeft = Math.Min(0, 0 - loadScreenWidth * ratio);
            var xRight = Math.Max(Width - loadScreenWidth, Width - (loadScreenWidth) * (1 - ratio));

            spriteBatch.Begin();
            loadingRect.X = (int)xLeft;
            loadingRect.Y = 0;
            loadingRect.Width = (int)loadScreenWidth;
            loadingRect.Height = (int)loadScreenHeight;
            spriteBatch.Draw(loadingScreenLeft, loadingRect, Color.White);
            loadingRect.X = (int)xRight;
            spriteBatch.Draw(loadingScreenRight, loadingRect, Color.White);
            spriteBatch.End();
        }

        List<string> drawHeaders = new List<string>();

        private void ExportLog()
        {
            if (Globals.DrawCycleLog.Count > 0)
            {
                using (StreamWriter fs = new StreamWriter(new FileStream("./log.csv", FileMode.Append)))
                {
                    if (fs.BaseStream.Position == 0)
                    {
                        foreach (var entry in Globals.DrawCycleLog)
                        {
                            foreach (var evt in entry.Events)
                            {
                                if (!drawHeaders.Contains(evt.Item2))
                                {
                                    drawHeaders.Add(evt.Item2);
                                    fs.Write(evt.Item2 + ",");
                                }
                            }
                        }
                        fs.Write("\n");
                    }
                    foreach (var item in Globals.DrawCycleLog)
                    {
                        foreach (var header in drawHeaders)
                        {
                            var match = item.Events.FirstOrDefault(x => x.Item2 == header);
                            if (match != null)
                                fs.Write(match.Item1 + ",");
                            else
                                fs.Write(",");
                        }
                        fs.Write("\n");
                    }
                    Globals.DrawCycleLog.Clear();
                }
            }
            if (UpdateCycleLog.Count > 0)
            {
                using (StreamWriter fs = new StreamWriter(new FileStream("./log-update.csv", FileMode.Append)))
                {
                    if (fs.BaseStream.Position == 0)
                    {
                        foreach (var header in UpdateCycleLog[0].Events)
                        {
                            fs.Write(header.Item2 + ",");
                        }
                        fs.Write("\n");
                    }
                    foreach (var item in UpdateCycleLog)
                    {
                        foreach (var time in item.Events)
                        {
                            fs.Write(time.Item1 + ",");
                        }
                        fs.Write("\n");
                    }
                    UpdateCycleLog.Clear();
                }
            }

            if (Globals.TouchCycleLog.Count > 0)
            {
                using (StreamWriter fs = new StreamWriter(new FileStream("./log-touch.csv", FileMode.Append)))
                {
                    if (fs.BaseStream.Position == 0)
                    {
                        foreach (var header in Globals.TouchCycleLog[0].Events)
                        {
                            fs.Write(header.Item2 + ",");
                        }
                        fs.Write("\n");
                    }
                    foreach (var item in Globals.TouchCycleLog)
                    {
                        foreach (var time in item.Events)
                        {
                            fs.Write(time.Item1 + ",");
                        }
                        fs.Write("\n");
                    }
                    Globals.TouchCycleLog.Clear();
                }
            }
        }

        private void LoadSong(SongMetadata meta)
        {
            currentSongNotes = new NoteCollection(meta);
            currentSongMeta = currentSongNotes.ParseFile();

            musicManager.LoadSong(currentSongMeta.FilePath + currentSongMeta.SongFilename, currentSongMeta.BpmEvents);
            musicManager.Offset = currentSongMeta.PlaybackOffset * 1000;

            // Preload all textures
            currentSongNotes.PreloadTextures(currentUserSettings);
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
