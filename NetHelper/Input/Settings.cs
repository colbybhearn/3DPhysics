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
        public KeyMapCollection keyMaps;
        public Settings(KeyMapCollection kmc)
        {
            InitializeComponent();
            keyMaps = kmc;
            AddKeyBindingGuiControls();
        }

        /// <summary>
        /// Adds controls (custom tab pages containing keybindingControls) for displaying each keybinding
        /// </summary>
        public void AddKeyBindingGuiControls()
        {
            for (int kmi = 0; kmi < keyMaps.keyMaps.Count; kmi++)
            {
                FlowLayoutTabPage fltp = new FlowLayoutTabPage();
                tcControlGroups.TabPages.Add(fltp);
                KeyMap km = keyMaps.keyMaps.Values[kmi];
                fltp.Text = km.Alias;
                foreach (Input.KeyBinding kb in km.KeyBindings.Values)
                {
                    KeyBindingControl kbc = new KeyBindingControl(kb);
                    
                    fltp.AddControl(kbc);
                }
            }
        }

        /// <summary>
        /// Processes the current keystate to set a keybinding.
        /// </summary>
        /// <param name="currentState"></param>
        internal void ProcessKey(Microsoft.Xna.Framework.Input.KeyboardState currentState)
        {
            foreach (TabPage tp in tcControlGroups.Controls)
            {
                // if the tab page is one of the custom tab pages (should always be the case)
                if (tp is FlowLayoutTabPage)
                {
                    // make it so
                    FlowLayoutTabPage fltp = tp as FlowLayoutTabPage;
                    // in case this tab page contains the active keybinding control needing to set its binding.
                    fltp.ProcessKey(currentState);
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
