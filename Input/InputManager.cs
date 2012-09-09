using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Timers;

namespace Input
{
    public class InputManager
    {
        Keys[] lastPressed;
        KeyboardState currentState = new KeyboardState();
        Timer tmr;
        List<KeyWatch> watches = new List<KeyWatch>();

        public InputManager(int interval)
        {
            tmr = new Timer();
            tmr.Interval = interval;
            tmr.Elapsed += new ElapsedEventHandler(tmr_Elapsed);
            tmr.Start();
            tmr.AutoReset = false;
        }

        void tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            tmr.Stop();
            Update();
            tmr.Start();
        }

        public void Update()
        {
            currentState = Keyboard.GetState();

            foreach (KeyWatch kw in watches)
                kw.Check(lastPressed, currentState);

            lastPressed = currentState.GetPressedKeys();
        }

        public void AddWatch(KeyWatch watch)
        {
            watches.Add(watch);
        }
    }

    public class KeyWatch
    {
        public enum keyEvent
        {
            PressUp,
            PressDown,
            Down,
            Up,
        }
        public Keys key;
        public keyEvent kEvent;
        public delegate void myCallbackDelegate();
        myCallbackDelegate dCallback;
        bool Ctrl;
        bool Shift;        
        bool Alt;

        public KeyWatch(Keys k, bool ctrl, bool shift, bool alt, keyEvent kevent, myCallbackDelegate callback)
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

        public void Check(Keys[] last, KeyboardState curr)
        {
            if (!matchMods(curr))
                return;
            switch (kEvent)
            {
                case KeyWatch.keyEvent.PressUp:
                    if (!(wasDown(key, last) && isUp(key, curr)))
                        return;                    
                    break;
                case KeyWatch.keyEvent.PressDown:
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
                if (!(curr.IsKeyDown(Keys.LeftControl) || curr.IsKeyDown(Keys.RightControl)))
                    return false;

            if(Shift)
                if (!(curr.IsKeyDown(Keys.LeftShift) || curr.IsKeyDown(Keys.RightShift)))
                    return false;

            if (Alt)
                if (!(curr.IsKeyDown(Keys.LeftAlt) || curr.IsKeyDown(Keys.RightAlt)))
                    return false;
            
            // we made it!
            return true;
        }
        public bool isDown(Keys key, KeyboardState curr)
        {
            if (key == Keys.Down)
            {
                //System.Diagnostics.Debug.WriteLine("Down");
            }
            return curr.IsKeyDown(key);
        }
        public bool isUp(Keys key, KeyboardState curr)
        {
            return curr.IsKeyUp(key);
        }
        public bool wasDown(Keys key, Keys[] last)
        {
            return last.Contains(key);
        }
        public bool wasUp(Keys key, Keys[] last)
        {
            return !last.Contains(key);
        }

    }
}
