﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
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
        int Width { get { return graphics.PreferredBackBufferWidth; } }
        int Height { get { return graphics.PreferredBackBufferHeight; } }

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
        int currentFolderIndex = 0;
        int selectedFolderIndex = -1;
        List<FolderParams> folderParams = new List<FolderParams>();

        KeyboardState prevKbState;
        MouseState prevMouseState;

        TouchPoints touchCollection = new TouchPoints();
        MotionCollection motionCollection = new MotionCollection();
        GradeCollection gradeCollection = new GradeCollection();

        bool enterLoadingScreen;
        bool leavingLoadingScreen;
        double loadingScreenTime;
        int loadingScreenTransition = 400;
        int loadingScreenWait = 1000;
        Texture2D loadingScreenImageUL;
        Texture2D loadingScreenImageUR;
        Texture2D loadingScreenImageLL;
        Texture2D loadingScreenImageLR;
        Rectangle loadingScreenRect;

        // Touch screen stuff
        bool useTouch;
        TouchCollection touchPointCollection;

        // Kinect stuff
        KinectTouch kinect = new KinectTouch();

        // Calibration stuff
        int calWait = 10000;    // 10 seconds between calibration times
        double calStart;
        CalibrationStage calStage;

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

            // Read Kinect calibration file if it exists
            kinect.ReadCalibrationFile("kinect.cal");

#if WINDOWS_UAP
            // HELLO
            Console.WriteLine("UAP??");
#elif WINDOWS
            // HELLO??
            Console.WriteLine("Windows??");
