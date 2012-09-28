using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Helper.Input
{
    public partial class FlowLayoutTabPage : TabPage
    {
        public FlowLayoutTabPage()
        {
            InitializeComponent();
        }

        internal void AddControl(KeyBindingControl kbc)
        {
            flpBindings.Controls.Add(kbc);
            kbc.BindFieldClicked += new Handlers.voidEH(kbc_BindFieldClicked); // prevent multiple keybindingcontrols from being edited at the same time. that would cause anarchy.
        }

        void kbc_BindFieldClicked()
        {
            // mark all keybindingcontrols as not being edited because only one should be being edited at any one time.
            foreach (KeyBindingControl kbc in flpBindings.Controls)
                kbc.Editing = false;
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
    }
}
