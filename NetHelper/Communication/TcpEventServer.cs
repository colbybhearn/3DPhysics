using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using Helper.Multiplayer.Packets;

namespace Helper.Communication
{
    public class TcpEventServer
    {
        /* Vision
         * One instance of the TcpEventServer handles the lobby, and thus the ClientConnected will hand off to a function to connect them to the correct "server"
         * A second instance of the TcpEventServer will also accept connections, but this time it will handle them appropriately via SocketComm and all its goodness
         * 
         * TcpEventServer will keep a list of "connected clients" (was ClientInfo.cs)
         */

        #region Properties
        string sIPAddress;
        int iPort;
        bool ShouldBeRunning=false;
        IPAddress ipAd;
        TcpListener myListener;
        Thread ServerListener;
        SortedList<int, ClientInfoSocket> Clients = new SortedList<int, ClientInfoSocket>();
        public event Helper.Handlers.IntPacketEH PacketReceived;

        #endregion

        #region Initialization

        public TcpEventServer( string IP, int port)
        {
            ipAd = IPAddress.Parse(IP);
            this.iPort = port;
            ServerListener = new Thread(new ThreadStart(ServerWorker));
            
        }
        public void Start()
        {
            this.ShouldBeRunning = true;
            ServerListener.Start();
            Debug.WriteLine("Lobby Listener: ServerListener.Start() finished: " + iPort.ToString());
        }
        public void Stop()
        {
            this.ShouldBeRunning = false;
            foreach (ClientInfoSocket s in Clients.Values)
                s.Disconnect();
        }

        #endregion

        private void ServerWorker()
        {
            try
            {
                // use local m/c IP address, and 
                Debug.WriteLine("Lobby Listener: creating Listener");

                // Initializes the Listener 
                // myList = new TcpListener(ipAd, iPort);
                myListener = new TcpListener(IPAddress.Any, iPort);
                Debug.WriteLine("Lobby Listener: starting Listener");
                // Start Listeneting at the specified port         
                myListener.Start();
                Debug.WriteLine("Lobby Listener: Waiting to Accept on: " + iPort.ToString());

                // poll for pending connections
                while (ShouldBeRunning)
                {
                    if (myListener.Pending())
                    {
                        Socket socket = myListener.AcceptSocket();
                        socket.NoDelay = true;
                        CallClientAccepted(socket);
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                string s = "LobbyListener Error: " + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(s);
            }
               myListener.Stop();
        }
        
        //public delegate void SocketEH(Socket s);
        public event Helper.Handlers.IntEH ClientAccepted;
        int nextClientId = 0;
        public void CallClientAccepted(Socket s)
        {

            nextClientId++;

            ClientInfoSocket socket = new ClientInfoSocket(s, nextClientId);
            socket.ClientDisconnected += new Handlers.IntEH(ClientDisconnected);
            socket.PacketReceived += new ClientInfoSocket.PacketReceivedEventHandler(Receive);
            Clients.Add(nextClientId, socket);
            //ClientInfoRequestPacket cirp = new ClientInfoRequestPacket(id);
            //ci.Send(cirp);

            if (ClientAccepted != null)
                ClientAccepted(nextClientId);
        }

        public void ClientDisconnected(int id)
        {
            // TODO
        }

        public void Send(Packet p)
        {
            // To everyone
            byte[] data = p.Serialize();
            foreach (SocketComm s in Clients.Values)
            {
                if (s != null)
                    s.Send(data);
            }
        }

        public void Send(Packet p, int id)
        {
            // To Specific id
            ClientInfoSocket s;
            if(Clients.TryGetValue(id, out s))
            {
                if(s != null)
                    s.Send(p.Serialize());
            }
        }

        public void Receive(int id, byte[] data)
        {
            Packet p = Packet.Read(data);
            if (p != null && PacketReceived != null)
                PacketReceived(id, p);
        }
    }
}
