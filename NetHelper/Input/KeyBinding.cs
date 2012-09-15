using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Input;

namespace Helper.Input
{
    public class KeyBinding
    {
        public Keys Key { get; set; }
        public KeyEvent KeyEvent { get; set; }

        [XmlIgnore]
        public KeyBindingDelegate Callback { get; set; }
        public string Alias { get; set; }
        public bool Ctrl { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }

        public KeyBinding()
        {
        }

        public KeyBinding(string alias, Keys k, bool ctrl, bool shift, bool alt, KeyEvent kevent)
        {
            Alias = alias;
            Key = k;
            Ctrl = ctrl;
            Shift = shift;
            Alt = alt;
            KeyEvent = kevent;
        }

        public KeyBinding(string alias, Keys k, bool ctrl, bool shift, bool alt, KeyEvent kevent, KeyBindingDelegate kdel)
            : this(alias, k, ctrl, shift, alt, kevent)
        {
            Callback = kdel;
        }

        private void CallDelegate()
        {
            if (Callback == null)
                return;
            Callback();
        }

        public void Check(KeyboardState last, KeyboardState curr)
        {
            if (last == null)
                return;
            if (!matchMods(curr))
                return;
            switch (KeyEvent)
            {
                case KeyEvent.Released:
                    if (!(keyDown(Key, last) && keyUp(Key, curr)))
                        return;
                    break;
                case KeyEvent.Pressed:
                    if (!(keyUp(Key, last) && keyDown(Key, curr)))
                        return;
                    break;
                case KeyEvent.Down:
                    if (!(keyDown(Key, curr)))
                        return;
                    break;
                case KeyEvent.Up:
                    if (!(keyUp(Key, curr)))
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
            if (Ctrl)
                if (!(curr.IsKeyDown(Keys.LeftControl) || curr.IsKeyDown(Keys.RightControl)))
                    return false;

            if (Shift)
                if (!(curr.IsKeyDown(Keys.LeftShift) || curr.IsKeyDown(Keys.RightShift)))
                    return false;

            if (Alt)
                if (!(curr.IsKeyDown(Keys.LeftAlt) || curr.IsKeyDown(Keys.RightAlt)))
                    return false;

            // we made it!
            return true;
        }
        public bool keyDown(Keys key, KeyboardState ks)
        {
            return ks.IsKeyDown(key);
        }
        public bool keyUp(Keys key, KeyboardState ks)
        {
            return ks.IsKeyUp(key);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Ctrl)
                sb.Append("Ctrl + ");
            if (Shift)
                sb.Append("Shift + ");
            if (Alt)
                sb.Append("Alt + ");
            sb.Append(Key.ToString());
            return sb.ToString();
        }
    }
}
