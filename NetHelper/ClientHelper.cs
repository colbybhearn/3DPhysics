using System;
using System.Collections.Generic;
using System.Text;
using MultiplayerHelper;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Timers;

namespace ClientHelper
{
    public class ClientHelper
    {
        #region Properties

        bool bConnected;
        bool bStopThreads;

        public int iKey;
        public int iPort;
        
        public string sAlias;        
        public string sIPAddress;
        
        private System.Timers.Timer ProcessServerDataTimer;
        private System.Timers.Timer StopThreadTimer;

        Queue<string> StatusMsgQueue = new Queue<string>();
        Queue<Packet> InputQueue = new Queue<Packet>();
        Queue<Packet> OutputQueue = new Queue<Packet>();
        //Queue<ServerPacket> myInputData;
        //Queue<ClientPacket> myOutputData;

        private System.ComponentModel.IContainer components = null;

        Stream stm;
        TcpClient tcpclnt;
        ASCIIEncoding asen = new ASCIIEncoding();

        Thread ServerReader;
        Thread ServerWriter;
        #endregion

        #region Methods

        #region Constructor
        public ClientHelper(Queue<Packet> inQueue, Queue<Packet> outQueue, string ip, int port)
        {
            ServerReader = new Thread(new ThreadStart(ReadServer));
            ServerWriter = new Thread(new ThreadStart(WriteServer));

            this.components = new System.ComponentModel.Container();
            ProcessServerDataTimer = new System.Timers.Timer();
            ProcessServerDataTimer.Elapsed += new ElapsedEventHandler(ProcessServerPackets);
            ProcessServerDataTimer.Interval =200;
            
            //this.ProcessServerDataTimer. = 200;
            //this.ProcessServerDataTimer.Tick += new System.EventHandler(this.ProcessServerData_Tick);

            this.components = new System.ComponentModel.Container();
            this.StopThreadTimer = new System.Timers.Timer();
            this.StopThreadTimer.Interval = 1000;
            this.StopThreadTimer.Elapsed += new ElapsedEventHandler(StopThreadTimer_Tick);
            
            iPort = port;
            sIPAddress = ip;
            //TrayIcon.ShowBalloonTip(5000);

            InputQueue = inQueue;
            OutputQueue = outQueue;

            bStopThreads = false;
        }

        void StopThreadTimer_Tick(object sender, ElapsedEventArgs e)
        {
            bStopThreads = true;
        }
        #endregion

        public bool isConnected()
        {
            return bConnected;
        }

        private void ConnectToServer(bool bToLobby, string alias)
        {
            ProcessServerDataTimer.Stop();

            while ((System.Diagnostics.ThreadState)ServerWriter.ThreadState == System.Diagnostics.ThreadState.Running ||
                (System.Diagnostics.ThreadState)ServerReader.ThreadState == System.Diagnostics.ThreadState.Running)
            {
                bStopThreads = true;
            }

            ServerReader = new Thread(new ThreadStart(ReadServer));
            ServerWriter = new Thread(new ThreadStart(WriteServer));                 

            if (Setup())
            {
                ProcessServerDataTimer.Start();
                ServerWriter.Start();
                ServerReader.Start();

                if (bToLobby)
                {
                    Packet p = new Packet(Packet.pType.TO_SERVER,
                        Packet.pInfo.NEW_CLIENT,
                        Packet.pDelivery.TARGETED,
                        "nullKey",
                        "");
                    p.AddFieldValue("ALIAS","nullAlias");
                    OutputQueue.Enqueue(p);
                }
                else
                    StatusMsgQueue.Enqueue("Reconnecting On Designated Port:"+ iPort.ToString());
            }
            else
            {
                /*
                if (bToLobby)
                    MessageBox.Show("Unable to connect to Server. Verify server has been started");
                else
                    MessageBox.Show("Unable to connect on Designated Client Port: "+ iPort.ToString()+"\nCheck Firewall and Router settings and verify that this \nport is not currently being used by other programs.");
                 * */
            }
        }