#endif

            var touchCap = TouchPanel.GetCapabilities();
            useTouch = touchCap.IsConnected;
            if (useTouch)
            {
                TouchPanel.EnableMouseTouchPoint = true;
                TouchPanel.EnabledGestures = GestureType.Tap | GestureType.Hold | GestureType.FreeDrag | GestureType.DragComplete;
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
            background = Content.Load<Texture2D>("Background");
            loadingScreenImageUL = Content.Load<Texture2D>("LoadingScreenUL");
            loadingScreenImageUR = Content.Load<Texture2D>("LoadingScreenUR");
            loadingScreenImageLL = Content.Load<Texture2D>("LoadingScreenLL");
            loadingScreenImageLR = Content.Load<Texture2D>("LoadingScreenLR");

            debugFont = Content.Load<SpriteFont>("DebugFont");
            Globals.Font = new Dictionary<string, SpriteFont>();
            Globals.Font.Add("Regular", Content.Load<SpriteFont>("Fonts/Roboto/Roboto-Regular"));
            Globals.Font.Add("Bold", Content.Load<SpriteFont>("Fonts/Roboto/Roboto-Bold"));
            Globals.Font.Add("Italic", Content.Load<SpriteFont>("Fonts/Roboto/Roboto-Italic"));
            Globals.Font.Add("BoldItalic", Content.Load<SpriteFont>("Fonts/Roboto/Roboto-BoldItalic"));

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
            folderParams.Add(new FolderParams() { Type = SortType.Alpha, Value = 0, Category = "BY TITLE", Name = "ALL SONGS" });
            folderParams.Add(new FolderParams() { Type = SortType.Alpha, Value = 1, Category = "BY ARTIST", Name = "ALL SONGS" });
            for (int i = 1; i <= 10; i++)
            {
                folderParams.Add(new FolderParams() { Type = SortType.Level, Value = i, Category = "DIFFICULTY", Name = "LEVEL " + i.ToString() });
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
            Globals.TotalGameMS = gameTime.TotalGameTime.TotalMilliseconds;

            KeyboardState kbState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (kbState.IsKeyDown(Keys.F4))
                Exit();

            if ((kbState.IsKeyDown(Keys.RightAlt) || kbState.IsKeyDown(Keys.LeftAlt)) && kbState.IsKeyDown(Keys.Enter) && !prevKbState.IsKeyDown(Keys.Enter))
                graphics.ToggleFullScreen();

            if (kbState.IsKeyDown(Keys.F1) && !prevKbState.IsKeyDown(Keys.F1))
            {
                currentMode = Mode.Calibration;
                calStage = CalibrationStage.Start;
            }

            if (useTouch)
                touchPointCollection = TouchPanel.GetState();

            if(touchPointCollection.Count > 0)
            {
                foreach (var pt in touchPointCollection)
                {
                    Console.WriteLine(pt.Position.X + ", " + pt.Position.Y);
                }
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

                    if (useTouch)
                    {
                        

                        if (TouchPanel.IsGestureAvailable)
                        {

                        }
                    }

                    if (kbState.IsKeyDown(Keys.Down) && !prevKbState.IsKeyDown(Keys.Down) || (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue))
                    {
                        if (selectedFolderIndex == -1)
                            currentFolderIndex = currentFolderIndex < (folderParams.Count - 1) ? ++currentFolderIndex : folderParams.Count - 1;
                        else
                            currentSongIndex = currentSongIndex < (songlist.Count - 1) ? ++currentSongIndex : songlist.Count - 1;
                    }

                    if (kbState.IsKeyDown(Keys.Up) && !prevKbState.IsKeyDown(Keys.Up) || (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue))
                    {
                        if (selectedFolderIndex == -1)
                            currentFolderIndex = currentFolderIndex > 0 ? --currentFolderIndex : 0;
                        else
                            currentSongIndex = currentSongIndex > 0 ? --currentSongIndex : 0;
                    }

                    if ((kbState.IsKeyDown(Keys.Enter) && kbState.IsKeyUp(Keys.LeftAlt) && kbState.IsKeyUp(Keys.RightAlt) && !prevKbState.IsKeyDown(Keys.Enter))
                        || (!useTouch && (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed)))
                    {
                        if (selectedFolderIndex == -1)
                        {
                            selectedFolderIndex = currentFolderIndex;
                            switch (folderParams[selectedFolderIndex].Type)
                            {
                                case SortType.Alpha:
                                    if (folderParams[selectedFolderIndex].Value == 0)
                                        songlist = songlist.OrderBy(x => x.Title).ToList();
                                    else
                                        songlist = songlist.OrderBy(x => x.Artist).ToList();
                                    break;
                                case SortType.Level:
                                    songlist = songlist.OrderBy(x => x.Level).ToList();
                                    currentSongIndex = songlist.FindLastIndex(x => x.Level < folderParams[selectedFolderIndex].Value) + 1;
                                    break;
                                case SortType.Genre:
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            LoadSong(songlist[currentSongIndex]);
                            //currentMode = Mode.GamePlay;
                            enterLoadingScreen = true;
                            loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }

                    if ((kbState.IsKeyDown(Keys.Escape) && !prevKbState.IsKeyDown(Keys.Escape)) || (kbState.IsKeyDown(Keys.Back) && !prevKbState.IsKeyDown(Keys.Back))
                        || (!useTouch && (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton != ButtonState.Pressed)))
                    {
                        selectedFolderIndex = -1;
                        currentSongIndex = 0;
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
                                loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                                break;
                            }
                            else
                            {
                                leavingLoadingScreen = false;
                                musicManager.Play();
                            }
                        }
                        else
                            break;
                    }

                    // Figure out if song is finished
                    if (kbState.IsKeyDown(Keys.Escape) || musicManager.IsFinished)
                    {
                        enterLoadingScreen = true;
                        loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    }

                    // Steps, which will hopefully move to an HID event class later
                    var currentBeat = hitBeat = musicManager.GetCurrentBeat();
                    var stepList = new List<Note>(currentSongNotes.Steps.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit));
                    stepList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

                    var holdList = new List<Hold>(
                        currentSongNotes.Holds.Where(x =>
                        Math.Abs(x.StartNote.BeatLocation - currentBeat) < 2 || (x.StartNote.BeatLocation < currentBeat && x.Notes.Last().BeatLocation > currentBeat)));

                    var motionList = new List<Note>(currentSongNotes.Motions.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit));
                    motionList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

                    // Touch screen controls
                    if(useTouch)
                    {
                        //var touchCol = TouchPanel.GetState();
                        //var width = TouchPanel.DisplayWidth;
                        //foreach (var loc in touchCol)
                        //{
                        //    if (loc.State == TouchLocationState.Pressed)
                        //        touchCollection.Points.Add(new TouchPoint(currentBeat) { RawX = (int)(loc.Position.X / width) * 1024, RawY = 500, RawWidth = 128, RawHeight = 20, ID = loc.Id });
                        //    else if (loc.State == TouchLocationState.Moved)
                        //    {
                        //        var pt = touchCollection.Points.Where(x => x.ID == loc.Id);
                        //        if (pt.Count() > 0)
                        //            pt.ElementAt(0).RawX = (int)(loc.Position.X / width) * 1024;
                        //    }
                        //    else if (loc.State == TouchLocationState.Released)
                        //    {
                        //        touchCollection.Points.RemoveAll(x => x.ID == loc.Id);
                        //    }
                        //}
                    }

                    // Temporary keyboard inputs
                    for (int i = 0; i < 8; i++)
                    {
                        if (kbState.IsKeyDown(touchkeys[i]) && !prevKbState.IsKeyDown(touchkeys[i]))
                        {
                            if (stepList.Count > 0)
                                closestNoteBeat = stepList.First().BeatLocation;
                            int id = random.Next(0, int.MaxValue);
                            touchCollection.Points.Add(new TouchPoint(currentBeat) { RawX = (int)(1024 / 8 * (i + 0.5)), RawY = 500, RawWidth = 128, RawHeight = 20, ID = id });
                            KeyDictionary.Add(touchkeys[i], id);

                            motionCollection.JumpBeat = double.NaN;
                        }
                        else if (!kbState.IsKeyDown(touchkeys[i]) && prevKbState.IsKeyDown(touchkeys[i]))
                        {
                            touchCollection.RemoveID(KeyDictionary[touchkeys[i]]);
                            KeyDictionary.Remove(touchkeys[i]);

                            if (touchCollection.Points.Count == 0)
                                motionCollection.JumpBeat = currentBeat;
                        }
                    }
                    if (kbState.IsKeyDown(Keys.Space) && !prevKbState.IsKeyDown(Keys.Space))
                        motionCollection.DownBeat = currentBeat;

                    // Kinect inputs
                    if (kinect.Points.Count >= 2)    // Right now it will only equal 0 or 2
                    {
                        if (kinect.Points[0].VState == VerticalState.InAir)
                            touchCollection.RemoveID(0);
                        else
                        {
                            var pt = touchCollection.Points.FirstOrDefault(x => x.ID == 0);
                            if (pt != null)
                                pt.RawX = (int)kinect.Points[0].X;
                            else
                                touchCollection.Points.Add(new TouchPoint(currentBeat) { RawX = (int)kinect.Points[0].X, RawY = 500, RawWidth = 128, RawHeight = 20, ID = 0 });
                        }

                        if (kinect.Points[1].VState == VerticalState.InAir)
                            touchCollection.RemoveID(1);
                        else
                        {
                            var pt = touchCollection.Points.FirstOrDefault(x => x.ID == 1);
                            if (pt != null)
                                pt.RawX = (int)kinect.Points[1].X;
                            else
                                touchCollection.Points.Add(new TouchPoint(currentBeat) { RawX = (int)kinect.Points[1].X, RawY = 500, RawWidth = 128, RawHeight = 20, ID = 1 });
                        }

                        //if (touchCollection.Points.Count < 1)
                        //    touchCollection.Points.Add(new TouchPoint(currentBeat) { RawX = (int)kinect.Points[0].X, RawY = 500, RawWidth = 128, RawHeight = 20, ID = 0 });
                        //else
                        //    touchCollection.Points[0].RawX = (int)kinect.Points[0].X;

                        //if(touchCollection.Points.Count < 2)
                        //    touchCollection.Points.Add(new TouchPoint(currentBeat) { RawX = (int)kinect.Points[1].X, RawY = 500, RawWidth = 128, RawHeight = 20, ID = 1 });
                        //else
                        //    touchCollection.Points[1].RawX = (int)kinect.Points[1].X;
                    }
                    else
                        touchCollection.Points.Clear();

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
                        if (!hold.StartNote.HitResult.WasHit)
                        {
                            var stepTimeMS = ((hold.StartNote.BeatLocation - currentBeat) * 60 / Globals.CurrentBpm);
                            if (stepTimeMS < -NoteTiming.Bad)
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
                        if (motionTimeMS < -MotionTiming.Miss)
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
                    if (kbState.IsKeyDown(Keys.R) && !prevKbState.IsKeyDown(Keys.R))
                        gradeCollection.Set(gameTime, currentSongNotes.Steps.First());

                    // Mostly Debug things
                    if (kbState.IsKeyDown(Keys.Enter) && kbState.IsKeyUp(Keys.LeftAlt) && kbState.IsKeyUp(Keys.RightAlt) && !prevKbState.IsKeyDown(Keys.Enter))
                    {
                        if (!musicManager.IsPlaying)
                            musicManager.Play();
                        else
                            musicManager.Pause();
                    }

                    if (kbState.IsKeyDown(Keys.Down) && !prevKbState.IsKeyDown(Keys.Down))
                        Globals.BeatToWorldYUnits /= 2;

                    if (kbState.IsKeyDown(Keys.Up) && !prevKbState.IsKeyDown(Keys.Up))
                        Globals.BeatToWorldYUnits *= 2;

                    if (kbState.IsKeyDown(Keys.Delete) && !prevKbState.IsKeyDown(Keys.Delete))
                        drawRateMin = float.PositiveInfinity;

                    if (kbState.IsKeyDown(Keys.P) && !prevKbState.IsKeyDown(Keys.P))
                        ExportLog();

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

                    if (kbState.IsKeyDown(Keys.Enter) && kbState.IsKeyUp(Keys.LeftAlt) && kbState.IsKeyUp(Keys.RightAlt) && !prevKbState.IsKeyDown(Keys.Enter))
                    {
                        enterLoadingScreen = true;
                        loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    break;
                case Mode.Calibration:
                    if (calStage == CalibrationStage.Start)
                    {
                        if (kbState.IsKeyDown(Keys.Enter) && kbState.IsKeyUp(Keys.LeftAlt) && kbState.IsKeyUp(Keys.RightAlt) && !prevKbState.IsKeyDown(Keys.Enter))
                        {
                            kinect.SetCal(CalibrationStage.Start);  // Clear cal flag
                            calStage = CalibrationStage.FrontLeft;
                            calStart = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                    else if (calStage == CalibrationStage.Finish)
                    {
                        if (kbState.IsKeyDown(Keys.Enter) && kbState.IsKeyUp(Keys.LeftAlt) && kbState.IsKeyUp(Keys.RightAlt) && !prevKbState.IsKeyDown(Keys.Enter))
                        {
                            kinect.WriteCalibrationFile("kinect.cal");
                            currentMode = Mode.SongSelect;
                        }
                    }
                    else
                    {
                        if((gameTime.TotalGameTime.TotalMilliseconds - calStart) > calWait)
                        {

                            if(kinect.SetCal(calStage))
                            {
                                switch (calStage)
                                {
                                    case CalibrationStage.FrontLeft:
                                        calStage = CalibrationStage.FrontRight;
                                        break;
                                    case CalibrationStage.FrontRight:
                                        calStage = CalibrationStage.BackLeft;
                                        break;
                                    case CalibrationStage.BackLeft:
                                        calStage = CalibrationStage.BackRight;
                                        break;
                                    case CalibrationStage.BackRight:
                                        calStage = CalibrationStage.Finish;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            calStart = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                    break;
                default:
                    break;
            }
            
            // TODO: Add your update logic here
            updateRate = 1.0f / (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);


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
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
                    spriteBatch.Draw(Globals.Textures["SongSelectionBG"], new Vector2(0, 0), Color.White);
                    spriteBatch.End();

                    SongSelection.DrawSelectionFrame(spriteBatch);


                    for (int i = 0; i < folderParams.Count; i++)
                    {
                        SongSelection.DrawFolder(spriteBatch, folderParams[i].Category, folderParams[i].Name, i - currentFolderIndex, selectedFolderIndex == -1);
                    }
                    if(currentFolderIndex < 2)
                    {
                        for (int i = -2; i < 0; i++)
                            SongSelection.DrawFolder(spriteBatch, null, null, i - currentFolderIndex, selectedFolderIndex == -1);
                    }
                    else if ((folderParams.Count - currentFolderIndex) < 4)
                    {
                        for (int i = folderParams.Count; i < folderParams.Count+4; i++)
                            SongSelection.DrawFolder(spriteBatch, null, null, i - currentFolderIndex, selectedFolderIndex == -1);
                    }

                    if (selectedFolderIndex >= 0)
                    {
                        for (int i = 0; i < songlist.Count; i++)
                        {
                            songlist[i].Draw(spriteBatch, i - currentSongIndex);
                        }
                    }
                    if (currentSongIndex < 2 && selectedFolderIndex >= 0)
                    {
                        for (int i = -2; i < 0; i++)
                            SongSelection.DrawFolder(spriteBatch, null, null, i - currentSongIndex, true);
                    }
                    else if ((songlist.Count - currentSongIndex) < 4 && selectedFolderIndex >= 0)
                    {
                        for (int i = songlist.Count; i < songlist.Count + 4; i++)
                            SongSelection.DrawFolder(spriteBatch, null, null, i - currentSongIndex, true);
                    }

                    //spriteBatch.Begin();
                    //for (int i = 0; i < songlist.Count; i++)
                    //{
                    //    spriteBatch.Draw(songlist[i].AlbumImage, new Rectangle(50, graphics.PreferredBackBufferHeight / 2 - 60 + 120 * (i - currentSongIndex), 200, 120), Color.White);
                    //}
                    //spriteBatch.End();

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
                    if(false)
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

                    // Draw Grades
                    spriteBatch.Begin();
                    gradeCollection.Draw(spriteBatch, gameTime);
                    spriteBatch.End();

                    // Draw hit stats
                    if(true)
                    {
                        var numStepHit = currentSongNotes.Steps.Where(x => x.HitResult.WasHit && x.HitResult.Difference != Timing.MissFlag).Count();
                        var numMotionHit = currentSongNotes.Motions.Where(x => x.HitResult.WasHit && x.HitResult.Difference != Timing.MissFlag).Count();

                        spriteBatch.Begin();
                        spriteBatch.DrawString(debugFont, "Steps: " + numStepHit + " / " + currentSongNotes.Steps.Count, new Vector2(1100, 300), Color.White);
                        spriteBatch.DrawString(debugFont, "Motions: " + numMotionHit + " / " + currentSongNotes.Motions.Count, new Vector2(1100, 320), Color.White);
                        spriteBatch.End();
                    }

                    if (enableProfiling)
                        log.AddEvent(stopwatch.ElapsedMilliseconds, "FPS counters drawn");

                    if (enterLoadingScreen || leavingLoadingScreen)
                        DrawLoadingTransition(gameTime);
                    break;
                case Mode.Results:
                    GraphicsDevice.Clear(Color.LightSkyBlue);

                    ResultScreen.Draw(spriteBatch, currentSongNotes);

                    if (enterLoadingScreen || leavingLoadingScreen)
                        DrawLoadingTransition(gameTime);
                    break;
                case Mode.Calibration:
                    GraphicsDevice.Clear(Color.Black);
                    double remainingTime = (calWait - (gameTime.TotalGameTime.TotalMilliseconds - calStart)) / 1000;
                    spriteBatch.Begin();
                    switch (calStage)
                    {
                        case CalibrationStage.Start:
                            spriteBatch.DrawString(debugFont, "Press ENTER to begin calibration", new Vector2(200, 200), Color.White);
                            break;
                        case CalibrationStage.FrontLeft:
                            spriteBatch.DrawString(debugFont, "Stand facing the kinect in the front left position (Time: " + remainingTime.ToString("F3") + ")", new Vector2(200, 200), Color.White);
                            break;
                        case CalibrationStage.FrontRight:
                            spriteBatch.DrawString(debugFont, "Stand facing the kinect in the front right position (Time: " + remainingTime.ToString("F3") + ")", new Vector2(200, 200), Color.White);
                            break;
                        case CalibrationStage.BackLeft:
                            spriteBatch.DrawString(debugFont, "Stand facing the kinect in the back left position (Time: " + remainingTime.ToString("F3") + ")", new Vector2(200, 200), Color.White);
                            break;
                        case CalibrationStage.BackRight:
                            spriteBatch.DrawString(debugFont, "Stand facing the kinect in the back right position (Time: " + remainingTime.ToString("F3") + ")", new Vector2(200, 200), Color.White);
                            break;
                        case CalibrationStage.Finish:
                            spriteBatch.DrawString(debugFont, "Press ENTER to leave calibration and overwrite your previous file", new Vector2(200, 200), Color.White);
                            break;
                        default:
                            break;
                    }
                    spriteBatch.End();
                    break;
                default:
                    break;
            }

            // Draw Kinect status
            if (true)
            {
                spriteBatch.Begin();
                if (kinect.IsCalibrated)
                {
                    string printout = "Kinect is calibrated\n" + kinect.GetCalReadout();
                    if(kinect.Points.Count == 2)
                    {
                        //printout += "\nLeft Foot  X: " + kinect.LastLeftAnkleRaw.X + ", Y: " + kinect.LastLeftAnkleRaw.Y + ", Depth: " + kinect.LastLeftAnkleRaw.Depth;
                        //printout += "\nLEFT KNEE ANGLE: " + kinect.Points[0].KneeAngle.ToString("F2");
                        //printout += "\nLEFT RATIO: " + kinect.LeftAnkleRatio.ToString("F3");
                        //printout += "\nRight Foot  X: " + kinect.LastRightAnkleRaw.X + ", Y: " + kinect.LastRightAnkleRaw.Y + ", Depth: " + kinect.LastRightAnkleRaw.Depth;
                        //printout += "\nRIGHT KNEE ANGLE: " + kinect.Points[1].KneeAngle.ToString("F2");
                        //printout += "\nRIGHT RATIO: " + kinect.RightAnkleRatio.ToString("F3");

                        printout += "\nLeFt Foot Y: " + kinect.LastSkeleton.Joints[Microsoft.Kinect.JointType.AnkleLeft].Position.Y.ToString("F2");
                        printout += "\nRight Foot Y: " + kinect.LastSkeleton.Joints[Microsoft.Kinect.JointType.AnkleRight].Position.Y.ToString("F2");
                        printout += "\nFloor Z: " + kinect.FloorZ.ToString("F2");
                    }

                    spriteBatch.DrawString(debugFont, printout, new Vector2(10, 10), Color.Chartreuse);
                }
                else
                    spriteBatch.DrawString(debugFont, "Kinect is NOT calibrated", new Vector2(10, 10), Color.Red);
                spriteBatch.End();
            }

            base.Draw(gameTime);

            if (enableProfiling)
            {
                log.AddEvent(stopwatch.ElapsedMilliseconds, "Base Method");
                DrawCycleLog.Add(log);
            }
        }

        private void DrawLoadingTransition(GameTime gt)
        {
            var duration = gt.TotalGameTime.TotalMilliseconds - loadingScreenTime;
            var ratio = enterLoadingScreen ? 1 - duration / loadingScreenTransition : duration / loadingScreenTransition;
            var xL = Math.Min(0, 0 - Width / 2 * ratio);
            var xR = Math.Max(Width / 2, Width / 2 + Width / 2 * ratio);
            var yT = Math.Min(0, 0 - Height / 2 * ratio);
            var yB = Math.Max(Height / 2, Height / 2 + Height / 2 * ratio);
            spriteBatch.Begin();
            spriteBatch.Draw(loadingScreenImageUL, new Rectangle((int)xL, (int)yT, Width / 2, Height / 2), Color.White);
            spriteBatch.Draw(loadingScreenImageUR, new Rectangle((int)xR, (int)yT, Width / 2, Height / 2), Color.White);
            spriteBatch.Draw(loadingScreenImageLL, new Rectangle((int)xL, (int)yB, Width / 2, Height / 2), Color.White);
            spriteBatch.Draw(loadingScreenImageLR, new Rectangle((int)xR, (int)yB, Width / 2, Height / 2), Color.White);
            spriteBatch.End();
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

            musicManager.LoadSong(currentSongMeta.FilePath + currentSongMeta.SongFilename, currentSongMeta.BpmEvents);
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
