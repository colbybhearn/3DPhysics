using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Input
{
    public partial class KeyBindingControl : UserControl
    {
        public Input.KeyMap.KeyBinding binding;
        public bool Editing;
        public KeyBindingControl(Input.KeyMap.KeyBinding kb)
        {
            binding = kb;
            InitializeComponent();
            lblAlias.Text = binding.Alias;
            tbBinding.Text = binding.Key.ToString();
        }

        private void tbBinding_Click(object sender, EventArgs e)
        {
            Editing = true;
        }

        internal void SetKey(Microsoft.Xna.Framework.Input.Keys keys)
        {
            binding.Key = keys;
            tbBinding.Text = binding.Key.ToString();
        }
    }
}
