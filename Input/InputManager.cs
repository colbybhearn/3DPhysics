using System;
using System.Collections.Generic;
using System.Linq;
//using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;


//global :: using Microsoft.Xna.Framework.Input;
//global::using Microsoft.Xna.Framework.Input.Keys;
namespace Input
{
    public enum InputMode
    {
        Setup,
        Chat,
        Mapped
    }

    public delegate void ChatDelegate(List<Microsoft.Xna.Framework.Input.Keys> pressedKeys);

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

        public InputMode Mode { get; set; }

        KeyboardState lastState = new KeyboardState();
        KeyboardState currentState = new KeyboardState();
        public KeyMap keyMap;
        public SortedList<InputMode, Delegate> InputModeDelegates;
        public String game;
        Settings frmSettings;


        public InputManager(String gameName, KeyMap defaultKeyMap)
        {
            game = gameName;
            keyMap = KeyMap.LoadKeyMap(game, defaultKeyMap);
            Mode = InputMode.Mapped;
            InputModeDelegates = new SortedList<InputMode, Delegate>();
        }

        public void AddInputMode(InputMode m, Delegate d)
        {
            InputModeDelegates.Add(m, d);
        }

        public void Update()
        {            
            currentState = Keyboard.GetState();
            switch(Mode)
            {
                case InputMode.Setup:
                    frmSettings.ProcessKey(currentState);
                    break;
                case InputMode.Chat:
                    Delegate d;
                    if(InputModeDelegates.TryGetValue(Mode, out d))
                        ((ChatDelegate)d)(GetPressedKeysWithShift(lastState, currentState));
                    break;
                case InputMode.Mapped:
                    if(keyMap != null)
                        keyMap.Check(lastState, currentState);
                    break;
            }
        
            lastState = currentState;
        }

        //Will always contain shift if shift is held
        private List<Microsoft.Xna.Framework.Input.Keys> GetPressedKeysWithShift(KeyboardState lastState, KeyboardState currentState)
        {
            Keys[] last = lastState.GetPressedKeys();
            Keys[] current = currentState.GetPressedKeys();
            List<Keys> pressed = new List<Keys>();

            foreach(Keys k in current)
                if(last.Contains(k) == false || k.HasFlag(Keys.LeftShift) || k.HasFlag(Keys.RightShift))
                    pressed.Add(k);

            return pressed;
        }
        
        public void Save()
        {
            KeyMap.SaveKeyMap(keyMap);
        }
        
        public void EditSettings()
        {
            frmSettings = new Settings(keyMap);
            InputMode lastMode = Mode;
            Mode = InputMode.Setup;            
            System.Windows.Forms.DialogResult dr = frmSettings.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                keyMap = frmSettings.keyMap;
                Save();
            }
            frmSettings.Dispose();
            Mode = lastMode;
        }
    }
}
