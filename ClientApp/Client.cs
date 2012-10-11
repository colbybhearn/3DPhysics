using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Helper.Multiplayer.Packets;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ClientApp
{
    /*
   * I had to reference the WindowsGameLibrary from Clientapp in order for the ContentManager to load any models when invoked from the client (it worked fine in XNA_Panel and the missing reference was the only difference)
   * 
   * 
   */

    public partial class Client : Form
    {
        #region Properties

        public string sKey;
        public int iPort;
        
        public string sAlias;        
        public string sIPAddress;

    
        System.Windows.Forms.Timer ProcessPacketTimer;

        ArrayList ActiveGames = new ArrayList();
        ArrayList GamesToPlay = new ArrayList();
            
        Queue<string> StatusMsgQueue = new Queue<string>();
        Queue<Packet> InputQueue = new Queue<Packet>();
        Queue<Packet> OutputQueue = new Queue<Packet>();

        Game.BaseGame game;
        #endregion

        #region Constructor

        

        public Client()
        {
            Mouse.WindowHandle = this.Handle;
            //Microsoft.Xna.Framework.Input.Keyboard.

            InitializeComponent();
            InitializeScene();
            sKey = "";// System.Guid.NewGuid().ToString();
            iPort = (int)numLobbyPort.Value;
            tStatus.Start();
            btnDisconnect.Enabled = false;

            // Create an instance of the game
            game = new Game.ExampleGame();
            //game.ClientDisconnected+=new Helper.Handlers.StringEH(game_ClientDisconnected);
            // Give the xna panel a reference to game.
            // Xna Panel will initialize the game with its graphicsDevice the moment it is ready.
            AddXnaPanel(ref game);
            Application.Idle += new EventHandler(Application_Idle);
        }

        void Application_Idle(object sender, EventArgs e)
        {
            game.ProcessInput();
        }

        private void InitializeScene()
        {
        }

        private void AddXnaPanel(ref Game.BaseGame game)
        {
            // 
            // XnaPanelMain
            // 
            this.XnaPanelMain = new XnaView.XnaPanel(ref game);
            //this.XnaPanelMain.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top                 | System.Windows.Forms.AnchorStyles.Bottom                | System.Windows.Forms.AnchorStyles.Left                     | System.Windows.Forms.AnchorStyles.Right);
            this.XnaPanelMain.Dock = DockStyle.Fill;
            this.XnaPanelMain.Debug = false;
            this.XnaPanelMain.DebugPhysics = false;
            this.XnaPanelMain.DrawingEnabled = true;            
            this.XnaPanelMain.Location = new System.Drawing.Point(296, 3);
            this.XnaPanelMain.Name = "XnaPanelMain";
            this.XnaPanelMain.PhysicsEnabled = true;
            this.XnaPanelMain.Size = new System.Drawing.Size(596, 366);
            this.XnaPanelMain.TabIndex = 46;
            this.XnaPanelMain.Text = "XnaPanel";
            this.XnaPanelMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlMouseDown);
            this.XnaPanelMain.MouseEnter += new System.EventHandler(this.pnlMouseEnter);
            this.XnaPanelMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlMouseMove);
            this.XnaPanelMain.MouseWheel += new MouseEventHandler(XnaPanelMain_MouseWheel);
            this.spMain.Panel2.Controls.Add(this.XnaPanelMain);
            //this.Controls.Add(this.XnaPanelMain);
        }

        


        #endregion

        

        #region Form Event Handlers

        
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (game.ConnectToServer(txtIPAddress.Text, iPort, txtAlias.Text))
            {
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
            }
        }

        private void Client_Load(object sender, EventArgs e)
        {
        }

        private void numLobbyPort_ValueChanged(object sender, EventArgs e)
        {
            iPort = (int)numLobbyPort.Value;
        }

        private void btnDisconnet_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            game.DisconnectFromServer();
        }

        
        private void ClientApp_MainFormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
            game.Stop();
        }

        #endregion

        #region Timer Ticks
        public void SendPacketToServer(Packet p)
        {
            OutputQueue.Enqueue(p);
            FrameworkDispatcher.Update();
        }

        private void tStatus_Tick(object sender, EventArgs e)
        {
            tStatus.Stop();
            if (StatusMsgQueue.Count > 0)
                toolStripStatus.Text = StatusMsgQueue.Dequeue();
            
            tStatus.Start();
        }

        #endregion

        private void txtChatBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }


        #region Mouse Input
        private void pnlMouseEnter(object sender, EventArgs e)
        {
            XnaPanelMain.Focus();
        }
        float lastX;
        float lastY;
        private void pnlMouseMove(object sender, MouseEventArgs e)
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
        private void pnlMouseDown(object sender, MouseEventArgs e)
        {
            XnaPanelMain.ProcessMouseDown(e, XnaPanelMain.Bounds);
        }
        void XnaPanelMain_MouseWheel(object sender, MouseEventArgs e)
        {
            game.AdjustZoom(e.Delta);
        }
        #endregion

        private void tsmiSettings_Click(object sender, EventArgs e)
        {
            game.EditSettings();
        }
    }
}