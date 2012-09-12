using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using MultiplayerHelper;
using System.Threading;
using System.Timers;
using System.IO;
using System.Diagnostics;

namespace ServerHelper
{
    public class ServerHelper
    {
        #region Properties
        int iLobbyPort;
        int iBasePort;
        IPAddress ipAd;
        Queue<Packet> LobbyInputQueue;
        Queue<Packet> LobbyOutputQueue;
        ArrayList Clients = new ArrayList();
        LobbyListener Lobby;
        Queue<string> StatusMsgQueue = new Queue<string>();
        bool bRunning;
        string sIPAddress;
        private System.ComponentModel.IContainer components = null;
        private System.Timers.Timer tStatus;
        private System.Timers.Timer tProcessClientsTimer;

        #endregion

        #region Methods

        #region Constructor

        

        public ServerHelper(Queue<Packet> input, Queue<Packet> output, int lobbyPort, int basePort)
        {
            LobbyInputQueue = input;
            LobbyOutputQueue = output;

            this.components = new System.ComponentModel.Container();
            #region Setup IP Address
            string a = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostByName(a);
            IPAddress[] ips = ipEntry.AddressList;
            string ip = ips[0].ToString();
            sIPAddress = ip;
            #endregion

            iLobbyPort = lobbyPort;
            iBasePort = basePort;
            #region Timer
            tProcessClientsTimer = new System.Timers.Timer();
            tProcessClientsTimer.Interval = 200;
            tProcessClientsTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.tProcessClientsTimer_Tick);

            tStatus = new System.Timers.Timer();
            tStatus.Interval = 200;
            tStatus.Elapsed +=new System.Timers.ElapsedEventHandler(this.tStatus_Tick);

            #endregion

            tStatus.Start();
//            TrayIcon.ShowBalloonTip(5000);
        }

        public void Dispose()
        {
            //TrayIcon.Dispose();
            Stop();
        }

        #endregion

        public void Start()
        {
            bRunning = true;
            StatusMsgQueue.Enqueue("Preparing Server Lobby");
            Lobby = new LobbyListener(LobbyInputQueue, LobbyOutputQueue, sIPAddress, iLobbyPort, Clients.Count);
            tProcessClientsTimer.Start();
        }

        public void Stop()
        {
            Lobby.StopListening();
            for (int i = 0; i < Clients.Count; i++)
            {
                ((SHClient)Clients[i]).StopClient();
            }

            Packet p = new Packet(
                Packet.pType.TO_CLIENT,
                Packet.pInfo.CONNECTION_INFO,
                Packet.pDelivery.BROADCAST_ALL,
                "SERVER",
                "");
            p.AddFieldValue("ACTION","STOPPING");
            SendPacket(p);

        }
        
