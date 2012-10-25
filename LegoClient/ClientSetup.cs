using System;
using System.Windows.Forms;
using Game;
using LegoClient;

namespace LegoGame
{
    public partial class ClientSetup : Form
    {
        BaseGame game;
        public int iPort = 2302;
        public string sAlias = "Alias";
        public string sIPAddress = "127.0.0.1";
        frmClient legoclient;

        public ClientSetup()
        {
            InitializeComponent();
            
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (game !=null && game.IsConnectedToServer)
            {                
                CloseClient();
            }
            else
            {
                CreateGame();
                if (game.ConnectToServer(txtIPAddress.Text, iPort, txtAlias.Text))
                {
                    LaunchClient();                    
                }
            }
        }

        private void CreateGame()
        {
            game = new LegoGame(false);
            game.OtherClientConnectedToServer += new Helper.Handlers.ClientConnectedEH(game_ConnectedToServer);
            game.ThisClientDisconnectedFromServer += new Helper.Handlers.voidEH(game_ThisClientDiconnectedFromServer);
        }

        void game_ThisClientDiconnectedFromServer()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Helper.Handlers.voidEH(game_ThisClientDiconnectedFromServer));
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
            CreateGame();
            LaunchClient();
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
            LegoClient.Properties.Settings.Default.Save();
            CloseClient();
        }

        private void ClientSetup_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
                this.Close();
        }

        private void ClientSetup_Load(object sender, EventArgs e)
        {
            

            // Comment me to stop auto-launching
            //LaunchClient();
        }

        private void LaunchClient()
        {
            legoclient = new frmClient(ref game);
            legoclient.Show();
        }

        private void CloseClient()
        {
            game.Stop(); // this closes the client form via a stopped event handled by the client form
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            game.EditSettings();
        }
    }
}
