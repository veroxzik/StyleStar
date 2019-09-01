using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using System.Runtime.InteropServices;
using Nett;

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

        // Song Selection
        List<SongMetadata> songlist = new List<SongMetadata>();
        int currentSongIndex = 0;
        int currentFolderIndex = 0;
        int currentLevelIndex = 0;
        int selectedFolderIndex = -1;
        int selectedLevelIndex = -1;
        int currentSongLevelIndex = 0;  // Used to track difficulty switches
        List<FolderParams> folderParams = new List<FolderParams>();

        KeyboardState prevKbState;
        MouseState prevMouseState;
        bool disableMouseClick;

        bool TouchScreenConnected = false;

        TouchWindowsHook touchHookWin;

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

        // Hit Debug
        double hitBeat = 0;
        double closestNoteBeat = 0;
        Keys[] touchkeys = new Keys[] { Keys.A, Keys.S, Keys.D, Keys.F, Keys.J, Keys.K, Keys.L, Keys.OemSemicolon };
        bool[] prevKeys = new bool[8];
        Dictionary<Keys, uint> KeyDictionary = new Dictionary<Keys, uint>();
        Random random = new Random();
        bool horzMotion = false;

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

            //m_event = Hook.GlobalEvents();
            //m_event.TouchMove += M_event_TouchMove;

            // Load config file
            if (File.Exists(Defines.ConfigFile))
            {
                var configTable = Toml.ReadFile(Defines.ConfigFile).ToDictionary();

                if(configTable.ContainsKey("KeyConfig"))
                {
                    InputMonitor.SetKeys((Dictionary<string, object>)configTable["KeyConfig"]);
                }
            }
        }

        private void M_event_TouchMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Console.WriteLine("TOUCH MOVE!!");
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

                touchHookWin = new TouchWindowsHook(touchCollection, musicManager);
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
            Globals.Font.Add("Regular", Content.Load<SpriteFont>("Fonts/Roboto/Roboto-Regular"));
            Globals.Font.Add("Bold", Content.Load<SpriteFont>("Fonts/Roboto/Roboto-Bold"));
            Globals.Font.Add("Italic", Content.Load<SpriteFont>("Fonts/Roboto/Roboto-Italic"));
            Globals.Font.Add("BoldItalic", Content.Load<SpriteFont>("Fonts/Roboto/Roboto-BoldItalic"));
            Globals.Font.Add("Franklin", Content.Load<SpriteFont>("Fonts/libre-franklin/librefranklin-blackitalic"));
            Globals.Font.Add("RunningStart", Content.Load<SpriteFont>("Fonts/RunningStart"));
            Globals.FontScalingFactor.Add(Globals.Font["Franklin"], new Tuple<float, float>(43.857f, -.486f));
            Globals.FontScalingFactor.Add(Globals.Font["RunningStart"], new Tuple<float, float>(160, 16));

            // Load songs
            DirectoryInfo di = new DirectoryInfo("Songs");
            var folders = di.EnumerateDirectories();
            foreach (var folder in folders)
            {
                // If a folder contains an *.ssi file, use that to load charts
                // Otherwise, load each chart individually
                var files = folder.EnumerateFiles();
                var info = files.Where(f => f.FullName.EndsWith(Defines.InfoExtension));
                if (info != null && info.Count() > 0)
                {
                    foreach (var file in info)
                    {
                        songlist.Add(new SongMetadata(file.FullName));
                    }
                }
                else
                {
                    var charts = files.Where(f => f.FullName.EndsWith(Defines.ChartExtension));
                    if (charts != null && charts.Count() > 0)
                    {
                        foreach (var chart in charts)
                            songlist.Add(new SongMetadata(chart.FullName));
                    }
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

            InputMonitor.Update(gameTime);

            if (InputMonitor.Monitors[Inputs.Exit].State == KeyState.Press)
                Exit();

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
                                var meta = songlist[currentSongIndex].IsMetadataFile ? songlist[currentSongIndex].ChildMetadata[currentSongLevelIndex] : songlist[currentSongIndex];
                                LoadSong(meta);
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

                    if (songlist.Count > 0)
                    {
                        if (InputMonitor.Monitors[Inputs.Down].State == KeyState.Press || (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue))
                        {
                            if (selectedFolderIndex == -1)
                                currentFolderIndex = currentFolderIndex < (folderParams.Count - 1) ? ++currentFolderIndex : folderParams.Count - 1;
                            else if (folderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                                currentLevelIndex = currentLevelIndex < 9 ? ++currentLevelIndex : 9;
                            else
                                currentSongIndex = currentSongIndex < (songlist.Count - 1) ? ++currentSongIndex : songlist.Count - 1;
                        }

                        if (InputMonitor.Monitors[Inputs.Up].State == KeyState.Press || (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue))
                        {
                            if (selectedFolderIndex == -1)
                                currentFolderIndex = currentFolderIndex > 0 ? --currentFolderIndex : 0;
                            else if (folderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                                currentLevelIndex = currentLevelIndex > 0 ? --currentLevelIndex : 0;
                            else
                                currentSongIndex = currentSongIndex > 0 ? --currentSongIndex : 0;
                        }

                        //if (kbState.IsKeyDown(Keys.Right) && !prevKbState.IsKeyDown(Keys.Right))
                        if(InputMonitor.Monitors[Inputs.Right].State == KeyState.Press)
                        {
                            currentSongLevelIndex++;
                            if (currentSongLevelIndex > 2)
                                currentSongLevelIndex = 0;
                        }

                        if (InputMonitor.Monitors[Inputs.Select].State == KeyState.Press
                            || (!disableMouseClick && (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed)))
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
                                if ((songlist[currentSongIndex].IsMetadataFile && songlist[currentSongIndex].ChildMetadata.FirstOrDefault(x => (int)x.Difficulty == currentSongLevelIndex) == null) ||
                                    (!songlist[currentSongIndex].IsMetadataFile && (int)songlist[currentSongIndex].Difficulty != currentSongLevelIndex))
                                    return;

                                enterLoadingScreen = true;
                                loadingScreenTime = gameTime.TotalGameTime.TotalMilliseconds;
                            }
                        }

                        if (InputMonitor.Monitors[Inputs.Back].State == KeyState.Press ||
                            InputMonitor.Monitors[Inputs.Back2].State == KeyState.Press ||
                            InputMonitor.Monitors[Inputs.Left].State == KeyState.Press ||
                            (!disableMouseClick && (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton != ButtonState.Pressed)))
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

                        if (InputMonitor.Monitors[Inputs.Auto].State == KeyState.Press)
                            Globals.IsAutoModeEnabled = !Globals.IsAutoModeEnabled;
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

                    // Steps, which will hopefully move to an HID event class later
                    var currentBeat = hitBeat = musicManager.GetCurrentBeat();
                    var currentTime = Globals.GetSecAtBeat(currentBeat);
                    var stepList = currentSongNotes.Steps.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit).ToList();
                    stepList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

                    var holdList = currentSongNotes.Holds.Where(x =>
                        Math.Abs(x.StartNote.BeatLocation - currentBeat) < 64 || (x.StartNote.BeatLocation < currentBeat && x.Notes.Last().BeatLocation > currentBeat)).ToList();

                    var motionList = currentSongNotes.Motions.Where(x => Math.Abs(x.BeatLocation - currentBeat) < 2 && !x.HitResult.WasHit).ToList();
                    motionList.Sort((x, y) => Math.Abs(x.BeatLocation - currentBeat).CompareTo(Math.Abs(y.BeatLocation - currentBeat)));

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
                                if(touchCollection.Points.TryAdd(id, new TouchPoint(currentBeat) { RawX = (int)(1024 / 8 * (i + 0.5)), RawY = 500, RawWidth = 128, RawHeight = 20, ID = id }));
                                    KeyDictionary.Add(touchkeys[i], id);

                                motionCollection.JumpBeat = double.NaN;
                                prevKeys[i] = true;
                            }
                            else if (!kbState.IsKeyDown(touchkeys[i]) && prevKbState.IsKeyDown(touchkeys[i]))
                            {
                                TouchPoint pt;
                                if(touchCollection.Points.TryRemove(KeyDictionary[touchkeys[i]], out pt))
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
                            if(Math.Abs(stepTimeMS) < NoteTiming.AutoTolerance)
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
                                if(Math.Abs(stepTimeMS) < NoteTiming.AutoTolerance)
                                {
                                    shuffle.HitResult.WasHit = true;
                                    shuffle.HitResult.Difference = 0;
                                    currentSongNotes.AddToScore(NoteType.Hold, shuffle.HitResult.Difference);
                                }
                            }
                            else if (horzMotion)
                            {
                                // Gotta see if we even remotely swiped the right area
                            }
                        }

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
                            if(touchCollection.Points.Count == 0)
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
                        Globals.SpeedScale -= 0.5;

                    if (InputMonitor.Monitors[Inputs.Up].State == KeyState.Press)
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
                    if (songlist.Count > 0)
                    {
                        spriteBatch.Draw(Globals.Textures["SsBgLine"], Globals.Origin, songlist[currentSongIndex].ColorAccent.IfNull(ThemeColors.Blue));
                        if (selectedFolderIndex == -1)
                        {
                            spriteBatch.Draw(Globals.Textures["SsActive"], Globals.Origin, Color.White);
                            for (int i = 0; i < folderParams.Count; i++)
                            {
                                var cardOffset = Globals.ItemOrigin + (i - currentFolderIndex) * Globals.ItemOffset;

                                spriteBatch.Draw(Globals.Textures["SsItemBg"], cardOffset, ThemeColors.GetColor(i));
                                spriteBatch.Draw(Globals.Textures["SsAccentStar"], cardOffset, ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f));
                                spriteBatch.DrawString(Globals.Font["Franklin"], folderParams[i].Name, new Rectangle((int)cardOffset.X + 120, (int)cardOffset.Y + 22, 225, 88), Color.White);
                                spriteBatch.Draw(Globals.Textures["SsFrame"], cardOffset, Color.White);
                            }

                            spriteBatch.Draw(Globals.Textures["SsFolderSelect"], Globals.ItemOrigin + new Vector2(480f, 28f), Color.White);
                        }
                        else if (folderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                        {
                            spriteBatch.Draw(Globals.Textures["SsActive"], Globals.Origin, Color.White);
                            for (int i = 0; i < 10; i++)
                            {
                                var cardOffset = Globals.ItemOrigin + (i - currentLevelIndex) * Globals.ItemOffset;

                                spriteBatch.Draw(Globals.Textures["SsItemBg"], cardOffset, ThemeColors.GetColor(i));
                                spriteBatch.Draw(Globals.Textures["SsAccentStar"], cardOffset, ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f));
                                spriteBatch.DrawString(Globals.Font["Franklin"], "LEVEL" + (i + 1), new Rectangle((int)cardOffset.X + 120, (int)cardOffset.Y + 22, 225, 88), Color.White);
                                spriteBatch.Draw(Globals.Textures["SsFrame"], cardOffset, Color.White);
                            }

                            spriteBatch.Draw(Globals.Textures["SsGoBack"], Globals.ItemOrigin + new Vector2(-40f, -70f), Color.White);
                            spriteBatch.Draw(Globals.Textures["SsFolderSelect"], Globals.ItemOrigin + new Vector2(480f, 28f), Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(Globals.Textures["SsActive"], Globals.Origin, songlist[currentSongIndex].ColorFore.IfNull(Color.White));
                            for (int i = 0; i < songlist.Count; i++)
                            {
                                var cardOffset = Globals.ItemOrigin + (i - currentSongIndex) * Globals.ItemOffset;

                                spriteBatch.Draw(Globals.Textures["SsItemBg"], cardOffset, songlist[i].ColorBack.IfNull(ThemeColors.GetColor(i)));
                                spriteBatch.Draw(Globals.Textures["SsAccentStar"], cardOffset, songlist[i].ColorFore.IfNull(ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f)));
                                spriteBatch.Draw(Globals.Textures["SsAccentAlbum"], cardOffset, songlist[i].ColorFore.IfNull(ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f)));
                                spriteBatch.Draw(songlist[i].AlbumImage, new Rectangle((int)cardOffset.X + 284, (int)cardOffset.Y + 12, 96, 96), Color.White);
                                spriteBatch.Draw(Globals.Textures["SsAlbumFrame"], cardOffset, Color.White);
                                spriteBatch.DrawString(Globals.Font["Franklin"], songlist[i].Title, new Rectangle((int)cardOffset.X + 70, (int)cardOffset.Y + 16, 200, 38), Color.White);
                                spriteBatch.DrawString(Globals.Font["Franklin"], songlist[i].Artist, new Rectangle((int)cardOffset.X + 108, (int)cardOffset.Y + 62, 160, 36), Color.White);
                                spriteBatch.Draw(Globals.Textures["SsFrame"], cardOffset, Color.White);
                                if (i == currentSongIndex)
                                {
                                    int[] difficulties = new int[3];
                                    int x = 423;
                                    spriteBatch.Draw(Globals.Textures["SsActiveDifficulty" + currentSongLevelIndex], new Vector2(cardOffset.X + 390, cardOffset.Y + 2), Color.White);
                                    if (songlist[i].IsMetadataFile)
                                    {
                                        foreach (var song in songlist[i].ChildMetadata)
                                            difficulties[(int)song.Difficulty] = song.Level;
                                    }
                                    else
                                        difficulties[(int)songlist[i].Difficulty] = songlist[i].Level;

                                    switch (currentSongLevelIndex)
                                    {
                                        case 0:
                                            if (difficulties[0] > 0)
                                                spriteBatch.DrawString(Globals.Font["Franklin"], difficulties[0].ToString("D2"), new Rectangle((int)cardOffset.X + x - 4, (int)cardOffset.Y + 17 + (39 * 0), 36, 32), Color.Black);
                                            if (difficulties[1] > 0)
                                                spriteBatch.DrawString(Globals.Font["Franklin"], difficulties[1].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 13 + (39 * 1), 20, 39), Color.Black);
                                            if (difficulties[2] > 0)
                                                spriteBatch.DrawString(Globals.Font["Franklin"], difficulties[2].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 2 + (39 * 2), 20, 39), Color.Black);
                                            break;
                                        case 1:
                                            if (difficulties[0] > 0)
                                                spriteBatch.DrawString(Globals.Font["Franklin"], difficulties[0].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 1 + (39 * 0), 20, 39), Color.Black);
                                            if (difficulties[1] > 0)
                                                spriteBatch.DrawString(Globals.Font["Franklin"], difficulties[1].ToString("D2"), new Rectangle((int)cardOffset.X + x - 4, (int)cardOffset.Y + 7 + (39 * 1), 36, 32), Color.Black);
                                            if (difficulties[2] > 0)
                                                spriteBatch.DrawString(Globals.Font["Franklin"], difficulties[2].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 2 + (39 * 2), 20, 39), Color.Black);
                                            break;
                                        case 2:
                                            if (difficulties[0] > 0)
                                                spriteBatch.DrawString(Globals.Font["Franklin"], difficulties[0].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 2 + (39 * 0), 20, 39), Color.Black);
                                            if (difficulties[1] > 0)
                                                spriteBatch.DrawString(Globals.Font["Franklin"], difficulties[1].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y - 8 + (39 * 1), 20, 39), Color.Black);
                                            if (difficulties[2] > 0)
                                                spriteBatch.DrawString(Globals.Font["Franklin"], difficulties[2].ToString("D2"), new Rectangle((int)cardOffset.X + x - 4, (int)cardOffset.Y - 7 + (39 * 2), 36, 32), Color.Black);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    spriteBatch.Draw(Globals.Textures["SsDifficultyBg"], cardOffset, Color.White);
                                    if (songlist[i].IsMetadataFile)
                                    {
                                        foreach (var song in songlist[i].ChildMetadata)
                                        {
                                            spriteBatch.DrawString(Globals.Font["Franklin"], song.Level.ToString("D2"), new Rectangle((int)cardOffset.X + 392, (int)cardOffset.Y + 2 + (39 * (int)song.Difficulty), 20, 39), Color.Black);
                                        }
                                    }
                                    else
                                        spriteBatch.DrawString(Globals.Font["Franklin"], songlist[i].Level.ToString("D2"), new Rectangle((int)cardOffset.X + 392, (int)cardOffset.Y + 2 + (39 * (int)songlist[i].Difficulty), 20, 39), Color.Black);
                                }

                            }

                            spriteBatch.Draw(Globals.Textures["SsGoBack"], Globals.ItemOrigin + new Vector2(-40f, -70f), Color.White);
                            spriteBatch.Draw(Globals.Textures["SsSongSelect"], Globals.ItemOrigin + new Vector2(480f, 28f), Color.White);

                            spriteBatch.DrawStringJustify(Globals.Font["Franklin"], songlist[currentSongIndex].Title, new Vector2(1220, 560), Color.White, 0.3f, Justification.Bottom | Justification.Right);
                            spriteBatch.DrawStringJustify(Globals.Font["Franklin"], songlist[currentSongIndex].Artist, new Vector2(1220, 600), Color.White, 0.2f, Justification.Bottom | Justification.Right);
                            // Metadata may not contain BPM info, if it's empty, check the first song
                            string bpm = "???";
                            if (songlist[currentSongIndex].BpmIndex.Count > 0)
                                bpm = songlist[currentSongIndex].BpmIndex.First().Value.ToString("F0");
                            else if (songlist[currentSongIndex].IsMetadataFile && songlist[currentSongIndex].BpmIndex.Count == 0 && songlist[currentSongIndex].ChildMetadata.Count > 0)
                                bpm = songlist[currentSongIndex].ChildMetadata.First().BpmIndex.First().Value.ToString("F0");
                            spriteBatch.DrawStringJustify(Globals.Font["Franklin"], bpm + " BPM", new Vector2(1220, 640), Color.White, 0.18f, Justification.Bottom | Justification.Right);
                            spriteBatch.DrawStringJustify(Globals.Font["Franklin"], "Choreo: " + songlist[currentSongIndex].Designer, new Vector2(1220, 680), Color.White, 0.14f, Justification.Bottom | Justification.Right);
                        }

                        if (Globals.IsAutoModeEnabled)
                            spriteBatch.DrawStringJustify(Globals.Font["Franklin"], "AUTO MODE ENABLED", new Vector2(Width - 10, 10), Color.White, 0.1f, Justification.Top | Justification.Right);
                    }
                    else
                        spriteBatch.DrawStringJustify(Globals.Font["RunningStart"], "NO SONGS FOUND", new Vector2(Width / 2, Height / 2), Color.White, 0.5f, Justification.Center | Justification.Middle);

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
                            touchPt.Value.Draw(view, projection);

                        foreach (var motion in motions)
                            motion.Draw(currentBeat, view, projection);

                        if (enableProfiling)
                            log.AddEvent(stopwatch.ElapsedMilliseconds, "Motions Drawn");

                        var holdStart = holds.Count() - 1;
                        for (int i = 0; i < holds.Count(); i++)
                            holds.ElementAt(i).Draw(currentBeat, view, projection, holdStart - i);
                            //holds.ElementAt(i).Draw(currentBeat, view, projection);

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
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], "ACCURACY", new Vector2(120, yTopRow), Color.White, 0.08f, Justification.Left);
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], Enum.GetName(typeof(Difficulty), currentSongNotes.Metadata.Difficulty).ToUpper(), new Vector2(1200, yTopRow), Color.White, 0.08f, Justification.Center);
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], currentSongNotes.Metadata.Title, new Vector2(1150, yTopRow), Color.White, 0.2f, Justification.Right);
                    float yBottomRow = 685f;
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], Globals.SpeedScale.ToString("F1"), new Vector2(50, yBottomRow), Color.White, 0.25f, Justification.Center | Justification.Bottom);
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], (currentSongNotes.CurrentScore / currentSongNotes.TotalNotes * 100.0).ToString("000.000"), new Vector2(120, yBottomRow), Color.White, 0.25f, Justification.Left | Justification.Bottom);
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], "/ 100.000%", new Vector2(335, yBottomRow), Color.White, 0.18f, Justification.Left | Justification.Bottom);
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], currentSongNotes.Metadata.Artist, new Vector2(1150, yBottomRow), Color.White, 0.1f, Justification.Right | Justification.Bottom);
                    spriteBatch.DrawStringJustify(Globals.Font["Franklin"], currentSongNotes.Metadata.Level.ToString("D2"), new Vector2(1200, yBottomRow), Color.White, 0.25f, Justification.Center | Justification.Bottom);

                    if (Globals.IsAutoModeEnabled)
                        spriteBatch.DrawStringJustify(Globals.Font["Franklin"], "AUTO MODE ENABLED", new Vector2(Width - 10, 10), Color.White, 0.1f, Justification.Top | Justification.Right);

                    spriteBatch.End();

                    // Draw hit stats
                    if(true)
                    {
                        var numStepHit = currentSongNotes.Steps.Where(x => x.HitResult.WasHit && x.HitResult.Difference != Timing.MissFlag).Count();
                        var numMotionHit = currentSongNotes.Motions.Where(x => x.HitResult.WasHit && x.HitResult.Difference != Timing.MissFlag).Count();

                        spriteBatch.Begin();
                        spriteBatch.DrawString(debugFont, "Steps: " + numStepHit + " / " + currentSongNotes.Steps.Count, new Vector2(1100, 300), Color.White);
                        spriteBatch.DrawString(debugFont, "Motions: " + numMotionHit + " / " + currentSongNotes.Motions.Count, new Vector2(1100, 320), Color.White);
                        spriteBatch.DrawString(debugFont, "Combo: " + currentSongNotes.CurrentCombo, new Vector2(1100, 340), Color.White);
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