        private void ProcessClientPackets()
        {
            if (LobbyInputQueue.Count > 0)
            {
                Packet p = LobbyInputQueue.Dequeue();
                //string[] word = p.sMsg.Split("|".ToCharArray());
                string sClientSender = p.sClientSource;
                string s = "";
                switch(p.info)
                {
                    #region NEW CLIENT
                    case Packet.pInfo.NEW_CLIENT:
                        StatusMsgQueue.Enqueue("Receiving New Client");
                        int iNewPort = (int)(iBasePort + Clients.Count);
                        //Debug.WriteLine("Server: Creating Special Listener");
                       
                        string sKey = System.Guid.NewGuid().ToString();

                        Packet p1 = new Packet(Packet.pType.TO_CLIENT,
                            Packet.pInfo.CONNECTION_INFO,
                            Packet.pDelivery.TARGETED,
                            "Server",
                            sKey);

                        p1.AddFieldValue("PORT",((int)(iBasePort + Clients.Count)).ToString());
                        p1.AddFieldValue("CLIENT KEY",sKey);

                        LobbyOutputQueue.Enqueue(p1);
                        //Debug.WriteLine("Server: Sent Connection Info");

                        StatusMsgQueue.Enqueue("Adding New Client");
                        Clients.Add(new SHClient(LobbyInputQueue, sIPAddress, iNewPort, sKey));
                        //Debug.WriteLine("Server: Done creating client object");

                        Lobby.StopListening();
                        StatusMsgQueue.Enqueue("Preparing Server Lobby");
                        Lobby = new LobbyListener(LobbyInputQueue,LobbyOutputQueue,sIPAddress,iLobbyPort,Clients.Count);
                        //Debug.WriteLine("Server: Done creating new LobbyListener");

                        
                        break;
                    #endregion
                        
                    #region ADD TO CLIENT LIST
                    case Packet.pInfo.CLIENT_LIST_ADD:
                        StatusMsgQueue.Enqueue("Updating Client List");

                        ((SHClient)Clients[Clients.Count - 1]).sAlias = p.GetFieldValue("ALIAS"); ;
                        
                        Packet p2 = new Packet(
                            Packet.pType.TO_CLIENT_GUI,
                            Packet.pInfo.CLIENT_LIST_REFRESH,
                            Packet.pDelivery.BROADCAST_ALL,
                            "SERVER",
                            "");

                        p2.AddFieldValue("CLIENT COUNT",Clients.Count.ToString());

                        for (int i = 0; i < Clients.Count; i++)
                            p2.AddFieldValue("CLIENT ALIAS "+i.ToString(), ((SHClient)Clients[i]).sAlias);

                        StatusMsgQueue.Enqueue("Sending Client List");
                        
                        SendPacket(p2);

                        Packet serverP2 = p2.Clone();
                        serverP2.type = Packet.pType.TO_SERVER_GUI;
                        SendPacket(serverP2);


                        break;
                    #endregion
                        
                    #region ALIAS CHANGE
                    case Packet.pInfo.ALIAS_CHANGE:
                        StatusMsgQueue.Enqueue("Updating Alias");

                        SHClient t = getClientByKey(sClientSender);

                        if(t!=null)

                            t.sAlias = p.GetFieldValue("ALIAS");
                        
                        
                        Packet p3 = new Packet(
                            Packet.pType.TO_CLIENT_GUI,
                            Packet.pInfo.CLIENT_LIST_REFRESH,
                            Packet.pDelivery.BROADCAST_ALL,
                            "SERVER",
                            "");
                        p3.AddFieldValue("CLIENT COUNT",Clients.Count.ToString());
                        AddClientAliases(p3);

                        SendPacket(p3);
                        //send this to the server gui also
                        p3.type = Packet.pType.TO_SERVER_GUI;
                        SendPacket(p3);

                        break;
                    #endregion

                    #region CHAT MESSAGE
                    case Packet.pInfo.CHAT_MESSAGE:
                        StatusMsgQueue.Enqueue("Forwarding Chat Message");
                        if (Clients.Count > 1)
                        {
                            Packet p4 = new Packet(Packet.pType.TO_CLIENT_GUI,
                                Packet.pInfo.CHAT_MESSAGE,
                                Packet.pDelivery.BROADCAST_OTHERS,
                                "SERVER",
                                sClientSender);

                            p4.AddFieldValue("CHAT MESSAGE",p.GetFieldValue("CHAT MESSAGE"));
                            SendPacket(p4);
                            //TODO: Add a correction for if the user types in a | in his chat message
                            // piece it back together with hasMoreWords
                        }
                        else
                        {
                            //broadcastToAllClients("CHAT MESSAGE|Server: There are no other clients connected, so stop talking to yourself. kthxbye.");
                            Packet p5 = new Packet(
                            Packet.pType.TO_CLIENT_GUI,
                            Packet.pInfo.CHAT_MESSAGE,
                            Packet.pDelivery.BROADCAST_ALL,
                            "SERVER",
                            "");
                            p5.AddFieldValue("CHAT MESSAGE","Server: There are no other clients connected, so stop talking to yourself. kthxbye.");
                            SendPacket(p5);
                        }

                        break;
                    #endregion

                    #region CLIENT DISCONNECT
                    case Packet.pInfo.CLIENT_DISCONNECT:
                        Clients.Remove(getClientByKey(p.sClientSource));

                        Packet p6 = new Packet(
                            Packet.pType.TO_CLIENT_GUI,
                            Packet.pInfo.CHAT_MESSAGE,
                            Packet.pDelivery.BROADCAST_ALL,
                            "SERVER",
                            "");                            
                        
                            p6.AddFieldValue("CHAT MESSAGE","Server: " + p.GetFieldValue("ALIAS") + " is no longer connected.");
                            SendPacket(p6);

                        

                        p6 = new Packet(
                            Packet.pType.TO_SERVER_GUI,
                            Packet.pInfo.CLIENT_LIST_REFRESH,
                            Packet.pDelivery.TARGETED,
                            "SERVER",
                            "");
                        AddClientAliases(p6);
                        SendPacket(p6);

                        break;
                    #endregion

                    #region NEW GAME:
                    case Packet.pInfo.NEW_GAME:
                        Packet p7 = new Packet(Packet.pType.TO_SERVER_GUI,
                            Packet.pInfo.NEW_GAME,
                            Packet.pDelivery.TARGETED,
                            "SERVER",
                            "SERVER_GUI");

                        p7.AddFieldValue("GAME NAME", p.GetFieldValue("GAME NAME"));
                        p7.AddFieldValue("GAME KEY", p.GetFieldValue("GAME KEY"));
                        SendPacket(p7);

                        p7.type = Packet.pType.TO_CLIENT_GUI;
                        p7.delivery = Packet.pDelivery.BROADCAST_ALL;
                        SendPacket(p7);

                        break;
                    #endregion


                    /*
                    case "DISCONNECT":
                        //int index = Convert.ToInt32(word[1]);
                        //Clients.RemoveAt(index);
                        //lstClients.Items.RemoveAt(index);
                        break;
                         * */
                    case Packet.pInfo.GAME_DATA:
                        //forwardToOtherClients(new ClientPacket(sClientSender,"GAME DATA|" + word[1] + "|" + word[2] + "|" + word[3]));
                        break;
                }
            }
        }

