using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;

namespace Helper.Input
{
    public partial class KeyBindingControl : UserControl
    {
        public Input.KeyBinding binding;
        public bool Editing;
        public KeyBindingControl(Input.KeyBinding kb)
        {
            binding = kb;
            InitializeComponent();
            lblAlias.Text = binding.Alias;
            UpdateBindingKeyAlias();
        }

        private void tbBinding_Click(object sender, EventArgs e)
        {
            Editing = true;
        }

        internal void SetKey(KeyboardState curr)
        {
            Microsoft.Xna.Framework.Input.Keys[] pressed = curr.GetPressedKeys();
            // make sure a key was pressed
            if (pressed.Length < 1)
                return;
            if (pressed[0] == Microsoft.Xna.Framework.Input.Keys.LeftShift ||
                pressed[0] == Microsoft.Xna.Framework.Input.Keys.RightShift ||
                pressed[0] == Microsoft.Xna.Framework.Input.Keys.LeftControl ||
                pressed[0] == Microsoft.Xna.Framework.Input.Keys.RightControl ||
                pressed[0] == Microsoft.Xna.Framework.Input.Keys.LeftAlt ||
                pressed[0] == Microsoft.Xna.Framework.Input.Keys.RightAlt)
                // we don't care, we need a real key to go with it!
                return;
            binding.Key = pressed[0];
            binding.Shift = curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);
            binding.Ctrl = curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl);
            binding.Alt = curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) || curr.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt);
            UpdateBindingKeyAlias();
            // take it out of edit mode!
            Editing = false;
        }

        private void UpdateBindingKeyAlias()
        {
            txtBinding.Text = binding.ToString();
        }
    }
}
