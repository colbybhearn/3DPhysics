using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using System.ComponentModel;

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

            keyMap.Check(lastState, currentState);

            /*foreach (KeyWatch kw in watches)
                kw.Check(lastPressed, currentState);*/

            lastState = currentState;
        }

        public void Save()
        {
            KeyMap.SaveKeyMap(keyMap);
        }

        /*public void AddWatch(KeyWatch watch)
        {
            watches.Add(watch);
        }*/
    }
}