        private SHClient getClientByKey(string s)
        {
            for (int i = 0; i < Clients.Count; i++)
                if (((SHClient)Clients[i]).getKey() == s)
                    return (SHClient)Clients[i];
            return null;
        }

        private Packet AddClientAliases(Packet p)
        {
            for (int i = 0; i < Clients.Count; i++)
                p.AddFieldValue("CLIENT ALIAS " + i.ToString(), ((SHClient)Clients[i]).sAlias);
            return p;
        }

        private void broadcastToAllClients(string s)
        {/*
            for (int i = 0; i < Clients.Count; i++)
                // this may be a waste of time, because the Packet Class should help here or something.
                ((SHClient)Clients[i]).SendToClient(new Packet(
                    Packet.pType.TO_CLIENT,
                    Packet.pInfo.GENERIC_DATA
                    s));
          * */
        }
        /*
        private void forwardToOtherClients(Packet p)
        {
            for (int i = 0; i < Clients.Count; i++)
                if (p.sClientSource != ((SHClient)Clients[i]).getKey())
                    ((SHClient)Clients[i]).SendToClient(p);
        }*/

        private void SendPacket(Packet p)
        {
            bool done = false;
            if (p.type != Packet.pType.TO_SERVER_GUI)
            {
                for (int i = 0; !done && i < Clients.Count; i++)
                    switch ((Packet.pDelivery)p.delivery)
                    {
                        case Packet.pDelivery.TARGETED:
                            if (p.sClientTarget == ((SHClient)Clients[i]).getKey())
                            {
                                ((SHClient)Clients[i]).SendToClient(p);
                                done = true;
                            }
                            break;

                        case Packet.pDelivery.BROADCAST_OTHERS:
                            if (p.sClientTarget != ((SHClient)Clients[i]).getKey())
                                ((SHClient)Clients[i]).SendToClient(p);
                            break;

                        case Packet.pDelivery.BROADCAST_ALL:
                            ((SHClient)Clients[i]).SendToClient(p);
                            break;
                    }
            }
            else
            {
                this.LobbyInputQueue.Enqueue(p);
            }
        }


