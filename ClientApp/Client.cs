using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using MultiplayerHelper;
using System.Diagnostics;
using ClientHelper;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


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

        ClientHelper.ClientHelper cHelper;
    
        System.Windows.Forms.Timer ProcessPacketTimer;

        ArrayList ActiveGames = new ArrayList();
        ArrayList GamesToPlay = new ArrayList();
            
        Queue<string> StatusMsgQueue = new Queue<string>();
        Queue<Packet> InputQueue = new Queue<Packet>();
        Queue<Packet> OutputQueue = new Queue<Packet>();

        Game.PhysGame game;
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
            TrayIcon.ShowBalloonTip(5000);
            btnDisconnect.Enabled = false;
            btnSendChat.Enabled = false;
                        
            ProcessPacketTimer = new System.Windows.Forms.Timer(this.components);
            ProcessPacketTimer.Interval = 100;
            ProcessPacketTimer.Tick += new EventHandler(ProcessPacketTimer_Tick);
            ProcessPacketTimer.Start();

            // Create an instance of the game
            game = new Game.PhysGame();
            // Give the xna panel a reference to game.
            // Xna Panel will initialize the game with its graphicsDevice the moment it is ready.
            AddXnaPanel(ref game);
        }

        private void InitializeScene()
        {            
        }

        private void AddXnaPanel(ref Game.PhysGame game)
        {
            // 
            // XnaPanelMain
            // 
            this.XnaPanelMain = new Winform_XNA.XnaPanel(ref game);
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
            this.XnaPanelMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.pnlKeyDown);
            this.XnaPanelMain.KeyUp += new System.Windows.Forms.KeyEventHandler(this.pnlKeyUp);
            this.XnaPanelMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlMouseDown);
            this.XnaPanelMain.MouseEnter += new System.EventHandler(this.pnlMouseEnter);
            this.XnaPanelMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlMouseMove);
            this.XnaPanelMain.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.pnlPreviewKeyDown);
            this.spMain.Panel2.Controls.Add(this.XnaPanelMain);
            //this.Controls.Add(this.XnaPanelMain);
        }


        #endregion

        private void ProcessClientGUIPackets()
        {
            if (InputQueue.Count > 0)
            {
                if (InputQueue.Peek().type == Packet.pType.TO_CLIENT_GUI)
                {
                    Packet p = InputQueue.Dequeue();
                    Debug.WriteLine("Client GUI Heard: "+p.ToString());
                    switch (p.info)
                    {
                        case Packet.pInfo.CLIENT_LIST_REFRESH:
                            int iClients = Convert.ToInt32(p.GetFieldValue("CLIENT COUNT"));

                            //lvClients.Clear();
                            //for (int i = 0; i < iClients; i++)
                                //lvClients.Items.Add(p.GetFieldValue("CLIENT ALIAS "+i.ToString()));
                            break;
                        case Packet.pInfo.STATUS_MESSAGE:
                            toolStripStatus.Text = p.GetFieldValue("STATUS MESSAGE");
                            break;
                        case Packet.pInfo.CONNECTION_INFO:
                            this.sKey = p.sClientTarget;
                            break;
                        case Packet.pInfo.CHAT_MESSAGE:
                            this.txtChatBox.Text += p.GetFieldValue("CHAT MESSAGE") + Convert.ToChar(13) + Convert.ToChar(10);
                            this.txtChatBox.Select(txtChatBox.Text.Length - 2, 1);
                            this.txtChatBox.ScrollToCaret();
                            break;
                        case Packet.pInfo.NEW_GAME:
                            MultiplayerHelper.Game g = new MultiplayerHelper.Game(p.GetFieldValue("GAME KEY"), p.GetFieldValue("GAME NAME"));
                            ActiveGames.Add(g);
                            //lvActiveGames.Items.Add(g.m_Name);
                            break;
                    }
                }
            }
        }

        #region Form Event Handlers

        private void btnConnect_Click(object sender, EventArgs e)
        {
            cHelper = new ClientHelper.ClientHelper(InputQueue,OutputQueue,txtIPAddress.Text, iPort);
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = true;
            cHelper.Connect(txtAlias.Text);
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
            cHelper.Disconnect();
        }

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            string s = txtAlias.Text + ": " + txtChat.Text;
            cHelper.SendChatMessage(s);
            txtChat.Text = "";
            txtChatBox.Text += s + Convert.ToChar(13) + Convert.ToChar(10);
            txtChatBox.Select(txtChatBox.Text.Length - 2, 1);
            txtChatBox.ScrollToCaret();
            btnSendChat.Enabled = false;
        }

        private void txtChat_TextChanged(object sender, EventArgs e)
        {
            if(txtChat.Text=="")
                btnSendChat.Enabled = false;
            else
                btnSendChat.Enabled = true;
        }

        private void txtAlias_TextChanged(object sender, EventArgs e)
        {
            tAliasChange.Start();
        }
        
        private void txtChat_KeyPress(object sender, KeyPressEventArgs e)
        {            
            if (e.KeyChar == 13)
            {
                btnSendChat_Click(null, null);
                e.Handled = true;
            }
        }

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            TrayIcon.Dispose();
        }

        #endregion

        #region Timer Ticks

        void ProcessPacketTimer_Tick(object sender, EventArgs e)
        {
            ProcessClientGUIPackets();
            ProcessPacketTimer.Start();
        }

        public void SendPacketToServer(Packet p)
        {
            OutputQueue.Enqueue(p);
        }

        private void tAliasChange_Tick(object sender, EventArgs e)
        {
            if(cHelper!=null)
            if (cHelper.isConnected())
            {
                StatusMsgQueue.Enqueue("Sending New Alias");
                Packet p = new Packet(Packet.pType.TO_SERVER,
                    Packet.pInfo.ALIAS_CHANGE,
                    Packet.pDelivery.BROADCAST_OTHERS,
                    this.sKey,
                    "");
                p.AddFieldValue("ALIAS",txtAlias.Text);
                OutputQueue.Enqueue(p);
                tAliasChange.Stop();
            }
        }

        private void tStatus_Tick(object sender, EventArgs e)
        {
            tStatus.Stop();
            if (StatusMsgQueue.Count > 0)
                toolStripStatus.Text = StatusMsgQueue.Dequeue();
            else if (cHelper!=null)
            {
                if(cHelper.isConnected())
                    toolStripStatus.Text = "Connected";
                else
                    StatusMsgQueue.Enqueue("Disconnected");
            
            }
            tStatus.Start();
        }

        #endregion

        private void txtChatBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void lvGameList_DragDrop(object sender, DragEventArgs e)
        {
            //e.Effect = DragDropEffects.Move;
            //e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, true);
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            /*
            MultiplayerHelper.Game g = new MultiplayerHelper.Game(lvGameList.SelectedItems[lvGameList.SelectedIndices[0]].Text);

            Packet p = new Packet(Packet.pType.TO_SERVER,
                Packet.pInfo.NEW_GAME,
                Packet.pDelivery.TARGETED,
                this.sKey,
                "SERVER");

            p.AddFieldValue("GAME KEY",g.m_Key);
            p.AddFieldValue("GAME NAME",g.m_Name);
            SendPacketToServer(p);*/
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
        #endregion

        #region Key Input
        private void pnlKeyDown(object sender, KeyEventArgs e)
        {
            XnaPanelMain.ProcessKeyDown(e);
        }
        private void pnlKeyUp(object sender, KeyEventArgs e)
        {
            XnaPanelMain.ProcessKeyUp(e);
        }
        private void pnlPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            XnaPanelMain.ProcessKeyDown(e);
        }
        #endregion
                      
    }
}