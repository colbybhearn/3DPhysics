using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Helper.Multiplayer;
using System.Net;
using Helper.Multiplayer.Packets;
using System.Threading;

namespace Multiplayer
{
    /*Vision
     * this will have a game
     * accept clients into the game.
     * send updates about a client to all other clients
     * accept client messages
     * process client messages
     * 
     * 
     * Server game is Reality
     * Client games are nearby realities
     * Clients update the server about their controlled objects only
     * Server updates client controlled objects
     * Server updates all clients about all objects
     * 
     * interpolation between realities may be needed to smooth out jitter
     * UDP packets may be needed to facilitate high update rates
     */
   

    public class GameServer
    {
        
        // list of client information with socket to communicate back
        SortedList<int, ClientInfo> Clients = new SortedList<int, ClientInfo>();
        TcpEventServer listener;
        private System.Timers.Timer tmrProcessClients;
        Queue<ClientPacketInfo> InputQueue = new Queue<ClientPacketInfo>();
        Thread inputThread;
        bool ShouldBeRunning = false;

        public GameServer(int lobbyport)
        {
            string a = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(a);
            IPAddress[] ips = ipEntry.AddressList;
            string ip = ips[0].ToString();

            listener = new TcpEventServer(ip, lobbyport);
            
            listener.ClientAccepted += new TcpEventServer.ClientAcceptedEventHandler(listener_ClientAccepted);

            tmrProcessClients = new System.Timers.Timer();
            tmrProcessClients.Interval = 200;
            tmrProcessClients.Elapsed += new System.Timers.ElapsedEventHandler(tProcessClientsTimer_Elapsed);
        }

        void tProcessClientsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (ClientInfo ci in Clients.Values)
            {
                switch (ci.connectionMode)
                {
                    case ClientInfo.ConnectionModes.Accepted:
                        
                        break;
                    case ClientInfo.ConnectionModes.ClientInfoRequested:
                        break;
                    case ClientInfo.ConnectionModes.Synced:
                        break;
                    default:
                        break;
                }

            }
            
        }



        public void Start()
        {
            ShouldBeRunning = true;
            listener.StartListening();
            tmrProcessClients.Start();
            inputThread = new Thread(new ThreadStart(inputWorker));
            inputThread.Start();
        }

        public void Stop()
        {
            foreach (ClientInfo ci in Clients.Values)
                ci.Stop();
        }

        void listener_ClientAccepted(Socket s)
        {
            int id = GetAvailableClientId();
            IPEndPoint ep = null;
            if(s.RemoteEndPoint is IPEndPoint)
                ep = s.RemoteEndPoint as IPEndPoint;
            
            ClientInfo ci = new ClientInfo(id, ep, string.Empty, s);
            ci.Start();
            ci.PacketReceived += new ClientInfo.PacketReceivedEventHandler(PacketReceived);
            Clients.Add(id, ci);
            ClientInfoRequestPacket cirp = new ClientInfoRequestPacket();
            ci.Send(cirp);
        }

        void PacketReceived(int id, Packet p)
        {
            if (!Clients.ContainsKey(id))
                return;
            ClientInfo ci = Clients[id];
            ClientPacketInfo cpi = new ClientPacketInfo(ref ci, p);
            InputQueue.Enqueue(cpi);
        }

        private void inputWorker()
        {
            while (ShouldBeRunning)
            {
                if (InputQueue.Count > 0)
                {
                    ProcessInputPacket(InputQueue.Dequeue());
                }
            }
        }

        private void ProcessInputPacket(ClientPacketInfo cpi)
        {
            Packet packet = cpi.packet;
            if (packet is ClientInfoResponsePacket)
            {
                ClientInfoResponsePacket cirp = packet as ClientInfoResponsePacket;
                cpi.client.alias = cirp.Alias;
                CallClientConnected(cpi.client.alias);
            }
        }

        public delegate void ClientConnectedEventHandler(string alias);
        public event ClientConnectedEventHandler ClientConnected;
        private void CallClientConnected(string alias)
        {
            if (ClientConnected == null)
                return;
            ClientConnected(alias);

        }

        private int GetAvailableClientId()
        {
            int newId = 0;
            bool found =true;
            while(found)
            {
                newId++;
                found = false;
                foreach (ClientInfo ci in Clients.Values)
                    if (ci.id == newId)
                    {
                        found = true;
                        break;
                    }
            }
            return newId;

        }


        
    }
}
