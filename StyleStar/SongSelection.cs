using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StyleStar
{
    public static class SongSelection
    {
        public static List<SongMetadata> Songlist { get; set; } = new List<SongMetadata>();
        public static List<FolderParams> FolderParams { get; set; } = new List<FolderParams>();

        private static int currentSongIndex = 0;
        private static int currentFolderIndex = 0;
        private static int currentLevelIndex = 0;
        private static int selectedFolderIndex = -1;
        private static int selectedLevelIndex = -1;
        private static int currentSongLevelIndex = 0;  // Used to track difficulty switches

        public static void ImportSongs(string songsFolder)
        {
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
                        Songlist.Add(new SongMetadata(file.FullName));
                    }
                }
                else
                {
                    var charts = files.Where(f => f.FullName.EndsWith(Defines.ChartExtension));
                    if (charts != null && charts.Count() > 0)
                    {
                        foreach (var chart in charts)
                            Songlist.Add(new SongMetadata(chart.FullName));
                    }
                }
            }
            FolderParams.Add(new FolderParams() { Type = SortType.Title, Name = "SORT BY\nTITLE" });
            FolderParams.Add(new FolderParams() { Type = SortType.Artist, Name = "SORT BY\nARTIST" });
            FolderParams.Add(new FolderParams() { Type = SortType.Level, Name = "SORT BY\nLEVEL" });
        }

        public static void Draw(SpriteBatch sb)
        {
            string titleFont = "", artistFont = "";

            sb.Begin();
            if (Songlist.Count > 0)
            {
                sb.Draw(Globals.Textures["SsBgLine"], Globals.Origin, Songlist[currentSongIndex].ColorAccent.IfNull(ThemeColors.Blue));
                if (selectedFolderIndex == -1)
                {
                    sb.Draw(Globals.Textures["SsActive"], Globals.Origin, Color.White);
                    for (int i = 0; i < FolderParams.Count; i++)
                    {
                        var cardOffset = Globals.ItemOrigin + (i - currentFolderIndex) * Globals.ItemOffset;

                        sb.Draw(Globals.Textures["SsItemBg"], cardOffset, ThemeColors.GetColor(i));
                        sb.Draw(Globals.Textures["SsAccentStar"], cardOffset, ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f));
                        sb.DrawString(Globals.Font["Franklin"], FolderParams[i].Name, new Rectangle((int)cardOffset.X + 120, (int)cardOffset.Y + 16, 225, 88), Color.White);
                        sb.Draw(Globals.Textures["SsFrame"], cardOffset, Color.White);
                    }

                    sb.Draw(Globals.Textures["SsFolderSelect"], Globals.ItemOrigin + new Vector2(480f, 28f), Color.White);
                }
                else if (FolderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                {
                    sb.Draw(Globals.Textures["SsActive"], Globals.Origin, Color.White);
                    for (int i = 0; i < 10; i++)
                    {
                        var cardOffset = Globals.ItemOrigin + (i - currentLevelIndex) * Globals.ItemOffset;

                        sb.Draw(Globals.Textures["SsItemBg"], cardOffset, ThemeColors.GetColor(i));
                        sb.Draw(Globals.Textures["SsAccentStar"], cardOffset, ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f));
                        sb.DrawString(Globals.Font["Franklin"], "LEVEL" + (i + 1), new Rectangle((int)cardOffset.X + 120, (int)cardOffset.Y + 16, 225, 88), Color.White);
                        sb.Draw(Globals.Textures["SsFrame"], cardOffset, Color.White);
                    }

                    sb.Draw(Globals.Textures["SsGoBack"], Globals.ItemOrigin + new Vector2(-40f, -70f), Color.White);
                    sb.Draw(Globals.Textures["SsFolderSelect"], Globals.ItemOrigin + new Vector2(480f, 28f), Color.White);
                }
                else
                {
                    sb.Draw(Globals.Textures["SsActive"], Globals.Origin, Songlist[currentSongIndex].ColorFore.IfNull(Color.White));
                    for (int i = 0; i < Songlist.Count; i++)
                    {
                        var cardOffset = Globals.ItemOrigin + (i - currentSongIndex) * Globals.ItemOffset;

                        sb.Draw(Globals.Textures["SsItemBg"], cardOffset, Songlist[i].ColorBack.IfNull(ThemeColors.GetColor(i)));
                        sb.Draw(Globals.Textures["SsAccentStar"], cardOffset, Songlist[i].ColorFore.IfNull(ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f)));
                        sb.Draw(Globals.Textures["SsAccentAlbum"], cardOffset, Songlist[i].ColorFore.IfNull(ThemeColors.GetColor(i).LerpBlackAlpha(0.3f, 0.1f)));
                        sb.Draw(Songlist[i].AlbumImage, new Rectangle((int)cardOffset.X + 284, (int)cardOffset.Y + 12, 96, 96), Color.White);
                        sb.Draw(Globals.Textures["SsAlbumFrame"], cardOffset, Color.White);
                        if (Songlist[i].TitleImage == null)
                        {
                            titleFont = FontTools.ContainsJP(Songlist[i].Title) ? "JP" : "Franklin";
                            sb.DrawString(Globals.Font[titleFont], Songlist[i].Title, new Rectangle((int)cardOffset.X + 70, (int)cardOffset.Y + 16, 200, 38), Color.White);
                        }
                        else
                        {
                            sb.Draw(Songlist[i].TitleImage, new Vector2(cardOffset.X + 60, cardOffset.Y + 16), null, Color.White, 0.0f, Vector2.Zero, 2.0f / 3.0f, SpriteEffects.None, 0.0f);
                        }
                        if (Songlist[i].ArtistImage == null)
                        {
                            artistFont = FontTools.ContainsJP(Songlist[i].Artist) ? "JP" : "Franklin";
                            sb.DrawString(Globals.Font[artistFont], Songlist[i].Artist, new Rectangle((int)cardOffset.X + 108, (int)cardOffset.Y + 62, 160, 36), Color.White);
                        }
                        else
                        {
                            sb.Draw(Songlist[i].ArtistImage, new Vector2(cardOffset.X + 50, cardOffset.Y + 84), null, Color.White, 0.0f, Vector2.Zero, 2.0f / 3.0f, SpriteEffects.None, 0.0f);
                        }
                        sb.Draw(Globals.Textures["SsFrame"], cardOffset, Color.White);
                        if (i == currentSongIndex)
                        {
                            int[] difficulties = new int[3];
                            int x = 423;
                            sb.Draw(Globals.Textures["SsActiveDifficulty" + currentSongLevelIndex], new Vector2(cardOffset.X + 390, cardOffset.Y + 2), Color.White);
                            if (Songlist[i].IsMetadataFile)
                            {
                                foreach (var song in Songlist[i].ChildMetadata)
                                    difficulties[(int)song.Difficulty] = song.Level;
                            }
                            else
                                difficulties[(int)Songlist[i].Difficulty] = Songlist[i].Level;

                            switch (currentSongLevelIndex)
                            {
                                case 0:
                                    if (difficulties[0] > 0)
                                        sb.DrawString(Globals.Font["Franklin"], difficulties[0].ToString("D2"), new Rectangle((int)cardOffset.X + x - 4, (int)cardOffset.Y + 17 + (39 * 0), 36, 32), Color.Black);
                                    if (difficulties[1] > 0)
                                        sb.DrawString(Globals.Font["Franklin"], difficulties[1].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 13 + (39 * 1), 20, 39), Color.Black);
                                    if (difficulties[2] > 0)
                                        sb.DrawString(Globals.Font["Franklin"], difficulties[2].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 2 + (39 * 2), 20, 39), Color.Black);
                                    break;
                                case 1:
                                    if (difficulties[0] > 0)
                                        sb.DrawString(Globals.Font["Franklin"], difficulties[0].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 1 + (39 * 0), 20, 39), Color.Black);
                                    if (difficulties[1] > 0)
                                        sb.DrawString(Globals.Font["Franklin"], difficulties[1].ToString("D2"), new Rectangle((int)cardOffset.X + x - 4, (int)cardOffset.Y + 7 + (39 * 1), 36, 32), Color.Black);
                                    if (difficulties[2] > 0)
                                        sb.DrawString(Globals.Font["Franklin"], difficulties[2].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 2 + (39 * 2), 20, 39), Color.Black);
                                    break;
                                case 2:
                                    if (difficulties[0] > 0)
                                        sb.DrawString(Globals.Font["Franklin"], difficulties[0].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y + 2 + (39 * 0), 20, 39), Color.Black);
                                    if (difficulties[1] > 0)
                                        sb.DrawString(Globals.Font["Franklin"], difficulties[1].ToString("D2"), new Rectangle((int)cardOffset.X + x, (int)cardOffset.Y - 8 + (39 * 1), 20, 39), Color.Black);
                                    if (difficulties[2] > 0)
                                        sb.DrawString(Globals.Font["Franklin"], difficulties[2].ToString("D2"), new Rectangle((int)cardOffset.X + x - 4, (int)cardOffset.Y - 7 + (39 * 2), 36, 32), Color.Black);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            sb.Draw(Globals.Textures["SsDifficultyBg"], cardOffset, Color.White);
                            if (Songlist[i].IsMetadataFile)
                            {
                                foreach (var song in Songlist[i].ChildMetadata)
                                {
                                    sb.DrawString(Globals.Font["Franklin"], song.Level.ToString("D2"), new Rectangle((int)cardOffset.X + 392, (int)cardOffset.Y + 2 + (39 * (int)song.Difficulty), 20, 39), Color.Black);
                                }
                            }
                            else
                                sb.DrawString(Globals.Font["Franklin"], Songlist[i].Level.ToString("D2"), new Rectangle((int)cardOffset.X + 392, (int)cardOffset.Y + 2 + (39 * (int)Songlist[i].Difficulty), 20, 39), Color.Black);
                        }

                    }

                    sb.Draw(Globals.Textures["SsGoBack"], Globals.ItemOrigin + new Vector2(-40f, -70f), Color.White);
                    sb.Draw(Globals.Textures["SsSongSelect"], Globals.ItemOrigin + new Vector2(480f, 28f), Color.White);

                    // Metadata may not contain BPM info, if it's empty, check the first song
                    string bpm = "???";
                    if (Songlist[currentSongIndex].BpmIndex.Count > 0)
                        bpm = Songlist[currentSongIndex].BpmIndex.First().Value.ToString("F0");
                    else if (Songlist[currentSongIndex].IsMetadataFile && Songlist[currentSongIndex].BpmIndex.Count == 0 && Songlist[currentSongIndex].ChildMetadata.Count > 0)
                        bpm = Songlist[currentSongIndex].ChildMetadata.First().BpmIndex.First().Value.ToString("F0");

                    titleFont = FontTools.ContainsJP(Songlist[currentSongIndex].Title) ? "JP" : "Franklin";
                    artistFont = FontTools.ContainsJP(Songlist[currentSongIndex].Artist) ? "JP" : "Franklin";

                    sb.DrawStringFixedHeight(Globals.Font[titleFont], Songlist[currentSongIndex].Title, new Vector2(1220, 570), Color.White, 40.0f, Justification.Bottom | Justification.Right);
                    sb.DrawStringFixedHeight(Globals.Font[artistFont], Songlist[currentSongIndex].Artist, new Vector2(1220, 610), Color.White, 30.0f, Justification.Bottom | Justification.Right);
                    sb.DrawStringFixedHeight(Globals.Font["Franklin"], bpm + " BPM", new Vector2(1220, 640), Color.White, 20.0f, Justification.Right | Justification.Bottom);
                    sb.DrawStringFixedHeight(Globals.Font["Franklin"], "Choreo: " + Songlist[currentSongIndex].Designer, new Vector2(1220, 670), Color.White, 20.0f, Justification.Right | Justification.Bottom);
                }

                if (Globals.IsAutoModeEnabled)
                    sb.DrawStringJustify(Globals.Font["Franklin"], "AUTO MODE ENABLED", new Vector2(Globals.WindowSize.X - 10, 10), Color.White, 0.1f, Justification.Top | Justification.Right);
            }
            else
                sb.DrawStringJustify(Globals.Font["RunningStart"], "NO SONGS FOUND", new Vector2(Globals.WindowSize.X / 2, Globals.WindowSize.Y / 2), Color.White, 0.5f, Justification.Center | Justification.Middle);

            sb.End();
        }

        public static void ScrollDown()
        {
            if (selectedFolderIndex == -1)
                currentFolderIndex = currentFolderIndex < (FolderParams.Count - 1) ? ++currentFolderIndex : FolderParams.Count - 1;
            else if (FolderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                currentLevelIndex = currentLevelIndex < 9 ? ++currentLevelIndex : 9;
            else
                currentSongIndex = currentSongIndex < (Songlist.Count - 1) ? ++currentSongIndex : Songlist.Count - 1;
        }

        public static void ScrollUp()
        {
            if (selectedFolderIndex == -1)
                currentFolderIndex = currentFolderIndex > 0 ? --currentFolderIndex : 0;
            else if (FolderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
                currentLevelIndex = currentLevelIndex > 0 ? --currentLevelIndex : 0;
            else
                currentSongIndex = currentSongIndex > 0 ? --currentSongIndex : 0;
        }

        public static void GoBack()
        {
            if (selectedFolderIndex != -1 && FolderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex != -1)
            {
                selectedLevelIndex = -1;
            }
            else
            {
                selectedFolderIndex = -1;
                currentSongIndex = 0;
            }
        }

        public static bool Select()
        {
            if (selectedFolderIndex == -1)
            {
                selectedFolderIndex = currentFolderIndex;
                switch (FolderParams[selectedFolderIndex].Type)
                {
                    case SortType.Title:
                        Songlist = Songlist.OrderBy(x => x.Title).ToList();
                        break;
                    case SortType.Artist:
                        Songlist = Songlist.OrderBy(x => x.Artist).ToList();
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
            else if (FolderParams[selectedFolderIndex].Type == SortType.Level && selectedLevelIndex == -1)
            {
                selectedLevelIndex = currentLevelIndex;
                Songlist = Songlist.OrderBy(x => x.Level).ToList();
                currentSongIndex = Songlist.FindLastIndex(x => x.Level < (selectedLevelIndex + 1)) + 1;
            }
            else
            {
                if ((Songlist[currentSongIndex].IsMetadataFile && Songlist[currentSongIndex].ChildMetadata.FirstOrDefault(x => (int)x.Difficulty == currentSongLevelIndex) == null) ||
                    (!Songlist[currentSongIndex].IsMetadataFile && (int)Songlist[currentSongIndex].Difficulty != currentSongLevelIndex))
                    return false;

                return true;
            }

            return false;
        }

        public static void CycleDifficulty()
        {
            currentSongLevelIndex++;
            if (currentSongLevelIndex > 2)
                currentSongLevelIndex = 0;
        }

        public static SongMetadata GetCurrentSongMeta(bool getChild = true)
        {
            if(getChild)
                return Songlist[currentSongIndex].IsMetadataFile ? Songlist[currentSongIndex].ChildMetadata.FirstOrDefault(x => (int)x.Difficulty == currentSongLevelIndex) : Songlist[currentSongIndex];
            else
                return Songlist[currentSongIndex];
        }
    }

    public class FolderParams
    {
        public SortType Type;
        public int Value;
        public string Category;
        public string Name;
    }

    public enum SortType
    {
        Title,
        Artist,
        Level,
        Genre
    }
}
