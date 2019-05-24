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
        int Width { get { return graphics.PreferredBackBufferWidth; } }
        int Height { get { return graphics.PreferredBackBufferHeight; } }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Vector3 cameraPos = new Vector3(0, -44, 23);
        Vector3 cameraTarget = new Vector3(0, 36, 2);

        private Matrix view;
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800 / 480f, 0.1f, 300f);

        QuadTexture gradeZone;
        QuadTexture noteLanes, noteLaneAccent1l, noteLaneAccent1r, noteLaneAccent2l, noteLaneAccent2r;
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
        int currentLevelIndex = 0;
        int selectedFolderIndex = -1;
        int selectedLevelIndex = -1;
        List<FolderParams> folderParams = new List<FolderParams>();

        KeyboardState prevKbState;
        MouseState prevMouseState;

        TouchCollection touchCollection = new TouchCollection();
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

            this.graphics.SynchronizeWithVerticalRetrace = false;
            base.IsFixedTimeStep = false;


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
            noteLaneAccent1l = new QuadTexture(Content.Load<Texture2D>("NoteLaneAccent1"));
            noteLaneAccent1l.SetVerts(Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth, Globals.GradeZoneWidth / 2, -20, Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth, Globals.GradeZoneWidth / 2, 300, -0.15f);
            noteLaneAccent1r = new QuadTexture(Content.Load<Texture2D>("NoteLaneAccent1"));
            noteLaneAccent1r.SetVerts(-Globals.GradeZoneWidth / 2, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth, -20, -Globals.GradeZoneWidth / 2, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth, 300, -0.15f);
            noteLaneAccent2l = new QuadTexture(Content.Load<Texture2D>("NoteLaneAccent2"));
            noteLaneAccent2l.SetVerts(Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth * 2, Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth, -20, Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth * 2, Globals.GradeZoneWidth / 2 + Globals.NoteLaneAccentWidth, 300, -0.15f);
            noteLaneAccent2r = new QuadTexture(Content.Load<Texture2D>("NoteLaneAccent2"));
            noteLaneAccent2r.SetVerts(-Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth * 2, -20, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth, -Globals.GradeZoneWidth / 2 - Globals.NoteLaneAccentWidth * 2, 300, -0.15f);
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
            Globals.Font.Add("Franklin", Content.Load<SpriteFont>("Fonts/libre-franklin/librefranklin-blackitalic"));

            // Load songs
            DirectoryInfo di = new DirectoryInfo("Songs");
            var folders = di.EnumerateDirectories();
            foreach (var folder in folders)
            {
                var files = folder.EnumerateFiles();
                var charts = files.Where(f => f.FullName.EndsWith(".ssf"));
                if (charts != null && charts.Count() > 0)
                {
                    foreach (var chart in charts)
                        songlist.Add(new SongMetadata(chart.FullName));
                }
            }
            folderParams.Add(new FolderParams() { Type = SortType.Title, Name = "SORT BY\nTITLE" });
            folderParams.Add(new FolderParams() { Type = SortType.Artist, Name = "SORT BY\nARTIST" });
            folderParams.Add(new FolderParams() { Type = SortType.Level, Name = "SORT BY\nLEVEL" });
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
            MouseState mouseState = Mouse.GetState();

            if (kbState.IsKeyDown(Keys.F4))
                Exit();

            if ((kbState.IsKeyDown(Keys.RightAlt) || kbState.IsKeyDown(Keys.LeftAlt)) && kbState.IsKeyDown(Keys.Enter) && !prevKbState.IsKeyDown(Keys.Enter))
                graphics.ToggleFullScreen();

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

                    // TEMP FOR TESTING
                    //LoadSong(songlist[6]);
                    ////currentMode = Mode.GamePlay;
                    //enterLoadingScreen = true;
                    //loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                    // END TEMP


                    if (kbState.IsKeyDown(Keys.Down) && !prevKbState.IsKeyDown(Keys.Down) || (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue))
                    {
                        if (selectedFolderIndex == -1)
                            currentFolderIndex = currentFolderIndex < (folderParams.Count - 1) ? ++currentFolderIndex : folderParams.Count - 1;
                        else if (folderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                            currentLevelIndex = currentLevelIndex < 9 ? ++currentLevelIndex : 9;
                        else
                            currentSongIndex = currentSongIndex < (songlist.Count - 1) ? ++currentSongIndex : songlist.Count - 1;
                    }

                    if (kbState.IsKeyDown(Keys.Up) && !prevKbState.IsKeyDown(Keys.Up) || (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue))
                    {
                        if (selectedFolderIndex == -1)
                            currentFolderIndex = currentFolderIndex > 0 ? --currentFolderIndex : 0;
                        else if (folderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                            currentLevelIndex = currentLevelIndex > 0 ? --currentLevelIndex : 0;
                        else
                            currentSongIndex = currentSongIndex > 0 ? --currentSongIndex : 0;
                    }

                    if ((kbState.IsKeyDown(Keys.Enter) && kbState.IsKeyUp(Keys.LeftAlt) && kbState.IsKeyUp(Keys.RightAlt) && !prevKbState.IsKeyDown(Keys.Enter))
                        || (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed))
                    {
                        if (selectedFolderIndex == -1)
                        {
                            selectedFolderIndex = currentFolderIndex;
                            switch (folderParams[selectedFolderIndex].Type)
                            {
                                case SortType.Title:
                                    songlist = songlist.OrderBy(x => x.Title).ToList();
                                    break;
                                case SortType.Artist:
                                    songlist = songlist.OrderBy(x => x.Artist).ToList();
                                    break;
                                case SortType.Level:
                                    selectedLevelIndex = -1;
                                    break;
                                case SortType.Genre:
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (folderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                        {
                            selectedLevelIndex = currentLevelIndex;
                            songlist = songlist.OrderBy(x => x.Level).ToList();
                            currentSongIndex = songlist.FindLastIndex(x => x.Level < (selectedLevelIndex + 1)) + 1;
                        }
                        else
                        {
                            LoadSong(songlist[currentSongIndex]);
                            //currentMode = Mode.GamePlay;
                            enterLoadingScreen = true;
                            loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }

                    if ((kbState.IsKeyDown(Keys.Escape) && !prevKbState.IsKeyDown(Keys.Escape)) || 
                        (kbState.IsKeyDown(Keys.Back) && !prevKbState.IsKeyDown(Keys.Back)) ||
                        ((kbState.IsKeyDown(Keys.Left) && !prevKbState.IsKeyDown(Keys.Left)) ||
                        (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton != ButtonState.Pressed)))
                    {
                        if (selectedFolderIndex != -1 && folderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex != -1)
                        {
                            selectedLevelIndex = -1;
                        }
                        else
                        {
                            selectedFolderIndex = -1;
                            currentSongIndex = 0;
                        }
                    }

                    break;
                case Mode.Loading:
                    break;
                case Mode.GamePlay:
                    // If we're entering or leaving a loading screen, block other inputs
                    if(enterLoadingScreen || leavingLoadingScreen)
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

                                // Reset inputs
                                motionCollection.DownBeat = double.NaN;
                                motionCollection.JumpBeat = double.NaN;
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
                    var currentTime = Globals.GetSecAtBeat(currentBeat);
                    var stepList = new List<Note>(currentSongNotes.Steps.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit));
                    stepList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

                    var holdList = new List<Hold>(
                        currentSongNotes.Holds.Where(x =>
                        Math.Abs(x.StartNote.BeatLocation - currentBeat) < 64 || (x.StartNote.BeatLocation < currentBeat && x.Notes.Last().BeatLocation > currentBeat)));

                    var motionList = new List<Note>(currentSongNotes.Motions.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit));
                    motionList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

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
                            if (touchCollection.Points.Count(pt => pt.ID == KeyDictionary[touchkeys[i]]) > 0)
                            {
                                touchCollection.RemoveID(KeyDictionary[touchkeys[i]]);
                                KeyDictionary.Remove(touchkeys[i]);
                            }

                            if (touchCollection.Points.Count == 0)
                                motionCollection.JumpBeat = currentBeat;
                        }
                    }
                    if (kbState.IsKeyDown(Keys.Space) && !prevKbState.IsKeyDown(Keys.Space))
                        motionCollection.DownBeat = currentBeat;
                    else
                        motionCollection.DownBeat = double.NaN;

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
                            //var stepTimeMS = ((hold.StartNote.BeatLocation - currentBeat) * 60 / Globals.CurrentBpm);
                            var stepTimeMS = Globals.GetSecAtBeat(hold.StartNote.BeatLocation) - currentTime;
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
                        //var motionTimeMS = ((motion.BeatLocation - currentBeat) * 60 / Globals.CurrentBpm);
                        var motionTimeMS = Globals.GetSecAtBeat(motion.BeatLocation) - currentTime;
                        if (motionTimeMS < -MotionTiming.Miss)
                        {
                            motion.HitResult.WasHit = true;
                            motion.HitResult.Difference = Timing.MissFlag;
                        }
                        else if (motion.Motion == Motion.Down && !double.IsNaN(motionCollection.DownBeat))
                        {
                            if (motionCollection.CheckHit(motion))
                                gradeCollection.Set(gameTime, motion);
                        }
                        else if (motion.Motion == Motion.Up && motionTimeMS < MotionTiming.JumpPerfectCheck)
                        {
                            // If there's no feet on the pad within the perfect window, it counts
                            if(touchCollection.Points.Count == 0)
                            {
                                motion.HitResult.WasHit = true;
                                motion.HitResult.Difference = (float)motionTimeMS;
                                gradeCollection.Set(gameTime, motion);
                            }
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
                        //Globals.BeatToWorldYUnits /= 2;
                        Globals.SpeedScale -= 0.5;

                    if (kbState.IsKeyDown(Keys.Up) && !prevKbState.IsKeyDown(Keys.Up))
                        //Globals.BeatToWorldYUnits *= 2;
                        Globals.SpeedScale += 0.5;

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

                    // Draw order:
                    // 1. BG
                    // 2. Menu Bar
                    // 3. Active Selection
                    // 4. Songs/Folders
                    // 5. UI elements

                    spriteBatch.Begin();
                    spriteBatch.Draw(Globals.Textures["SsBgLine"], Globals.Origin, ThemeColors.Blue);
                    spriteBatch.Draw(Globals.Textures["SsActive"], Globals.Origin, Color.White);
                    if (selectedFolderIndex == -1)
                    {
                        for (int i = 0; i < folderParams.Count; i++)
                        {
                            var cardOffset = Globals.ItemOrigin + (i - currentFolderIndex) * Globals.ItemOffset;

                            spriteBatch.Draw(Globals.Textures["SsItemBg"], cardOffset, ThemeColors.GetColor(i));
                            spriteBatch.Draw(Globals.Textures["SsAccentStar"], cardOffset, ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f));
                            spriteBatch.DrawString(Globals.Font["Franklin"], folderParams[i].Name, new Rectangle((int)cardOffset.X + 120, (int)cardOffset.Y + 22, 225, 88), Color.White);
                            spriteBatch.Draw(Globals.Textures["SsFrame"], cardOffset, Color.White);
                        }
                    }
                    else if(folderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            var cardOffset = Globals.ItemOrigin + (i - currentLevelIndex) * Globals.ItemOffset;

                            spriteBatch.Draw(Globals.Textures["SsItemBg"], cardOffset, ThemeColors.GetColor(i));
                            spriteBatch.Draw(Globals.Textures["SsAccentStar"], cardOffset, ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f));
                            spriteBatch.DrawString(Globals.Font["Franklin"], "LEVEL" + (i + 1), new Rectangle((int)cardOffset.X + 120, (int)cardOffset.Y + 22, 225, 88), Color.White);
                            spriteBatch.Draw(Globals.Textures["SsFrame"], cardOffset, Color.White);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < songlist.Count; i++)
                        {
                            var cardOffset = Globals.ItemOrigin + (i - currentSongIndex) * Globals.ItemOffset;

                            var bgCol = songlist[i].ColorBack != ThemeColors.NullColor ? songlist[i].ColorBack : ThemeColors.GetColor(i);
                            var foreCol = songlist[i].ColorFore != ThemeColors.NullColor ? songlist[i].ColorFore : ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f);

                            spriteBatch.Draw(Globals.Textures["SsItemBg"], cardOffset, bgCol);
                            spriteBatch.Draw(Globals.Textures["SsAccentStar"], cardOffset, foreCol);
                            spriteBatch.Draw(Globals.Textures["SsAccentAlbum"], cardOffset, foreCol);
                            spriteBatch.Draw(songlist[i].AlbumImage, new Rectangle((int)cardOffset.X + 284, (int)cardOffset.Y + 12, 96, 96), Color.White);
                            spriteBatch.Draw(Globals.Textures["SsAlbumFrame"], cardOffset, Color.White);
                            spriteBatch.DrawString(Globals.Font["Franklin"], songlist[i].Title, new Rectangle((int)cardOffset.X + 70, (int)cardOffset.Y + 16, 200, 38), Color.White);
                            spriteBatch.DrawString(Globals.Font["Franklin"], songlist[i].Artist, new Rectangle((int)cardOffset.X + 108, (int)cardOffset.Y + 62, 160, 36), Color.White);
                            spriteBatch.Draw(Globals.Textures["SsFrame"], cardOffset, Color.White);
                        }
                    }
                    spriteBatch.End();

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

                    if (enableProfiling)
                        log.AddEvent(stopwatch.ElapsedMilliseconds, "GradeZone Drawn");

                    if (currentSongNotes != null)
                    {
                        var currentBeat = musicManager.GetCurrentBeat();
                        var motions = currentSongNotes.Motions.Where(p => p.BeatLocation > currentBeat - 6 && p.BeatLocation < currentBeat + 16);
                        var holds = currentSongNotes.Holds.Where(p => p.StartNote.BeatLocation > currentBeat - 16 && p.StartNote.BeatLocation < currentBeat + 16);
                        var notes = currentSongNotes.Steps.Where(p => p.BeatLocation > currentBeat - 6 && p.BeatLocation < currentBeat + 16);
                        var marks = currentSongNotes.Markers.Where(p => p.BeatLocation > currentBeat - 6 && p.BeatLocation < currentBeat + 16);

                        if (enableProfiling)
                            log.AddEvent(stopwatch.ElapsedMilliseconds, "Lists generated");

                        // Draw beat markers
                        foreach (var mark in marks)
                            mark.Draw(currentBeat, view, projection);

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
                        spriteBatch.DrawString(debugFont, "Update Rate: " + updateRate.ToString("F2"), new Vector2(10, 10), Color.White);
                        spriteBatch.DrawString(debugFont, "Draw Rate: " + drawRate.ToString("F2"), new Vector2(10, 30), Color.White);
                        spriteBatch.DrawString(debugFont, "Draw Rate: " + drawRateMin.ToString("F2"), new Vector2(10, 50), Color.White);
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

                    // Draw UI elements
                    spriteBatch.Begin();
                    spriteBatch.Draw(Globals.Textures["GpLowerBG"], new Vector2(0, 599), Color.Black);
                    float yTopRow = 625f;
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], "SCROLL", new Vector2(50, yTopRow), Color.White, 0.08f, Justification.Center);
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], "ACCURACY", new Vector2(100, yTopRow), Color.White, 0.08f, Justification.Left);
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], "NORMAL", new Vector2(1200, yTopRow), Color.White, 0.08f, Justification.Center);
                    float yBottomRow = 685f;
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], "1.0", new Vector2(50, yBottomRow), Color.White, 0.2f, Justification.Center | Justification.Bottom);
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