        #endregion

        #region Timer Ticks

        private void tProcessClientsTimer_Tick(object sender, ElapsedEventArgs e)
        {
            ProcessClientPackets();
        }

        private void tStatus_Tick(object sender, ElapsedEventArgs e)
        {
            /*
            if (StatusMsgQueue.Count > 0)
                toolStripStatus.Text = StatusMsgQueue.Dequeue();
            else if (bRunning)
                toolStripStatus.Text = "Running";
            else
                toolStripStatus.Text = "Stopped";
            */

        }

        #endregion
    }

    public class LobbyListener
    {
        #region Properties
        string sIPAddress;
        int iPort;
        ASCIIEncoding asen = new ASCIIEncoding();
        bool bStopThread;
        IPAddress ipAd;
        TcpListener myList;
        Socket socket;
        Thread ServerListener;
        TcpClient tcpclnt;
        Stream stm;
        string text = "";
        int iNumClients = 0;
        Queue<Packet> InputQueue;// = new Queue<ClientPacket>();
        Queue<Packet> OutputQueue;// = new Queue<ClientPacket>();
        System.Threading.Timer timer1;

        #endregion

        #region Methods

        #region Constructor

        public LobbyListener(Queue<Packet> inputQueue, Queue<Packet> outputQueue, string IP, int port, int c)
        {

            this.InputQueue = inputQueue;
            this.OutputQueue = outputQueue;
            this.sIPAddress = IP;
            this.iPort = port;
            this.iNumClients = c;

            this.timer1 = new System.Threading.Timer(new TimerCallback(sendPacketsToClient), null, 5000, 500);

            ServerListener = new Thread(new ThreadStart(ServerListen));

            Debug.WriteLine("Lobby Listener: Listening on port: " + iPort.ToString());
            ServerListener.Start();
            Debug.WriteLine("Lobby Listener: ServerListener.Start() finished: " + iPort.ToString());


        }

        #endregion

        private void sendMessage(string s)
        {
            this.socket.Send(asen.GetBytes(s));
        }

        public void setNumClients(int c)
        {
            iNumClients = c;
        }

        private bool ServerSetup()
        {
            try
            {
                ipAd = IPAddress.Parse(sIPAddress);
                // use local m/c IP address, and 
                Debug.WriteLine("Lobby Listener: creating Listener");

                // Initializes the Listener 
                // myList = new TcpListener(ipAd, iPort);
                myList = new TcpListener(IPAddress.Any, iPort);

                Debug.WriteLine("Lobby Listener: starting Listener");
                // Start Listeneting at the specified port         
                myList.Start();
                Debug.WriteLine("Lobby Listener: Waiting to Accept on: " + iPort.ToString());

                while (!bStopThread && socket==null)
                {
                    if (myList.Pending())
                        socket = myList.AcceptSocket();
                    Thread.Sleep(500);
                }

                return true;
            }
            catch (Exception ex)
            {
                string s = "Error: " + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(s);
                return false;
            }
        }

        private void StartListening()
        {
            ServerListener.Start();
        }

        public void StopListening()
        {
            timer1.Dispose();
            //socket.Close();
            //myList.Stop();
            this.bStopThread = true;

        }

        private void sendPacketsToClient(object o)
        {
            if (OutputQueue.Count > 0)
            {
                sendMessage(OutputQueue.Dequeue().ToString());
            }
        }

        private void ServerListen()
        {
            if (ServerSetup())
            {
                byte[] b;

                while (!bStopThread)
                {
                    try
                    {
                        b = new byte[500];

                        socket.ReceiveTimeout = 1000;
                        int k = socket.Receive(b);

                        lock (text)
                        {
                            text = "";
                            for (int i = 0; i < k; i++)
                                text += Convert.ToChar(b[i]);

                            if (text != "")
                            {
                                Debug.WriteLine("Lobby Listener Heard: " + text);
                                Packet p = new Packet();
                                p.FromString(text);

                                InputQueue.Enqueue(p);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);

                        //MessageBox.Show(e.Message);
                        //s.ReceiveTimeout = 1001;
                    }
                }
                /* clean up */
                if(socket!=null)
                socket.Close();
                myList.Stop();
            }
        }

        #endregion
    }

    class SHClient
    {
        #region Properties
        bool bStopThreads;
        ASCIIEncoding asen = new ASCIIEncoding();
        TcpListener myListener;
        Socket socket;
        IPAddress ipAddress;
        public string sAlias;
        public int iPort;
        public string sIPAddress;
        Queue<Packet> InputQueue;
        Queue<Packet> OutputQueue = new Queue<Packet>();

        Thread ClientReader;
        Thread ClientWriter;
        System.Threading.Timer timer1;
        public string sKey;
        #endregion

        #region Method

        #region Contstructor

        public SHClient(Queue<Packet> input, string IP, int port, string k)
        {
            this.InputQueue = input;
            this.timer1 = new System.Threading.Timer(new TimerCallback(sendServerPackets), null, 5000, 500);

            this.sKey = k;
            //this.sAlias = a;
            this.sIPAddress = IP;
            this.iPort = port;
            ClientReader = new Thread(new ThreadStart(ReadClient));
            ClientWriter = new Thread(new ThreadStart(WriteClient));

            if (Setup())
            {
                ClientWriter.Start();
                ClientReader.Start();
            }
        }

        #endregion

        public void StopClient()
        {
            bStopThreads = true;
        }

        private void sendServerPackets(object o)
        {
            //OutputQueue.Enqueue(new ServerPacket("Heartbeat"));
        }

        private void ReadClient()
        {

            byte[] b;
            string sMsg = "";

            while (!bStopThreads)
            {
                try
                {
                    b = new byte[500];
                    //socket.ReceiveTimeout = 200;
                    int k = socket.Receive(b);

                    sMsg = "";
                    for (int i = 0; i < k; i++)
                        sMsg += Convert.ToChar(b[i]);
                    sMsg.Replace("\n", "");

                    Packet p = new Packet();
                    p.FromString(sMsg);
                    InputQueue.Enqueue(p);


                    Debug.WriteLine("Server Heard: '" + sMsg + "'");
                }
                catch (Exception e)
                {

                }
                Thread.Sleep(100);
            }
            socket.Close();
            myListener.Stop();
        }

        private void WriteClient()
        {
            while (!bStopThreads)
            {
                if (OutputQueue.Count > 0)
                {
                    if (OutputQueue.Peek().type != Packet.pType.TO_SERVER_GUI)
                    {
                        try
                        {
                            string s = OutputQueue.Dequeue().ToString();
                            //socket.SendTimeout = 1000;
                            socket.Send(asen.GetBytes(s));
                            Debug.WriteLine("Server Says: " + s);
                        }
                        catch (Exception e)
                        {
                            /*
                             * InputQueue.Enqueue(new Packet(
                                Packet.pType.TO_SERVER,
                                Packet.pInfo.
                                "", "DISCONNECT|" + this.sKey.ToString()));
                             * */
                            bStopThreads = true;
                        }
                    }
                    else // move this Server GUI Packet to the input queue
                    {
                        InputQueue.Enqueue(OutputQueue.Dequeue());
                    }
                }
                Thread.Sleep(100);
            }
        }

        public void SendToClient(Packet p)
        {
            OutputQueue.Enqueue(p);            
        }

        private bool Setup()
        {
            try
            {
                ipAddress = IPAddress.Parse(sIPAddress);
                Debug.WriteLine("Client-Server: Listening...");
                //myListener = new TcpListener(ipAddress, iPort);
                myListener = new TcpListener(IPAddress.Any, iPort);
                myListener.Start();
                Debug.WriteLine("Client-Server: Listener started.");
                socket = myListener.AcceptSocket();
                Debug.WriteLine("Client-Server: Socket Accepted.");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string getKey()
        {
            return this.sKey;
        }

        #endregion
    }
    
}