        private bool Setup()
        {
            Debug.WriteLine("Client: Setup Connection " + sIPAddress + " " + iPort);
            try
            {
                StatusMsgQueue.Enqueue("Connecting");
                //Debug.WriteLine("Client: Creating new TCPclient");
                tcpclnt = new TcpClient();
                //Debug.WriteLine("Client: Connecting");
                StatusMsgQueue.Enqueue("Connecting.");

                tcpclnt.Connect(sIPAddress, iPort);
                StatusMsgQueue.Enqueue("Connecting..");

                //Debug.WriteLine("Client: Getting Stream");                
                stm = tcpclnt.GetStream();
                StatusMsgQueue.Enqueue("Connecting...");
                
                //Debug.WriteLine("Client: Connected");
                return true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error..... " + ex.StackTrace);
                return false;
            }
        }

        private void ReadServer()
        {
            //if (Setup())
            {

                byte[] bb;
                string sMsg;

                while (!bStopThreads)
                {
                    try
                    {
                        bb = new byte[500];
                        //stm.ReadTimeout = 1000;
                        int k = stm.Read(bb, 0, 500);

                        sMsg = "";
                        for (int i = 0; i < k; i++)
                            sMsg += Convert.ToChar(bb[i]);
                        Debug.WriteLine("Client Heard: " + sMsg);
                        Packet p = new Packet();
                        p.FromString(sMsg);
                        InputQueue.Enqueue(p);
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.ToString());
                        //stm.ReadTimeout = 1000;
                    }
                    Thread.Sleep(100);
                }
                
            }
        }

