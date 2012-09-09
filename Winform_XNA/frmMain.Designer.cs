namespace Winform_XNA
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.tbStep = new System.Windows.Forms.TrackBar();
            this.chkPhysics = new System.Windows.Forms.CheckBox();
            this.chkDraw = new System.Windows.Forms.CheckBox();
            this.chkDebugPhysics = new System.Windows.Forms.CheckBox();
            this.chkDebugText = new System.Windows.Forms.CheckBox();
            this.XnaPanelMain = new Winform_XNA.XnaPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbStep)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.tbStep);
            this.splitContainer1.Panel1.Controls.Add(this.chkPhysics);
            this.splitContainer1.Panel1.Controls.Add(this.chkDraw);
            this.splitContainer1.Panel1.Controls.Add(this.chkDebugPhysics);
            this.splitContainer1.Panel1.Controls.Add(this.chkDebugText);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.XnaPanelMain);
            this.splitContainer1.Size = new System.Drawing.Size(923, 557);
            this.splitContainer1.SplitterDistance = 132;
            this.splitContainer1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 119);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Physics Rate:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // tbStep
            // 
            this.tbStep.Location = new System.Drawing.Point(3, 135);
            this.tbStep.Maximum = 20;
            this.tbStep.Name = "tbStep";
            this.tbStep.Size = new System.Drawing.Size(126, 45);
            this.tbStep.TabIndex = 5;
            this.tbStep.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbStep.Value = 10;
            this.tbStep.Scroll += new System.EventHandler(this.tbStep_Scroll);
            // 
            // chkPhysics
            // 
            this.chkPhysics.AutoSize = true;
            this.chkPhysics.Checked = true;
            this.chkPhysics.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPhysics.Location = new System.Drawing.Point(12, 12);
            this.chkPhysics.Name = "chkPhysics";
            this.chkPhysics.Size = new System.Drawing.Size(98, 17);
            this.chkPhysics.TabIndex = 4;
            this.chkPhysics.Text = "Enable Physics";
            this.chkPhysics.UseVisualStyleBackColor = true;
            this.chkPhysics.CheckedChanged += new System.EventHandler(this.chkPhysics_CheckedChanged);
            // 
            // chkDraw
            // 
            this.chkDraw.AutoSize = true;
            this.chkDraw.Checked = true;
            this.chkDraw.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDraw.Location = new System.Drawing.Point(12, 35);
            this.chkDraw.Name = "chkDraw";
            this.chkDraw.Size = new System.Drawing.Size(101, 17);
            this.chkDraw.TabIndex = 3;
            this.chkDraw.Text = "Enable Drawing";
            this.chkDraw.UseVisualStyleBackColor = true;
            this.chkDraw.CheckedChanged += new System.EventHandler(this.chkDrawing_CheckedChanged);
            // 
            // chkDebugPhysics
            // 
            this.chkDebugPhysics.AutoSize = true;
            this.chkDebugPhysics.Location = new System.Drawing.Point(12, 58);
            this.chkDebugPhysics.Name = "chkDebugPhysics";
            this.chkDebugPhysics.Size = new System.Drawing.Size(97, 17);
            this.chkDebugPhysics.TabIndex = 2;
            this.chkDebugPhysics.Text = "Debug Physics";
            this.chkDebugPhysics.UseVisualStyleBackColor = true;
            this.chkDebugPhysics.CheckedChanged += new System.EventHandler(this.chkDebugPhysics_CheckedChanged);
            // 
            // chkDebugText
            // 
            this.chkDebugText.AutoSize = true;
            this.chkDebugText.Location = new System.Drawing.Point(12, 81);
            this.chkDebugText.Name = "chkDebugText";
            this.chkDebugText.Size = new System.Drawing.Size(82, 17);
            this.chkDebugText.TabIndex = 0;
            this.chkDebugText.Text = "Debug Text";
            this.chkDebugText.UseVisualStyleBackColor = true;
            this.chkDebugText.CheckedChanged += new System.EventHandler(this.cbDebug_CheckedChanged);
            // 
            // XnaPanelMain
            // 
            this.XnaPanelMain.Debug = false;
            this.XnaPanelMain.DebugPhysics = false;
            this.XnaPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XnaPanelMain.DrawingEnabled = true;
            this.XnaPanelMain.Location = new System.Drawing.Point(0, 0);
            this.XnaPanelMain.Name = "XnaPanelMain";
            this.XnaPanelMain.PhysicsEnabled = true;
            this.XnaPanelMain.Size = new System.Drawing.Size(787, 557);
            this.XnaPanelMain.TabIndex = 0;
            this.XnaPanelMain.Text = "XnaPanel";
            this.XnaPanelMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.test_XNAControl_KeyDown);
            this.XnaPanelMain.KeyUp += new System.Windows.Forms.KeyEventHandler(this.test_XNAControl_KeyUp);
            this.XnaPanelMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.XnaPanelMain_MouseDown);
            this.XnaPanelMain.MouseEnter += new System.EventHandler(this.test_XNAControl_MouseEnter);
            this.XnaPanelMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.test_XNAControl_MouseMove);
            this.XnaPanelMain.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.XnaPanelMain_PreviewKeyDown);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(923, 557);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmMain";
            this.Text = "inFormed XNA Physics";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tbStep)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private XnaPanel XnaPanelMain;
        private System.Windows.Forms.CheckBox chkDebugText;
        private System.Windows.Forms.CheckBox chkDebugPhysics;
        private System.Windows.Forms.CheckBox chkDraw;
        private System.Windows.Forms.CheckBox chkPhysics;
        private System.Windows.Forms.TrackBar tbStep;
        private System.Windows.Forms.Label label1;
    }
}

