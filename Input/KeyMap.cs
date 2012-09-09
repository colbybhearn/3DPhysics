using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.IO;

namespace Input
{
    /// <summary>
    /// Stores a set of keys, mapped to a set of possible bindings
    /// </summary>
    [Serializable]
    public class KeyMap
    {
        /*
         * Keymap Has
         *  - [DONE]A game that it is tied to
         *  - [DONE]A list of bindings
         *  
         * A binding has
         *  - [DONE]an alias
         *  - a default setting (key + modifier)
         *  - [DONE]a keyEvent type (Down, Up, PressDown, PressUp)
         *  
         * [DONE]Static methods to handle Reading/Writing KeyMaps
         */
        public String Game { get; set; }
        public List<KeyBinding> KeyBindings { get; set; }

        public KeyMap(String game, List<KeyBinding> defaultBindings)
        {
            this.Game = game;
            KeyBindings = defaultBindings;
        }

        public void Save()
        {
            KeyMap.SaveKeyMap(this);
        }


        //Todo
        // add a form for showing, editing bindings.
        // add a class to read and write a keymap file, per user and per game name.
        // ^ can be handled via static methods in KeyMap

        public class KeyBinding
        {
            public Keys Keys { get; set; }
            public Input.KeyWatch.keyEvent KeyEvent { get; set; }
            public String Alias { get; set; }
            public bool Ctrl { get; set; }
            public bool Shift { get; set; }
            public bool Alt { get; set; }
        }


        public static void SaveKeyMap(KeyMap km)
        {
            XmlSerializer x = new XmlSerializer(typeof(KeyMap));
            StreamWriter stm = null;
            try
            {
                stm = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CnJ Xna Physics\\KeyBindings\\" + km.Game + ".xml");
                x.Serialize(stm, km);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in SaveKeyMap " + e.Message);
            }
            finally
            {
                if(stm != null)
                    stm.Close();
            }
        }

        public static KeyMap LoadKeyMap(String game)
        {
            XmlSerializer x = new XmlSerializer(typeof(KeyMap));
            KeyMap km = null;
            StreamReader stm = null;
            try
            {
                stm = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CnJ Xna Physics\\KeyBindings\\" + km.Game + ".xml");
                km = (KeyMap)x.Deserialize(stm);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in LoadKeyMap " + e.Message);
            }
            finally
            {
                if (stm != null)
                    stm.Close();
            }

            return km;
        }

        public static List<KeyWatch> LoadKeyWatches(KeyMap km, SortedList<String, Input.KeyWatch.myCallbackDelegate> map)
        {
            List<KeyWatch> watches = new List<KeyWatch>();
            foreach (KeyBinding kb in km.KeyBindings)
            {
                Input.KeyWatch.myCallbackDelegate d;
                bool success = map.TryGetValue(kb.Alias, out d);
                if (success)
                    watches.Add(new KeyWatch(kb.Keys, kb.Ctrl, kb.Shift, kb.Alt, kb.KeyEvent, d));
                else
                    System.Diagnostics.Debug.WriteLine("Error loading keybinding delegate for " + kb.Alias);
            }

            return watches;
        }
    }
}
