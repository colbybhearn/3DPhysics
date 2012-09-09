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
using System.Diagnostics;
using ServerHelper;
using MultiplayerHelper;

namespace ServerApp
{    
    public partial class Server : Form
    {

        #region Properties
        ServerHelper.ServerHelper sHelper;
        Queue<Packet> InputQueue = new Queue<Packet>();
        Queue<Packet> OutputQueue = new Queue<Packet>();
        int iLobbyPort;
        int iBasePort;

        System.Windows.Forms.Timer ProcessPacketTimer;
        #endregion
        
        #region Constructor
        Game.PhysGame game;
        public Server()
        {
            InitializeComponent();
            iLobbyPort = (int)numLobbyPort.Value;
            iBasePort = (int)numBasePort.Value;
            btnStopServer.Enabled = false;
            ProcessPacketTimer = new System.Windows.Forms.Timer(this.components);
            ProcessPacketTimer.Interval = 200;
            ProcessPacketTimer.Tick += new EventHandler(ProcessPacketsTimer_Tick);
            ProcessPacketTimer.Start();
            
            game = new Game.PhysGame();
            AddXnaPanel(ref game);
        }
        Winform_XNA.XnaPanel XnaPanelMain;
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



        private void ProcessServerGUIPackets()
        {
            if (InputQueue.Count > 0)
            {
                if (InputQueue.Peek().type == Packet.pType.TO_SERVER_GUI)
                {
                    Packet p = InputQueue.Dequeue();
                    switch (p.info)
                    {
                        case Packet.pInfo.CLIENT_LIST_REFRESH:
                            if (p.Fields.ContainsKey("CLIENT COUNT"))
                            {
                                int iClients = Convert.ToInt32(p.GetFieldValue("CLIENT COUNT"));

                                lstClients.Clear();
                                for (int i = 0; i < iClients; i++)
                                    lstClients.Items.Add(p.GetFieldValue("CLIENT ALIAS " + i.ToString()));
                            }
                            break;

                        case Packet.pInfo.STATUS_MESSAGE:
                            toolStripStatus.Text = p.GetFieldValue("STATUS MESSAGE");
                            break;
                        case Packet.pInfo.NEW_GAME:
                            lvActiveGames.Items.Add(p.GetFieldValue("GAME NAME"));
                            break;
                    }
                }
            }
        }
       
        #endregion
        
        #region Events Handlers

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            sHelper = new ServerHelper.ServerHelper(InputQueue,OutputQueue, this.iLobbyPort, this.iBasePort);
            sHelper.Start();
            btnStartServer.Enabled = false;
            btnStopServer.Enabled = true;
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            sHelper.Stop();
            btnStopServer.Enabled = false;
            btnStartServer.Enabled = true;
        }   
        
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
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

        #region Timers

        void ProcessPacketsTimer_Tick(object sender, EventArgs e)
        {
            ProcessServerGUIPackets();
            ProcessPacketTimer.Start();
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
/*
namespace Packets
{
    public class ServerPacket
    {
        public string sMsg;

        public ServerPacket(string s)
        {
            sMsg = s;
        }
    }

    public class ClientPacket
    {
        public int iClientKey;
        public string sMsg;

        public ClientPacket(int k, string s)
        {
            this.iClientKey = k;
            this.sMsg = s;
        }      
    }
}
*/



/*
        private void ServerListen()
        {
            if (ServerSetup())
            {
                byte[] b;

                while (!bDone)
                {
                    try
                    {
                        b = new byte[100];

                        //socket.ReceiveTimeout = 1000;
                        int k = socket.Receive(b);

                        lock (text)
                        {
                            text = "";
                            for (int i = 0; i < k; i++)
                                text += Convert.ToChar(b[i]);

                            if (text != "")
                            {
                                Debug.WriteLine("Server Lobby Heard: " + text);
                                ClientPackets.Enqueue(new ClientPacket(text));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.Message);
                        //s.ReceiveTimeout = 1001;
                    }
                }
                // clean up 
                socket.Close();
                myList.Stop();
            }
        }*/
/*
private void ClientListen()
{
    byte[] bb;

    while (!bDone)
    {
                
        bb = new byte[100];
        int k = stm.Read(bb, 0, 100);

        lock (text)
        {
            text = "";
            for (int i = 0; i < k; i++)
                text += Convert.ToChar(bb[i]);
                    
        }
        Thread.Sleep(100);
    }            
    tcpclnt.Close();
}*/
/*
private bool ServerSetup()
{
    try 
    {
        ipAd = IPAddress.Parse(txtIPAddress.Text);
         // use local m/c IP address, and 
         // use the same in the client

        // Initializes the Listener 
        myList=new TcpListener(ipAd,(int)numLobbyPort.Value);

        // Start Listeneting at the specified port         
        myList.Start();
                
        //lblStatus.Text = "The server is running at port 2302. Waiting...";

        //                 Console.WriteLine("The local End point is  :" + 
        //                  myList.LocalEndpoint );
                                 
        socket=myList.AcceptSocket();
        //lblStatus.Text = "Connection accepted from " + s.RemoteEndPoint;

        return true;
    }
    catch (Exception ex)
    {
        //lblStatus.Text = "Error: " + ex.StackTrace;
        return false;
    }
}
*/
/*
private bool ClientSetup()
{
  try
  {
      tcpclnt = new TcpClient();
      lblStatus.Text="Connecting...";

      tcpclnt.Connect(txtIPAddress.Text,(int)numLobbyPort.Value);
      // use the ipaddress as in the server program

      lblStatus.Text="Connected.";
      stm = tcpclnt.GetStream();

      return true;
                
  }
  catch (Exception ex)
  {
      Console.WriteLine("Error..... " + ex.StackTrace);
      return false;
  }
}




 private void sendMessage(string s)
        {            
            myList.Start();
            this.socket.Send(asen.GetBytes(s));
        }

        private void broadcastToAllClients(string s)
        {
            for (int i = 0; i < Clients.Count; i++)
                     ((Client)Clients[i]).SendToClient(new ServerPacket(s));
        }

        private void forwardToOtherClients(ClientPacket cp)
        {
            for (int i = 0; i < Clients.Count; i++)
                if (cp.iClientKey != i)
                    ((Client)Clients[i]).SendToClient(new ServerPacket(cp.sMsg));
        }
*/