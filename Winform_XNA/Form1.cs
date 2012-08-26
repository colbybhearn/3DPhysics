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
            XnaPanelMain.Debug = chkDebug.Checked;
            XnaPanelMain.Focus();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            XnaPanelMain.ResetTimer();
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
                    XnaPanelMain.PanCam(dX, dY);                    
                }
            }
            lastX = e.X;
            lastY = e.Y;
        }

        private void test_XNAControl_KeyDown(object sender, KeyEventArgs e)
        {
            XnaPanelMain.ProcessKeyDown(e);
        }

        private void test_XNAControl_KeyUp(object sender, KeyEventArgs e)
        {
            XnaPanelMain.ProcessKeyUp(e);
        }

        private void test_XNAControl_MouseEnter(object sender, EventArgs e)
        {
            XnaPanelMain.Focus();
        }

        private void chkDebugPhysics_CheckedChanged(object sender, EventArgs e)
        {
            XnaPanelMain.DebugPhysics = chkDebugPhysics.Checked;
        }

        private void chkDrawing_CheckedChanged(object sender, EventArgs e)
        {
            XnaPanelMain.DrawingEnabled = chkDrawing.Checked;
        }

        private void XnaPanelMain_MouseClick(object sender, MouseEventArgs e)
        {            
        }

        private void chkPhysics_CheckedChanged(object sender, EventArgs e)
        {
            XnaPanelMain.PhysicsEnabled = chkPhysics.Checked;
        }

        private void XnaPanelMain_MouseDown(object sender, MouseEventArgs e)
        {
            XnaPanelMain.ProcessMouseDown(e, XnaPanelMain.Bounds);
        }

        private void tbStep_Scroll(object sender, EventArgs e)
        {
            float value = tbStep.Value;
            if(value <= 10)
                value /= 10;
            else
            {
                value = value - 10;
                // 15 should be around 5 times
                // 16 should be around 6
                // 18 should be around 9 times
            }
            //1 -> 1/10
            //11 -> 10/10
            //20 -> 20/10
            XnaPanelMain.SetSimFactor(value);

        }
        
    }
}
