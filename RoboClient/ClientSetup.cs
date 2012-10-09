using System;
using System.Windows.Forms;
using Game;

namespace RoboGame
{
    public partial class ClientSetup : Form
    {
        BaseGame game;
        public int iPort;
        public string sAlias;
        public string sIPAddress;
        frmClient roboclient;

        public ClientSetup()
        {
            InitializeComponent();
            game = new RoboGame();
            game.ConnectedToServer += new Helper.Handlers.ClientConnectedEH(game_ConnectedToServer);
            game.DiconnectedFromServer += new Helper.Handlers.IntEH(game_DiconnectedFromServer);
        }

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
                    roboclient = new frmClient(ref game);
                    roboclient.Show();
                }
            }
        }

        void game_DiconnectedFromServer(int i)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Helper.Handlers.IntEH(game_DiconnectedFromServer), new object[] { i });
                return;
            }
            btnConnect.Text = "Connect";
        }

        void game_ConnectedToServer(int id, string alias)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Helper.Handlers.ClientConnectedEH(game_ConnectedToServer), new object[] { id, alias });
                return;
            }
            btnConnect.Text = "Disconnect";
        }
        private void numLobbyPort_ValueChanged(object sender, EventArgs e)
        {
            iPort = (int)numLobbyPort.Value;
        }
        
        private void btnConnectLocal_Click(object sender, EventArgs e)
        {
            roboclient = new frmClient(ref game);
            roboclient.Show();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ClientSetup_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void ClientSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (roboclient != null)
                roboclient.Close();
        }

        private void ClientSetup_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
                this.Close();

        }

        
    }
}
