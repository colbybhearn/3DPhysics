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

namespace Winform_XNA
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
                            int iClients = Convert.ToInt32(p.GetFieldValue("CLIENT COUNT"));

                            lstClients.Clear();
                            for (int i = 0; i < iClients; i++)
                                lstClients.Items.Add(p.GetFieldValue("CLIENT ALIAS "+i.ToString()));
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