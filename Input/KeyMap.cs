using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;

namespace Input
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
         * 
         * Todo
         *  add a form for showing, editing bindings.
         *  [DONE] add a class to read and write a keymap file, per user and per game name.
         */
        public string Game { get; set; }
        public List<KeyBinding> KeyBindings { get; set; }
        

        public KeyMap()
        {
        }

        public KeyMap(string game, List<KeyBinding> defaultBindings)
        {
            this.Game = game;
            KeyBindings = defaultBindings;
        }

        public KeyMap(KeyMap other)
        {
            Game = other.Game;
            KeyBindings = new List<KeyBinding>(other.KeyBindings);
        }

        public void Check(KeyboardState last, KeyboardState current)
        {
            foreach (KeyBinding kb in KeyBindings)
            {
                kb.Check(last, current);
            }
        }

        public static void SaveKeyMap(KeyMap km)
        {
            XmlSerializer x = new XmlSerializer(typeof(KeyMap));
            StreamWriter stm = null;
            try
            {
                string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CnJ Xna Physics\\KeyBindings\\" + km.Game + ".xml";
                if (!Directory.Exists(filepath))
                {
                    string dirpath = Path.GetDirectoryName(filepath);
                    Directory.CreateDirectory(dirpath);
                }
                stm = new StreamWriter(filepath);
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

        public static KeyMap LoadKeyMap(string game, KeyMap defaultKeyMap)
        {
            XmlSerializer x = new XmlSerializer(typeof(KeyMap));
            KeyMap loadedKeyMap = new KeyMap(defaultKeyMap);
            StreamReader stm = null;
            try
            {
                string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CnJ Xna Physics\\KeyBindings\\" + game + ".xml";
                stm = new StreamReader(filepath);

                KeyMap km = (KeyMap)x.Deserialize(stm);

                foreach (KeyBinding saved in km.KeyBindings)
                {
                    foreach (KeyBinding kb in loadedKeyMap.KeyBindings)
                    {
                        // We found a saved keybinding, use its settings instead
                        if (saved.Alias.Equals(kb.Alias))
                        {
                            kb.Alt = saved.Alt;
                            kb.Ctrl = saved.Ctrl;
                            kb.Key = saved.Key;
                            kb.KeyEvent = saved.KeyEvent;
                            kb.Shift = saved.Shift;
                        }
                    }
                }
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
            return loadedKeyMap;
        }
    }
}
