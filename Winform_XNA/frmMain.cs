using System;
using System.Windows.Forms;

namespace Winform_XNA
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        #region Toggle checkboxes
        private void cbDebug_CheckedChanged(object sender, EventArgs e)
        {
            XnaPanelMain.Debug = chkDebugText.Checked;
            XnaPanelMain.Focus();
        }
        private void chkDebugPhysics_CheckedChanged(object sender, EventArgs e)
        {
            XnaPanelMain.DebugPhysics = chkDebugPhysics.Checked;
        }
        private void chkDrawing_CheckedChanged(object sender, EventArgs e)
        {
            XnaPanelMain.DrawingEnabled = chkDraw.Checked;
        }
        private void chkPhysics_CheckedChanged(object sender, EventArgs e)
        {
            XnaPanelMain.PhysicsEnabled = chkPhysics.Checked;
        }
        #endregion

        #region Mouse Input
        private void test_XNAControl_MouseEnter(object sender, EventArgs e)
        {
            XnaPanelMain.Focus();
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
            //XnaPanelMain.SetSimFactor(value);

        }
        #endregion

        #region Key Input
        private void test_XNAControl_KeyDown(object sender, KeyEventArgs e)
        {
        }
        private void test_XNAControl_KeyUp(object sender, KeyEventArgs e)
        {
        }
        private void XnaPanelMain_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
        }
        #endregion

        private void label1_Click(object sender, EventArgs e)
        {

        }

    }
}
