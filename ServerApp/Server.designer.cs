namespace ServerApp
{
    partial class Server
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server));
            this.btnStartServer = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnStopServer = new System.Windows.Forms.Button();
            this.lstClients = new System.Windows.Forms.ListView();
            this.label3 = new System.Windows.Forms.Label();
            this.numLobbyPort = new System.Windows.Forms.NumericUpDown();
            this.numBasePort = new System.Windows.Forms.NumericUpDown();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.lvActiveGames = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbStep = new System.Windows.Forms.TrackBar();
            this.spMain = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.numLobbyPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBasePort)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spMain)).BeginInit();
            this.spMain.Panel1.SuspendLayout();
            this.spMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStartServer
            // 
            this.btnStartServer.Location = new System.Drawing.Point(12, 12);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(59, 23);
            this.btnStartServer.TabIndex = 2;
            this.btnStartServer.Text = "Start";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Lobby Port:";
            // 
            // btnStopServer
            // 
            this.btnStopServer.Location = new System.Drawing.Point(77, 12);
            this.btnStopServer.Name = "btnStopServer";
            this.btnStopServer.Size = new System.Drawing.Size(59, 23);
            this.btnStopServer.TabIndex = 14;
            this.btnStopServer.Text = "Stop";
            this.btnStopServer.UseVisualStyleBackColor = true;
            this.btnStopServer.Click += new System.EventHandler(this.btnStopServer_Click);
            // 
            // lstClients
            // 
            this.lstClients.Location = new System.Drawing.Point(12, 294);
            this.lstClients.Name = "lstClients";
            this.lstClients.Size = new System.Drawing.Size(148, 142);
            this.lstClients.TabIndex = 18;
            this.lstClients.UseCompatibleStateImageBehavior = false;
            this.lstClients.View = System.Windows.Forms.View.List;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 160);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Client Base Port:";
            // 
            // numLobbyPort
            // 
            this.numLobbyPort.Location = new System.Drawing.Point(94, 132);
            this.numLobbyPort.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numLobbyPort.Name = "numLobbyPort";
            this.numLobbyPort.Size = new System.Drawing.Size(66, 20);
            this.numLobbyPort.TabIndex = 21;
            this.numLobbyPort.Value = new decimal(new int[] {
            2302,
            0,
            0,
            0});
            this.numLobbyPort.ValueChanged += new System.EventHandler(this.numLobbyPort_ValueChanged);
            // 
            // numBasePort
            // 
            this.numBasePort.Location = new System.Drawing.Point(94, 158);
            this.numBasePort.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numBasePort.Name = "numBasePort";
            this.numBasePort.Size = new System.Drawing.Size(66, 20);
            this.numBasePort.TabIndex = 22;
            this.numBasePort.Value = new decimal(new int[] {
            2303,
            0,
            0,
            0});
            this.numBasePort.ValueChanged += new System.EventHandler(this.numBasePort_ValueChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 517);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(763, 22);
            this.statusStrip1.TabIndex = 23;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatus
            // 
            this.toolStripStatus.Name = "toolStripStatus";
            this.toolStripStatus.Size = new System.Drawing.Size(42, 17);
            this.toolStripStatus.Text = "Status:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 278);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Clients:";
            // 
            // TrayIcon
            // 
            this.TrayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.TrayIcon.BalloonTipText = "\'Amy Server\' is running. =D";
            this.TrayIcon.BalloonTipTitle = "Amy Server";
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "Amy Gaming Server";
            this.TrayIcon.Visible = true;
            // 
            // lvActiveGames
            // 
            this.lvActiveGames.Location = new System.Drawing.Point(12, 208);
            this.lvActiveGames.Name = "lvActiveGames";
            this.lvActiveGames.Size = new System.Drawing.Size(148, 67);
            this.lvActiveGames.TabIndex = 25;
            this.lvActiveGames.UseCompatibleStateImageBehavior = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 192);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "Games:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 28;
            this.label5.Text = "Physics Rate:";
            // 
            // tbStep
            // 
            this.tbStep.AutoSize = false;
            this.tbStep.Location = new System.Drawing.Point(12, 76);
            this.tbStep.Maximum = 20;
            this.tbStep.Name = "tbStep";
            this.tbStep.Size = new System.Drawing.Size(148, 26);
            this.tbStep.TabIndex = 27;
            this.tbStep.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbStep.Value = 10;
            this.tbStep.Scroll += new System.EventHandler(this.tbStep_Scroll);
            // 
            // spMain
            // 
            this.spMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spMain.Location = new System.Drawing.Point(0, 0);
            this.spMain.Name = "spMain";
            // 
            // spMain.Panel1
            // 
            this.spMain.Panel1.Controls.Add(this.btnStartServer);
            this.spMain.Panel1.Controls.Add(this.label5);
            this.spMain.Panel1.Controls.Add(this.label2);
            this.spMain.Panel1.Controls.Add(this.tbStep);
            this.spMain.Panel1.Controls.Add(this.btnStopServer);
            this.spMain.Panel1.Controls.Add(this.label4);
            this.spMain.Panel1.Controls.Add(this.lstClients);
            this.spMain.Panel1.Controls.Add(this.lvActiveGames);
            this.spMain.Panel1.Controls.Add(this.label3);
            this.spMain.Panel1.Controls.Add(this.label1);
            this.spMain.Panel1.Controls.Add(this.numLobbyPort);
            this.spMain.Panel1.Controls.Add(this.numBasePort);
            this.spMain.Size = new System.Drawing.Size(763, 517);
            this.spMain.SplitterDistance = 176;
            this.spMain.TabIndex = 29;
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 539);
            this.Controls.Add(this.spMain);
            this.Controls.Add(this.statusStrip1);
            this.MinimumSize = new System.Drawing.Size(337, 264);
            this.Name = "Server";
            this.Text = "WinPhysiX Server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.numLobbyPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBasePort)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbStep)).EndInit();
            this.spMain.Panel1.ResumeLayout(false);
            this.spMain.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spMain)).EndInit();
            this.spMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStopServer;
        private System.Windows.Forms.ListView lstClients;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numLobbyPort;
        private System.Windows.Forms.NumericUpDown numBasePort;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ListView lvActiveGames;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar tbStep;
        private System.Windows.Forms.SplitContainer spMain;
    }
}

