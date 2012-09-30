using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Helper.Input
{
    public enum KeyEvent
    {
        [Description("While Not Pressed")]  Up, // happens to be up right now           
        [Description("While Pressed")]      Down, // happens to be down right now
        [Description("On Press")]           Pressed, // just pressed since last update
        [Description("On Release")]         Released, // just released since last update
    }

    public delegate void KeyBindingDelegate();

    /// <summary>
    /// Stores a set of keys, mapped to a set of possible bindings
    /// </summary>
    [DataContract]
    public class KeyMap
    {
        public bool Enabled;

        [DataMember]
        public string Alias;
        [DataMember]
        public SortedList<string, KeyBinding> KeyBindings { get; set; }

        public KeyMap()
        {
        }

        public KeyMap(string alias, List<KeyBinding> defaultBindings)
        {
            Alias = alias;
            KeyBindings = new SortedList<string, KeyBinding>();
            foreach (KeyBinding kb in defaultBindings)
                KeyBindings.Add(kb.Alias, kb);
        }

        public KeyMap(KeyMap other)
        {
            Alias = other.Alias;
            KeyBindings = new SortedList<string, KeyBinding>(other.KeyBindings);
        }

        public void Check(KeyboardState last, KeyboardState current)
        {
            // if this keymap is not enabled,
            if (!Enabled)
                // then don't bother checking
                return; 
            // for each binding,
            foreach (KeyBinding kb in KeyBindings.Values)
                // check if it is happening now
                kb.Check(last, current);
        }

        internal void LoadOverrides(KeyMap savedkm)
        {
            // for each saved preference,
            foreach (KeyBinding saved in savedkm.KeyBindings.Values)
            {
                // if the game can use that preference,
                if (this.KeyBindings.ContainsKey(saved.Alias))
                {
                    // get the default for that input, 
                    KeyBinding kb = KeyBindings[saved.Alias];
                    // and totally override it with the saved preference
                    kb.Alt = saved.Alt;
                    kb.Ctrl = saved.Ctrl;
                    kb.Key = saved.Key;
                    //kb.KeyEvent = saved.KeyEvent;
                    kb.Shift = saved.Shift;
                }
            }
        }
    }
}
