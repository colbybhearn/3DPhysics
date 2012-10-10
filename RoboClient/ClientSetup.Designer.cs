namespace RoboGame
{
    partial class ClientSetup
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtAlias = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numLobbyPort = new System.Windows.Forms.NumericUpDown();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnConnectLocal = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLobbyPort)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.txtAlias);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtIPAddress);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numLobbyPort);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(149, 152);
            this.groupBox1.TabIndex = 43;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connectivity";
            // 
            // txtAlias
            // 
            this.txtAlias.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::RoboGame.Properties.Settings.Default, "Alias", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtAlias.Location = new System.Drawing.Point(9, 111);
            this.txtAlias.Name = "txtAlias";
            this.txtAlias.Size = new System.Drawing.Size(134, 20);
            this.txtAlias.TabIndex = 1;
            this.txtAlias.Text = global::RoboGame.Properties.Settings.Default.Alias;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Port:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 95);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 35;
            this.label5.Text = "Alias:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "Server IP:";
            // 
            // numLobbyPort
            // 
            this.numLobbyPort.Location = new System.Drawing.Point(9, 72);
            this.numLobbyPort.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numLobbyPort.Name = "numLobbyPort";
            this.numLobbyPort.Size = new System.Drawing.Size(66, 20);
            this.numLobbyPort.TabIndex = 2;
            this.numLobbyPort.Value = new decimal(new int[] {
            2302,
            0,
            0,
            0});
            // 
            // btnConnect
            // 
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Location = new System.Drawing.Point(12, 175);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(108, 23);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "Connect to Sever";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Location = new System.Drawing.Point(355, 175);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 45;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnConnectLocal
            // 
            this.btnConnectLocal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnectLocal.Location = new System.Drawing.Point(185, 175);
            this.btnConnectLocal.Name = "btnConnectLocal";
            this.btnConnectLocal.Size = new System.Drawing.Size(108, 23);
            this.btnConnectLocal.TabIndex = 46;
            this.btnConnectLocal.Text = "Connect to Local";
            this.btnConnectLocal.UseVisualStyleBackColor = true;
            this.btnConnectLocal.Click += new System.EventHandler(this.btnConnectLocal_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(185, 12);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(75, 23);
            this.btnSettings.TabIndex = 47;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::RoboGame.Properties.Settings.Default, "Ip", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtIPAddress.Location = new System.Drawing.Point(9, 33);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(134, 20);
            this.txtIPAddress.TabIndex = 0;
            this.txtIPAddress.Text = global::RoboGame.Properties.Settings.Default.Ip;
            // 
            // ClientSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 210);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnConnectLocal);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnConnect);
            this.KeyPreview = true;
            this.Name = "ClientSetup";
            this.Text = "RoboGame Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientSetup_FormClosing);
            this.Load += new System.EventHandler(this.ClientSetup_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ClientSetup_KeyPress);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.ClientSetup_PreviewKeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLobbyPort)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtAlias;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numLobbyPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnConnectLocal;
        private System.Windows.Forms.Button btnSettings;
    }
}