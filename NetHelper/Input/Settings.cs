using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;

namespace Helper.Input
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
            foreach (Input.KeyBinding kb in keyMap.KeyBindings)
            {
                KeyBindingControl kbc = new KeyBindingControl(kb);
                flpBindings.Controls.Add(kbc);
            }
        }

        internal void ProcessKey(Microsoft.Xna.Framework.Input.KeyboardState currentState)
        {
            foreach (KeyBindingControl kbc in flpBindings.Controls)
            {
                // if this keybindingcontrol is in edit mode
                if (kbc.Editing)
                {
                    // apply the pressed key to it
                    kbc.SetKey(currentState);
                                        
                    // get focus off of the textbox
                    flpBindings.Focus();
                }
            }
            
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
