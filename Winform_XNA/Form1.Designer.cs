namespace Winform_XNA
{
    partial class Form1
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
            this.chkDrawing = new System.Windows.Forms.CheckBox();
            this.chkDebugPhysics = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.chkDebug = new System.Windows.Forms.CheckBox();
            this.XnaPanelMain = new Winform_XNA.XnaPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.chkDrawing);
            this.splitContainer1.Panel1.Controls.Add(this.chkDebugPhysics);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.chkDebug);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.XnaPanelMain);
            this.splitContainer1.Size = new System.Drawing.Size(923, 557);
            this.splitContainer1.SplitterDistance = 207;
            this.splitContainer1.TabIndex = 0;
            // 
            // chkDrawing
            // 
            this.chkDrawing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkDrawing.AutoSize = true;
            this.chkDrawing.Checked = true;
            this.chkDrawing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawing.Location = new System.Drawing.Point(13, 482);
            this.chkDrawing.Name = "chkDrawing";
            this.chkDrawing.Size = new System.Drawing.Size(101, 17);
            this.chkDrawing.TabIndex = 3;
            this.chkDrawing.Text = "Enable Drawing";
            this.chkDrawing.UseVisualStyleBackColor = true;
            this.chkDrawing.CheckedChanged += new System.EventHandler(this.chkDrawing_CheckedChanged);
            // 
            // chkDebugPhysics
            // 
            this.chkDebugPhysics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkDebugPhysics.AutoSize = true;
            this.chkDebugPhysics.Location = new System.Drawing.Point(12, 505);
            this.chkDebugPhysics.Name = "chkDebugPhysics";
            this.chkDebugPhysics.Size = new System.Drawing.Size(97, 17);
            this.chkDebugPhysics.TabIndex = 2;
            this.chkDebugPhysics.Text = "Debug Physics";
            this.chkDebugPhysics.UseVisualStyleBackColor = true;
            this.chkDebugPhysics.CheckedChanged += new System.EventHandler(this.chkDebugPhysics_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(91, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Start Physics";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // chkDebug
            // 
            this.chkDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkDebug.AutoSize = true;
            this.chkDebug.Location = new System.Drawing.Point(12, 528);
            this.chkDebug.Name = "chkDebug";
            this.chkDebug.Size = new System.Drawing.Size(82, 17);
            this.chkDebug.TabIndex = 0;
            this.chkDebug.Text = "Debug Text";
            this.chkDebug.UseVisualStyleBackColor = true;
            this.chkDebug.CheckedChanged += new System.EventHandler(this.cbDebug_CheckedChanged);
            // 
            // XnaPanelMain
            // 
            this.XnaPanelMain.Debug = false;
            this.XnaPanelMain.DebugPhysics = false;
            this.XnaPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XnaPanelMain.DrawingEnabled = true;
            this.XnaPanelMain.Location = new System.Drawing.Point(0, 0);
            this.XnaPanelMain.Name = "XnaPanelMain";
            this.XnaPanelMain.Size = new System.Drawing.Size(712, 557);
            this.XnaPanelMain.TabIndex = 0;
            this.XnaPanelMain.Text = "XnaPanel";
            this.XnaPanelMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.test_XNAControl_KeyDown);
            this.XnaPanelMain.KeyUp += new System.Windows.Forms.KeyEventHandler(this.test_XNAControl_KeyUp);
            this.XnaPanelMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.XnaPanelMain_MouseClick);
            this.XnaPanelMain.MouseEnter += new System.EventHandler(this.test_XNAControl_MouseEnter);
            this.XnaPanelMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.test_XNAControl_MouseMove);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(923, 557);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private XnaPanel XnaPanelMain;
        private System.Windows.Forms.CheckBox chkDebug;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox chkDebugPhysics;
        private System.Windows.Forms.CheckBox chkDrawing;
    }
}

