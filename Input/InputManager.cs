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

        KeyboardState lastState = new KeyboardState();
        KeyboardState currentState = new KeyboardState();
        //List<KeyWatch> watches = new List<KeyWatch>();
        public KeyMap keyMap;
        public String game;

        public InputManager(String gameName, KeyMap defaultKeyMap)
        {
            game = gameName;
            keyMap = KeyMap.LoadKeyMap(game, defaultKeyMap);
            if (keyMap == null)
                keyMap = defaultKeyMap;
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
                keyMap.Check(lastState, currentState);
                lastState = currentState;
            }
        }
        
        public void Save()
        {
            KeyMap.SaveKeyMap(keyMap);
        }
        
        Settings frmSettings;
        public void EditSettings()
        {
            //HUGE BUG POTENTIAL here
            //KeyMap keyMap = new KeyMap("whatever",  

            //keyMap.KeyBindings.Add(new KeyBinding("CameraMoveForward", Microsoft.Xna.Framework.Input.Keys.W, false, false, false, Input.KeyWatch.keyEvent.Down));
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

        /*public void AddWatch(KeyWatch watch)
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
            watches.Add(watch);
        }*/

    
}
