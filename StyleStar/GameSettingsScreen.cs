using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StyleStar
{
    public static class GameSettingsScreen
    {
        private static List<Label> fixedLabels = new List<Label>();
        private static List<SelectableLabel> selectableLabels = new List<SelectableLabel>();
        private static Label confirmLabel;
        private static Label rejectLabel;

        private static Vector2 titlePoint = new Vector2(372, 20);
        private static Vector2 titleOffset = new Vector2(-47, 81);
        private static Vector2 optionsPoint = new Vector2(390, 56);

        private static float fontTitleHeight = 25.0f;
        private static float fontOptionHeight = 18.0f;

        private static int selectedCategory = 0;
        private static int numCategories;

        private static bool confirmSelection = false;

        private static Language selectedLanguage = Language.English;

        public static void GenerateLabels()
        {
            fixedLabels.Add(new Label(Globals.Font["Franklin"], "LANGUAGE", titlePoint, Color.White, Justification.Top | Justification.Left, LabelType.FixedHeight, fontTitleHeight));
            fixedLabels.Add(new Label(Globals.Font["Franklin"], "SCREEN RESOLUTION", titlePoint + titleOffset, Color.White, Justification.Top | Justification.Left, LabelType.FixedHeight, fontTitleHeight));
            fixedLabels.Add(new Label(Globals.Font["Franklin"], "TOUCH SCREEN ORIENTATION", titlePoint + 2 * titleOffset, Color.White, Justification.Top | Justification.Left, LabelType.FixedHeight, fontTitleHeight));
            fixedLabels.Add(new Label(Globals.Font["Franklin"], "ENABLE FREE PLAY", titlePoint + 3 * titleOffset, Color.White, Justification.Top | Justification.Left, LabelType.FixedHeight, fontTitleHeight));
            fixedLabels.Add(new Label(Globals.Font["Franklin"], "AUTO MODE", titlePoint + 4 * titleOffset, Color.White, Justification.Top | Justification.Left, LabelType.FixedHeight, fontTitleHeight));

            selectableLabels.Add(new SelectableLabel(Globals.Font["Franklin"], new string[] { "English", "Open language select..." }, optionsPoint, Justification.Top | Justification.Left, LabelType.FixedHeight, fontOptionHeight));
            selectableLabels.Add(new SelectableLabel(Globals.Font["Franklin"], new string[] {"640x360", "1280x720", "1920x1080" }, optionsPoint + titleOffset, Justification.Top | Justification.Left, LabelType.FixedHeight, fontOptionHeight));
            selectableLabels.Add(new SelectableLabel(Globals.Font["Franklin"], new string[] { "Horizontal (0°)", "Vertical (90°)", "Horizontal (180°)", "Vertical (270°)" }, optionsPoint + 2 * titleOffset, Justification.Top | Justification.Left, LabelType.FixedHeight, fontOptionHeight));
            selectableLabels.Add(new SelectableLabel(Globals.Font["Franklin"], new string[] { "ON", "OFF" }, optionsPoint + 3 * titleOffset, Justification.Top | Justification.Left, LabelType.FixedHeight, fontOptionHeight));
            selectableLabels.Add(new SelectableLabel(Globals.Font["Franklin"], new string[] { "OFF", "ON", "DOWN ONLY" }, optionsPoint + 4 * titleOffset, Justification.Top | Justification.Left, LabelType.FixedHeight, fontOptionHeight));

            selectableLabels.Add(new SelectableLabel(Globals.Font["Franklin"], new string[] { "Save and Exit", "Leave without Saving" }, optionsPoint + 5 * titleOffset, Justification.Top | Justification.Left, LabelType.FixedHeight, fontOptionHeight));

            numCategories = selectableLabels.Count;

            confirmLabel = new Label(Globals.Font["Franklin"], "Press SELECT again to confirm changes.", new Vector2(Globals.WindowSize.X / 2, Globals.WindowSize.Y - 80), ThemeColors.Blue, Justification.Center | Justification.Middle, LabelType.FixedHeight, 30.0f);
            rejectLabel = new Label(Globals.Font["Franklin"], "Press SELECT again to discard changes.", new Vector2(Globals.WindowSize.X / 2, Globals.WindowSize.Y - 80), ThemeColors.Blue, Justification.Center | Justification.Middle, LabelType.FixedHeight, 30.0f);
        }

        public static void Draw(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            // Draw background colors
            sb.Draw(Globals.Textures["SettingsBg1"], Globals.Origin, ThemeColors.Pink);
            sb.Draw(Globals.Textures["SettingsBg2"], Globals.Origin, ThemeColors.Blue);
            sb.Draw(Globals.Textures["SettingsBg3"], Globals.Origin, ThemeColors.Yellow);

            // Draw active selection
            sb.Draw(Globals.Textures["SettingsSelection"], new Vector2(312, 11) + selectedCategory * titleOffset, Color.White);

            // Draw labels
            foreach (var label in fixedLabels)
            {
                label.Draw(sb);
            }

            // Draw selectable labels
            foreach (var label in selectableLabels)
            {
                label.Draw(sb);
            }

            if (confirmSelection)
            {
                if (selectableLabels[5].SelectedOption == 0)
                    confirmLabel.Draw(sb);
                else
                    rejectLabel.Draw(sb);
            }
                

            sb.End();
        }

        public static void ScrollUp()
        {
            if (confirmSelection)
                return;

            selectedCategory--;
            if (selectedCategory < 0)
                selectedCategory = numCategories - 1;
        }

        public static void ScrollDown()
        {
            if (confirmSelection)
                return;

            selectedCategory++;
            if (selectedCategory >= numCategories )
                selectedCategory = 0;
        }

        public static void ScrollRight()
        {
            if (confirmSelection)
                return;

            selectableLabels[selectedCategory].ScrollRight();
        }

        public static void ScrollLeft()
        {
            if (confirmSelection)
                return;

            selectableLabels[selectedCategory].ScrollLeft();
        }

        public static bool GoBack()
        {
            if (confirmSelection)
            {
                confirmSelection = false;
                return false;
            }
            else
            {
                confirmSelection = false;
                return true;
            }
        }

        public static DialogResult Select()
        {
            if (confirmSelection)
            {
                confirmSelection = false;
                // Config is saved in the main thread
                return selectableLabels[5].SelectedOption == 0 ? DialogResult.Confirm : DialogResult.Cancel;
            }
            else
                confirmSelection = true;

            return DialogResult.NoAction;
        }

        public static void SetConfig(Dictionary<string, object> config)
        {
            foreach (var entry in config)
            {
                switch (entry.Key)
                {
                    case ConfigKeys.Language:
                        selectedLanguage = (Language)Convert.ToInt32(entry.Value);
                        break;
                    case ConfigKeys.Resolution:
                        selectableLabels[1].SelectedOption = Convert.ToInt32(entry.Value);
                        break;
                    case ConfigKeys.TouchScreenOrientation:
                        selectableLabels[2].SelectedOption = Convert.ToInt32(entry.Value);
                        break;
                    case ConfigKeys.EnableFreePlay:
                        selectableLabels[3].SelectedOption = Convert.ToBoolean(entry.Value) ? 0 : 1;
                        break;
                    case ConfigKeys.AutoMode:
                        selectableLabels[4].SelectedOption = Convert.ToInt32(entry.Value);
                        break;
                    default:
                        break;
                }
            }
        }

        public static Dictionary<string, object> GetConfig()
        {
            return new Dictionary<string, object>()
            {
                {ConfigKeys.Language, (int)selectedLanguage},
                {ConfigKeys.Resolution,selectableLabels[1].SelectedOption },
                {ConfigKeys.TouchScreenOrientation, selectableLabels[2].SelectedOption },
                {ConfigKeys.EnableFreePlay, selectableLabels[3].SelectedOption == 0 ? true : false },
                {ConfigKeys.AutoMode, selectableLabels[4].SelectedOption }
            };
        }

        public enum LabelField
        {
            StylishCount,
            CoolCount,
            GoodCount,
            MissCount,
            Score,
            Result,
            Title,
            Artist,
            Level
        }

        public enum Language
        {
            English,
            Japanese
        }

        public enum Resolution
        {
            _640x360,
            _1280x720,
            _1920x1080
        }

        public enum AutoMode
        {
            Off,
            Auto,
            AutoDown
        }

        public static class ConfigKeys
        {
            public const string Language = "Language";
            public const string Resolution = "Resolution";
            public const string TouchScreenOrientation = "TouchScreenOrientation";
            public const string EnableFreePlay = "EnableFreePlay";
            public const string StageNumber = "StageNumber";
            public const string AutoMode = "AutoMode";
        }
    }
}
