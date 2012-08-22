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
            test_XNAControl.Debug = cbDebug.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            test_XNAControl.WireUpTimer();
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

                    //float scaleFactor = .005f * Math.Abs(test_XNAControl.CameraPosition.Y);
                    float dX = lastX - e.X;
                    float dY = lastY - e.Y;
                    //dX *= scaleFactor;
                    //dY *= scaleFactor;

                    //test_XNAControl.CameraPosition.X += dX;
                    //simView1.CameraPosition.Y -= dY;
                    // Map is Top down, looking at the X-Z plane, so a mouse movement in the Y direction, is actually Z
                    //test_XNAControl.CameraPosition.Z += dY;
                    //txtCam.Text = test_XNAControl.CameraPosition.X.ToString() + ", " + test_XNAControl.CameraPosition.Z.ToString();
                    test_XNAControl.PanCam(dX, dY);
                }
            }
            lastX = e.X;
            lastY = e.Y;
        }

        private void test_XNAControl_KeyDown(object sender, KeyEventArgs e)
        {
            test_XNAControl.ProcessKey(e);
        }
    }
}
