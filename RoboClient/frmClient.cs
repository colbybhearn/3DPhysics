using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Helper.Multiplayer.Packets;
using Microsoft.Xna.Framework.Input;
using RoboGame;

namespace RoboGame
{
    /*
   * I had to reference the WindowsGameLibrary from Clientapp in order for the ContentManager to load any models when invoked from the client (it worked fine in XNA_Panel and the missing reference was the only difference)
   * 
   * 
   */

    public partial class frmClient : Form
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



        public frmClient()
        {
            Mouse.WindowHandle = this.Handle;
            //Microsoft.Xna.Framework.Input.Keyboard.

            InitializeComponent();
            InitializeScene();
            sKey = "";// System.Guid.NewGuid().ToString();
            iPort = (int)numLobbyPort.Value;

            // Create an instance of the game
            game = new RoboGame();
            game.ConnectedToServer += new Helper.Handlers.ClientConnectedEH(game_ConnectedToServer);
            game.DiconnectedFromServer += new Helper.Handlers.IntEH(game_DiconnectedFromServer);
            // Give the xna panel a reference to game.
            // Xna Panel will initialize the game with its graphicsDevice the moment it is ready.
            AddXnaPanel(ref game);
            Application.Idle += new EventHandler(Application_Idle);
        }

        void game_DiconnectedFromServer(int i)
        {
            btnConnect.Text = "Connect";
        }

        void game_ConnectedToServer(int id, string alias)
        {
            btnConnect.Text = "Disconnect";
        }

        void Application_Idle(object sender, EventArgs e)
        {
            game.ProcessInput();
        }

        private void InitializeScene()
        {
        }
        XnaView.XnaPanel XnaPanelMain;
        private void AddXnaPanel(ref Game.BaseGame game)
        {
            // 
            // XnaPanelMain
            // 
            this.XnaPanelMain = new XnaView.XnaPanel(ref game);
            //this.XnaPanelMain.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top                 | System.Windows.Forms.AnchorStyles.Bottom                | System.Windows.Forms.AnchorStyles.Left                     | System.Windows.Forms.AnchorStyles.Right);
            this.XnaPanelMain.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.XnaPanelMain.Debug = false;
            this.XnaPanelMain.DebugPhysics = false;
            this.XnaPanelMain.DrawingEnabled = true;
            
            this.XnaPanelMain.Name = "XnaPanelMain";
            this.XnaPanelMain.PhysicsEnabled = true;
            RepositionXnaPanel();
            this.XnaPanelMain.TabIndex = 46;
            this.XnaPanelMain.Text = "XnaPanel";
            this.XnaPanelMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlMouseDown);
            this.XnaPanelMain.MouseEnter += new System.EventHandler(this.pnlMouseEnter);
            this.XnaPanelMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlMouseMove);
            this.XnaPanelMain.MouseWheel += new MouseEventHandler(XnaPanelMain_MouseWheel);
            this.Controls.Add(this.XnaPanelMain);
        }

        private void RepositionXnaPanel()
        {
            this.XnaPanelMain.Size = new System.Drawing.Size((int)(this.Width * .8f), (int)(this.Height * .6f));
            this.XnaPanelMain.Location = new System.Drawing.Point(this.Width / 2 - XnaPanelMain.Size.Width / 2, 0);
        }




        #endregion



        #region Form Event Handlers


        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (game.IsConnectedToServer)
            {
                game.DisconnectFromServer();
            }
            else
            {

                if (game.ConnectToServer(txtIPAddress.Text, iPort, txtAlias.Text))
                {
                }
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

        private void frmClient_SizeChanged(object sender, EventArgs e)
        {
            RepositionXnaPanel();
        }

    }
}