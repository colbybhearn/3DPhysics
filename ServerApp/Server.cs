using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Helper.Multiplayer.Packets;
using Helper;

namespace ServerApp
{    
    public partial class Server : Form
    {

        #region Properties
        Queue<Packet> InputQueue = new Queue<Packet>();
        Queue<Packet> OutputQueue = new Queue<Packet>();
        int iLobbyPort;
        int iBasePort;

        #endregion
        
        #region Constructor
        Game.BaseGame game;
        public Server()
        {
            InitializeComponent();
            iLobbyPort = (int)numLobbyPort.Value;
            iBasePort = (int)numBasePort.Value;
            btnStopServer.Enabled = false;
            
            game = new RoboGame.RoboGame();
            game.ClientConnected += new Helper.Handlers.IntStringEH(game_ClientConnected); 
            AddXnaPanel(ref game);
        }

        void game_ClientConnected(int id, string alias)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Helper.Handlers.IntStringEH(game_ClientConnected), new object[] { id, alias });
                return;
            }
            lstClients.Items.Add(alias);
        }

        XnaView.XnaPanel XnaPanelMain;
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
            this.XnaPanelMain.KeyDown += new KeyEventHandler(XnaPanelMain_KeyDown);
            this.XnaPanelMain.KeyUp += new KeyEventHandler(XnaPanelMain_KeyUp);
            this.XnaPanelMain.MouseDown += new MouseEventHandler(XnaPanelMain_MouseDown);
            this.XnaPanelMain.MouseEnter += new EventHandler(XnaPanelMain_MouseEnter);
            this.XnaPanelMain.MouseMove += new MouseEventHandler(XnaPanelMain_MouseMove);
            this.XnaPanelMain.PreviewKeyDown += new PreviewKeyDownEventHandler(XnaPanelMain_PreviewKeyDown);
            this.spMain.Panel2.Controls.Add(this.XnaPanelMain);
            //this.Controls.Add(this.XnaPanelMain);
        }

        void XnaPanelMain_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
        }

        void XnaPanelMain_MouseMove(object sender, MouseEventArgs e)
        {
        }

        void XnaPanelMain_MouseEnter(object sender, EventArgs e)
        {
        }

        void XnaPanelMain_MouseDown(object sender, MouseEventArgs e)
        {
        }

        void XnaPanelMain_KeyUp(object sender, KeyEventArgs e)
        {
        }

        void XnaPanelMain_KeyDown(object sender, KeyEventArgs e)
        {
        }
        #endregion
        
        #region Events Handlers

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            game.ListenForClients(iLobbyPort);
            btnStartServer.Enabled = false;
            btnStopServer.Enabled = true;
        }


        private void btnStopServer_Click(object sender, EventArgs e)
        {
            btnStopServer.Enabled = false;
            btnStartServer.Enabled = true;
        }
        
        private void ServerApp_MainFormClosed(object sender, FormClosedEventArgs e)
        {
            game.Stop();
        }

        private void numLobbyPort_ValueChanged(object sender, EventArgs e)
        {
            iLobbyPort = (int)numLobbyPort.Value;
        }

        private void numBasePort_ValueChanged(object sender, EventArgs e)
        {
            iBasePort = (int)numBasePort.Value;
        }

        #endregion


        private void tbStep_Scroll(object sender, EventArgs e)
        {
            float value = tbStep.Value;
            if (value <= 10)
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
            game.SetSimFactor(value);
        }

    }
}