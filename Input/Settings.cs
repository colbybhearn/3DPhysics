using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;

namespace Input
{
    public partial class Settings : Form
    {
        public KeyMap keyMap;
        public Settings(KeyMap km)
        {
            InitializeComponent();
            keyMap = km;
            AddKeys();
        }

        public void AddKeys()
        {
            foreach (Input.KeyMap.KeyBinding kb in keyMap.KeyBindings)
            {
                KeyBindingControl kbc = new KeyBindingControl(kb);
                flpBindings.Controls.Add(kbc);
            }
        }

        internal void ProcessKey(Microsoft.Xna.Framework.Input.KeyboardState currentState)
        {
            Microsoft.Xna.Framework.Input.Keys[] pressed = currentState.GetPressedKeys();
            if (pressed.Length < 1)
                return;                
            
            foreach (KeyBindingControl kbc in flpBindings.Controls)
            {
                if (kbc.Editing)
                {
                    kbc.SetKey(pressed[0]);
                    kbc.Editing = false;
                }
            }
            flpBindings.Focus();
            
        }
    }
}
