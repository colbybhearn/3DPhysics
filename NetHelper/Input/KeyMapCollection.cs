using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Helper.Input
{
    // DataContract serialization allows IDictionary implementers to be serialized! WooHoo
    [DataContract]
    public class KeyMapCollection
    {
        [DataMember]
        public string Game;
        [DataMember]
        public SortedList<string, KeyMap> keyMaps = new SortedList<string, KeyMap>(); // this can't be serialized with XMLSerializer, so DataContract serialization has been used.

        private string FilePath
        {
            get
            {
                return GetSettingsPath(Game);
            }
        }

        public KeyMapCollection()
        {
        }

        public KeyMapCollection(KeyMapCollection other)
        {
            Game = other.Game;
            keyMaps = new SortedList<string, KeyMap>(other.keyMaps);
        }

        public KeyMapCollection(SortedList<string, KeyMap> maps)
        {
            keyMaps = maps;
        }

        /// <summary>
        /// Adds a keymap to the collection
        /// </summary>
        /// <param name="keyMap"></param>
        public void AddMap(KeyMap keyMap)
        {
            string alias = keyMap.Alias;
            if (keyMaps.ContainsKey(alias))
                throw new ArgumentException("A keymapgroup by the name " + alias + " already exists. Use a different name.");
            keyMaps.Add(alias, keyMap);
        }

        public void DisableAllKeyGroups()
        {
            foreach (KeyMap map in keyMaps.Values)
                map.Enabled = false;
        }

        public void EnableKeyGroups(string alias)
        {
            if (keyMaps.ContainsKey(alias))
                keyMaps[alias].Enabled = true;
        }

        public void DisableKeyGroups(string alias)
        {
            if (keyMaps.ContainsKey(alias))
                keyMaps[alias].Enabled = false;
        }

        public static void Save(KeyMapCollection kmc)
        {
            DataContractSerializer x = new DataContractSerializer(typeof(KeyMapCollection));
            StreamWriter stm = null;
            StringWriter sw = new StringWriter();
            XmlTextWriter tw = new XmlTextWriter(sw);
            try
            {
                string filepath = kmc.FilePath;                
                if (!Directory.Exists(filepath))
                {
                    string dirpath = Path.GetDirectoryName(filepath);
                    Directory.CreateDirectory(dirpath);
                }
                stm = new StreamWriter(filepath);
                tw.Formatting = Formatting.Indented; // Make it human readable!
                x.WriteObject(tw, kmc);
                tw.Flush();
                stm.Write(sw.ToString());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in SaveKeyMap " + e.StackTrace);
            }
            finally
            {
                if(tw!=null)
                    tw.Close();
                if (stm != null)
                    stm.Close();
            }
        }

        public static KeyMapCollection Load(string game, KeyMapCollection defaultKeyMap)
        {
            DataContractSerializer x = new DataContractSerializer(typeof(KeyMapCollection));
            KeyMapCollection finalCollection = new KeyMapCollection(defaultKeyMap);
            StreamReader stm = null;
            try
            {
                string filepath = KeyMapCollection.GetSettingsPath(game);
                stm = new StreamReader(filepath);

                KeyMapCollection savedCollection = (KeyMapCollection)x.ReadObject(stm.BaseStream);

                foreach (KeyMap finalkm in finalCollection.keyMaps.Values)
                    // if tha keymap by this name exists on disk
                    if (savedCollection.keyMaps.ContainsKey(finalkm.Alias))
                    {
                        KeyMap savedkm = savedCollection.keyMaps[finalkm.Alias];
                        // load those preferences, overriding the defaults
                        finalkm.LoadOverrides(savedkm);
                    }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in LoadKeyMap " + e.StackTrace);
            }
            finally
            {
                if (stm != null)
                    stm.Close();
            }
            return finalCollection;
        }

        /// <summary>
        /// Centralizes the settings path for optimum maintainability.
        /// Static for flexibility.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string GetSettingsPath(string game)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CnJ Xna Physics\\KeyBindings\\" + game + ".xml";
        }
    }
}