        private void WriteServer()
        {
            while (!bStopThreads)
            {
                if (OutputQueue.Count > 0)
                {
                    // if this packet is headed away from the client entirely,
                    if (OutputQueue.Peek().type != Packet.pType.TO_CLIENT_GUI)
                    {
                        //stm = tcpclnt.GetStream();
                        string s = OutputQueue.Dequeue().ToString();
                        byte[] ba = asen.GetBytes(s);
                        if(stm.CanWrite)
                            stm.Write(ba, 0, ba.Length);
                        Debug.WriteLine("Client Says: " + s);
                    }
                    else // move this Client GUI Packet to the input queue
                    {
                        InputQueue.Enqueue(OutputQueue.Dequeue());
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void SendPacket(Packet p)
        {
            OutputQueue.Enqueue(p);
        }

        private void ProcessServerPackets(object sender, ElapsedEventArgs e)
        {
            if (InputQueue.Count > 0)
            {
                //string data = InputQueue.Dequeue().sMsg;
                //string[] word = data.Split("|".ToCharArray());

                Packet p = new Packet();
                if (InputQueue.Peek().type == Packet.pType.TO_CLIENT)
                {
                    p.FromString(InputQueue.Dequeue().ToString());

                    switch (p.info)
                    {
                        #region CONNECTION INFO
                        case Packet.pInfo.CONNECTION_INFO:
                            this.iPort = Convert.ToInt32(p.GetFieldValue("PORT"));
                            this.sKey = p.GetFieldValue("CLIENT KEY");
                            stm.Close();
                            tcpclnt.Close();
                            stm.Dispose();

                            ConnectToServer(false, sAlias);

                            p.type = Packet.pType.TO_CLIENT_GUI;
                            OutputQueue.Enqueue(p);

                            Packet p1 = new Packet(Packet.pType.TO_SERVER,
                                Packet.pInfo.CLIENT_LIST_ADD,
                                Packet.pDelivery.TARGETED,
                                this.sKey,
                                "");
                            p1.AddFieldValue("ALIAS",sAlias);
                            OutputQueue.Enqueue(p1);

                            bConnected = true;
                            break;                            
                        #endregion
                        case Packet.pInfo.CLIENT_LIST_REFRESH:
                            break;
                            /*
                        #region NEW GAME
                        case Packet.pInfo.NEW_GAME:

                            SendPacket(new Packet(Packet.pType.TO_CLIENT_GUI,
                                Packet.pInfo.NEW_GAME,
                                Packet.pDelivery.TARGETED,
                                "",
                                "",
                                p.sMessage);
                            break;
                        #endregion */

                    }
                }
            }
        }

        private void SendClientPackets()
        {
            if (OutputQueue.Count > 0)
            {
                //OutputQueue.Enqueue(new ClientPacket(this.sKey, OutputQueue.Dequeue()));
            }          
        }

        private void CloseConnection()
        {
            StopThreadTimer.Start();
        }

        public void SendChatMessage(string sMsg)
        {
            StatusMsgQueue.Enqueue("Sending Message");
            Packet p = new Packet(
                Packet.pType.TO_SERVER,
                Packet.pInfo.CHAT_MESSAGE,
                Packet.pDelivery.BROADCAST_OTHERS,
                this.sKey,
                "SERVER");
            p.AddFieldValue("CHAT MESSAGE", sMsg);
            OutputQueue.Enqueue(p);
        }

        public void Connect(string alias)
        {
            this.sAlias = alias;
            ConnectToServer(true,sAlias);            
        }

        public void Disconnect()
        {
            Packet p = new Packet(
                Packet.pType.TO_SERVER,
                Packet.pInfo.CLIENT_DISCONNECT,
                Packet.pDelivery.BROADCAST_OTHERS,
                this.sKey,
                "SERVER");
            p.AddFieldValue("ALIAS", sAlias);
            SendPacket(p);    
            CloseConnection();
        }

        public void Dispose()
        {
        }

        #endregion

        #region Timer Ticks

        private void ProcessServerData_Tick()
        {
            ProcessServerPackets(null, null );
            SendClientPackets();
        }

        #endregion

        Player pPlayer;
        IPAddress IP;
        private string sKey;

        public ClientHelper()
        {
            pPlayer = new Player();
            IP = IPAddress.Any;
            iPort = 2304;
            sKey = "";// System.Guid.NewGuid().ToString();
        }

        public ClientHelper(Player player, int port, string s)
        {
            pPlayer = player;
            iPort = port;
            sKey = s;
        }

        public void setKey(string k)
        {
            sKey = k;
        }

        public void setPlayer(Player player)
        {
            pPlayer = player;
        }

        public void setPlayer(int key, string name, Color color, p3d p)
        {
            pPlayer = new Player(key, name, color, p);
        }

        public string getKey()
        {
            return sKey;
        }

        public ElapsedEventHandler ProcessServerDataTimer_Elapsed { get; set; }
    }

    public class Player
    {
        private p3d position;
        private p3d momentum;
        private p3d rotation;
        private string sName;
        private int iScore;
        private Color cColor;


        public Player()
        {
            position = new p3d(0.0, 0.0, 0.0);
            sName = "Colby";
            iScore = 0;
            cColor = Color.Blue;
        }

        public Player(int key, string name, Color c, p3d p)
        {
            position = p;
            sName = name;
            iScore = 0;
            cColor = c;
        }

        public void setPosition(p3d p)
        {
            position = p;
        }

        public void setName(string s)
        {
            sName = s;
        }

        public p3d getPosition()
        {
            return position;
        }

        public string getName()
        {
            return sName;
        }


    }

    public class p2d
    {
        private double x;
        private double y;

        public p2d()
        {
            x = 0;
            y = 0;
        }

        public void setX(double d)
        {
            x = d;
        }

        public void setY(double d)
        {
            y = d;
        }

        public double getX()
        {
            return x;
        }

        public double getY()
        {
            return y;
        }

    }

    public class p3d
    {
        private double x;
        private double y;
        private double z;

        public p3d()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public p3d(double dx, double dy, double dz)
        {
            x = dx;
            y = dy;
            z = dz;
        }

        public void setX(double d)
        {
            x = d;
        }

        public void setY(double d)
        {
            y = d;
        }

        public void setZ(double d)
        {
            z = d;
        }

        public double getX()
        {
            return x;
        }

        public double getY()
        {
            return y;
        }

        public double getZ()
        {
            return z;
        }

    }

}
