using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StyleStar
{
    public static class InputMonitor
    {

        public static Dictionary<Inputs, KeyMonitor> Monitors = new Dictionary<Inputs, KeyMonitor>()
        {
            {Inputs.Left, new KeyMonitor(Keys.Left) },
            {Inputs.Right, new KeyMonitor(Keys.Right) },
            {Inputs.Up, new KeyMonitor(Keys.Up) },
            {Inputs.Down, new KeyMonitor(Keys.Down) },
            {Inputs.Exit, new KeyMonitor(Keys.F4) },
            {Inputs.Back, new KeyMonitor(Keys.Escape) },
            {Inputs.Back2, new KeyMonitor(Keys.Back) },
            {Inputs.Auto, new KeyMonitor(Keys.F2) },
            {Inputs.Select, new KeyMonitor(Keys.Enter) }
        };

        public static void Update(GameTime time)
        {
            var currentState = Keyboard.GetState();
            foreach (var monitor in Monitors)
                monitor.Value.Update(currentState, time);
        }

        public static void SetKeys(Dictionary<string, object> config)
        {
            foreach (var item in config)
            {
                var newKey = (Keys)Convert.ToInt32(item.Value);
                Inputs input;
                if(Enum.TryParse(item.Key, out input))
                    Monitors[input].Key = newKey;
            }
        }

        public static Dictionary<string, object> GetConfig()
        {
            var output = new Dictionary<string, object>();
            foreach (var monitor in Monitors)
            {
                output.Add(Enum.GetName(typeof(Inputs), monitor.Key), (int)monitor.Value.Key);
            }
            return output;
        }
    }

    public class KeyMonitor
    {
        public Keys Key { get; set; }
        public KeyState State { get; private set; }

        private KeyboardState lastState;

        public KeyMonitor(Keys key)
        {
            Key = key;
        }

        public void Update(KeyboardState state, GameTime time)
        {
            if (state.IsKeyDown(Key) && !lastState.IsKeyDown(Key))
                State = KeyState.Press;
            else if (state.IsKeyUp(Key) && lastState.IsKeyDown(Key))
                State = KeyState.Release;
            else
                State = KeyState.NotHeld;

            lastState = state;
        }
    }

    public enum KeyState
    {
        NotHeld,
        Press,
        Release,
        HeldLong
    }

    public enum Inputs
    {
        Left,
        Right,
        Up,
        Down,
        Exit,
        Back,
        Back2,
        Auto,
        Select
    }
}
