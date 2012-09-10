using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using System.ComponentModel;
using System.Windows.Forms;


//global :: using Microsoft.Xna.Framework.Input;
//global::using Microsoft.Xna.Framework.Input.Keys;
namespace Input
{
    
    
    public class InputManager
    {
        
        
        /* Vision:
         * 
         * KeyMap should be an end-to-end map of possible actions in terms of (alias to key to delegate)
         * the alias, 
         * the key,
         * the callback
         * 
         * Defaults are handled by each specific game creating a default hard-coded keymap
         * hand the default keymap to the input manager.
         * The input manager will attempt to load a saved keymap according to the game name.
         * Any loaded key keybindings will be used to overwrite the default keybindings
         * 
         * Watchlist can be created from the keyMaps' final keybindings.
         * KeyMap can have a Check method that does what InputManagers' Update method does
         * 
         */

        bool SetupMode = false;

        Microsoft.Xna.Framework.Input.Keys[] lastPressed;
        KeyboardState currentState = new KeyboardState();
        List<KeyWatch> watches = new List<KeyWatch>();

        public InputManager()
        {
        }

        public void Update()
        {
            
            currentState = Keyboard.GetState();

            if (SetupMode)
            {
                frmSettings.ProcessKey(currentState);                
            }
            else
            {

                foreach (KeyWatch kw in watches)
                    kw.Check(lastPressed, currentState);

                lastPressed = currentState.GetPressedKeys();
            }
        }

        public void AddWatch(KeyWatch watch)
        {
            watches.Add(watch);
        }

        Settings frmSettings;
        public void EditSettings()
        {
            //HUGE BUG POTENTIAL here
            KeyMap keyMap = new KeyMap("whatever", new List<KeyMap.KeyBinding>());
            keyMap.KeyBindings.Add(new KeyMap.KeyBinding("CameraMoveForward", Microsoft.Xna.Framework.Input.Keys.W, false, false, false, Input.KeyWatch.keyEvent.Down));
            frmSettings = new Settings(keyMap);
            SetupMode = true;            
            DialogResult dr = frmSettings.ShowDialog();
            if (dr == DialogResult.OK)
            {
                keyMap = frmSettings.keyMap;
            }
            frmSettings.Dispose();
            SetupMode = false;
        }
    }

    public class KeyWatch
    {
        public enum keyEvent
        {
            [Description("While Not Pressed")]      Up, // happens to be up right now            
            [Description("While Pressed")]          Down, // happens to be down right now
            [Description("On Press")]           Pressed, // just pressed since last update
            [Description("On Release")]          Released, // just released since last update
        }
        public Microsoft.Xna.Framework.Input.Keys key;
        public keyEvent kEvent;
        public delegate void myCallbackDelegate();
        myCallbackDelegate dCallback;
        bool Ctrl;
        bool Shift;
        bool Alt;

        public KeyWatch(Microsoft.Xna.Framework.Input.Keys k, bool ctrl, bool shift, bool alt, keyEvent kevent, myCallbackDelegate callback)
        {
            key = k;
            Ctrl = ctrl;
            Shift = shift;
            Alt = alt;
            kEvent = kevent;
            dCallback = callback;
        }

        private void CallDelegate()
        {
            if (dCallback == null)
                return;
            dCallback();
        }

        public void Check(Microsoft.Xna.Framework.Input.Keys[] last, KeyboardState curr)
        {
            if (last == null)
                return;
            if (!matchMods(curr))
                return;
            switch (kEvent)
            {
                case KeyWatch.keyEvent.Released:
                    if (!(wasDown(key, last) && isUp(key, curr)))
                        return;                    
                    break;
                case KeyWatch.keyEvent.Pressed:
                    if (!(wasUp(key, last) && isDown(key, curr)))
                        return;
                    break;
                case KeyWatch.keyEvent.Down:
                    if (!(isDown(key, curr)))
                        return;
                    break;
                case KeyWatch.keyEvent.Up:
                    if (!(isUp(key, curr)))
                        return;
                    break;
                default:
                    return;
            }
            // we made it through! Do something freakin' awesome
            CallDelegate();
        }

        private bool matchMods(KeyboardState curr)
        {
            if(Ctrl)
                if (!(curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl)))
                    return false;

            if(Shift)
                if (!(curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift)))
                    return false;

            if (Alt)
                if (!(curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) || curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt)))
                    return false;
            
            // we made it!
            return true;
        }
        public bool isDown(Microsoft.Xna.Framework.Input.Keys key, KeyboardState curr)
        {
            return curr.IsKeyDown(key);
        }
        public bool isUp(Microsoft.Xna.Framework.Input.Keys key, KeyboardState curr)
        {
            return curr.IsKeyUp(key);
        }
        public bool wasDown(Microsoft.Xna.Framework.Input.Keys key, Microsoft.Xna.Framework.Input.Keys[] last)
        {
            return last.Contains(key);
        }
        public bool wasUp(Microsoft.Xna.Framework.Input.Keys key, Microsoft.Xna.Framework.Input.Keys[] last)
        {
            return !last.Contains(key);
        }

    }
}
