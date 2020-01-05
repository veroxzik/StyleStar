using Nett;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public class ConfigFile
    {
        public string FilePath { get; set; } = Defines.ConfigFile;

        private Dictionary<string, object> configTable;

        public ConfigFile(string filepath = "")
        {
            if (!String.IsNullOrEmpty(filepath))
                FilePath = filepath;
        }

        public bool Load(string filepath = "")
        {
            if (!String.IsNullOrEmpty(filepath))
                FilePath = filepath;

            // Load config file
            if (File.Exists(Defines.ConfigFile))
            {
                configTable = Toml.ReadFile(Defines.ConfigFile).ToDictionary();

                if (configTable.ContainsKey(Defines.KeyConfig))
                    InputMonitor.SetKeys((Dictionary<string, object>)configTable[Defines.KeyConfig]);
                if (configTable.ContainsKey(Defines.TouchConfig))
                    TouchSettings.SetConfig((Dictionary<string, object>)configTable[Defines.TouchConfig]);
                if (configTable.ContainsKey(Defines.GameConfig))
                    GameSettingsScreen.SetConfig((Dictionary<string, object>)configTable[Defines.GameConfig]);
            }
            else 
            {
                // Build defaults

            }
            return true;
        }

        public void Update()
        {
            configTable = new Dictionary<string, object>()
            {
                {Defines.KeyConfig, InputMonitor.GetConfig() },
                {Defines.TouchConfig, TouchSettings.GetConfig() },
                {Defines.GameConfig, GameSettingsScreen.GetConfig() }
            };
        }

        public void Save(string filepath = "")
        {
            if (!String.IsNullOrEmpty(filepath))
                FilePath = filepath;

            var data = new Dictionary<string, object>()
            {
                {Defines.KeyConfig, InputMonitor.GetConfig() },
                {Defines.TouchConfig, TouchSettings.GetConfig() },
                {Defines.GameConfig, GameSettingsScreen.GetConfig() }
            };
            Toml.WriteFile(data, Defines.ConfigFile);

            // Should this go here? I don't see why we would save if we didn't want to apply it immediately
            UpdateGlobals();
        }

        public Dictionary<string, object> this[string key]
        {
            get { return (Dictionary<string, object>)configTable[key]; }
            set { configTable[key] = value; }
        }

        public object GetKey(string key)
        {
            foreach (var dict in configTable)
            {
                if (((Dictionary<string, object>)dict.Value).ContainsKey(key))
                    return ((Dictionary<string, object>)dict.Value)[key];
            }
            return null;
        }

        public void UpdateGlobals()
        {
            int mode = Convert.ToInt32(GetKey(GameSettingsScreen.ConfigKeys.AutoMode));
            Globals.AutoMode = (GameSettingsScreen.AutoMode)mode;
        }

        public void ResetGameSettings()
        {
            configTable = Toml.ReadFile(Defines.ConfigFile).ToDictionary();
            if (configTable.ContainsKey(Defines.GameConfig))
                GameSettingsScreen.SetConfig((Dictionary<string, object>)configTable[Defines.GameConfig]);
        }
    }
}
