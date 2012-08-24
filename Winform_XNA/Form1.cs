using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Winform_XNA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void cbDebug_CheckedChanged(object sender, EventArgs e)
        {
            test_XNAControl.Debug = chkDebug.Checked;
            test_XNAControl.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            test_XNAControl.ResetTimer();
            button1.Enabled = false;
        }


        float lastX;
        float lastY;
        private void test_XNAControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (lastX != 0 && lastY != 0)
                {
                    float dX = lastX - e.X;
                    float dY = lastY - e.Y;
                    test_XNAControl.PanCam(dX, dY);                    
                }
            }
            lastX = e.X;
            lastY = e.Y;
        }

        private void test_XNAControl_KeyDown(object sender, KeyEventArgs e)
        {
            test_XNAControl.ProcessKeyDown(e);
        }

        private void test_XNAControl_KeyUp(object sender, KeyEventArgs e)
        {
            test_XNAControl.ProcessKeyUp(e);
        }

        private void test_XNAControl_MouseEnter(object sender, EventArgs e)
        {
            test_XNAControl.Focus();
        }

        private void chkDebugPhysics_CheckedChanged(object sender, EventArgs e)
        {
            test_XNAControl.DebugPhysics = true;
        }
        
    }
}
