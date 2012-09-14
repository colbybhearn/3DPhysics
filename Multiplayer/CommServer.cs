using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Helper.Multiplayer;
using System.Net;
using Helper.Multiplayer.Packets;
using System.Threading;
using Microsoft.Xna.Framework;

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
   

    public class CommServer
    {
        
        // list of client information with socket to communicate back
        SortedList<int, ClientInfo> Clients = new SortedList<int, ClientInfo>();
        TcpEventServer listener;
        private System.Timers.Timer tmrUpdateClients;
        Queue<ClientPacketInfo> InputQueue = new Queue<ClientPacketInfo>();
        Thread inputThread;
        bool ShouldBeRunning = false;

        public CommServer(int lobbyport)
        {
            string a = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(a);
            IPAddress[] ips = ipEntry.AddressList;
            string ip = ips[0].ToString();

            listener = new TcpEventServer(ip, lobbyport);
            
            listener.ClientAccepted += new TcpEventServer.ClientAcceptedEventHandler(listener_ClientAccepted);

            tmrUpdateClients = new System.Timers.Timer();
            tmrUpdateClients.Interval = 200;
            tmrUpdateClients.Elapsed += new System.Timers.ElapsedEventHandler(tProcessClientsTimer_Elapsed);
        }

        void tProcessClientsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (ClientInfo ci in Clients.Values)
            {               

            }
        }



        public void Start()
        {
            ShouldBeRunning = true;
            listener.StartListening();
            tmrUpdateClients.Start();
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
            if (cpi == null)
                return;
            Packet packet = cpi.packet;
            if (packet is ClientInfoResponsePacket)
            {
                ClientInfoResponsePacket cirp = packet as ClientInfoResponsePacket;
                cpi.client.alias = cirp.Alias;
                CallClientConnected(cpi.client.alias);
            }
            else if (packet is ChatPacket)
            {
                ChatPacket cp = packet as ChatPacket;
                SendChatPacket(cp.message, cp.player);
                CallChatMessageReceived(cp.message);
                
            }
            else if (packet is ObjectRequestPacket)
            {
                ObjectRequestPacket corp = packet as ObjectRequestPacket;
                CallObjectRequestReceived(cpi.client.id,corp.AssetName);
            }
            else if (packet is ObjectUpdatePacket)
            {
                ObjectUpdatePacket oup = packet as ObjectUpdatePacket;
                CallObjectUpdateRecived(oup.objectId, oup.assetName, oup.position, oup.orientation, oup.velocity);
            }
            
        }

        public event Helper.Handlers.ObjectRequestEH ObjectRequestReceived;
        private void CallObjectRequestReceived(int clientId, string asset)
        {
            if (ObjectRequestReceived == null)
                return;
            ObjectRequestReceived(clientId, asset);
        }

        public event Helper.Handlers.StringEH ChatMessageReceived;
        private void CallChatMessageReceived(string msg)
        {
            if (ChatMessageReceived == null)
                return;
            ChatMessageReceived(msg);
        }

        private void BroadcastPacket(Packet p)
        {
            foreach (ClientInfo ci in Clients.Values)
                ci.Send(p);            
        }

        public event Helper.Handlers.StringEH ClientConnected;
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


        public event Helper.Handlers.ObjectUpdateEH ObjectUpdateReceived;
        private void CallObjectUpdateRecived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            if (ObjectUpdateReceived == null)
                return;
            ObjectUpdateReceived(id, asset, pos, orient, vel);
        }

        public void SendChatPacket(string msg, string player)
        {
            BroadcastPacket(new ChatPacket(msg, "Someone Else"));
        }

        public void SendObjectResponsePacket(int clientid, int objectId, string asset)
        {
            if (!Clients.ContainsKey(clientid))
                return;
            Clients[clientid].Send(new ObjectResponsePacket(objectId, asset));
        }

        public void BroadcastObjectUpdate(Packet p)
        {
            BroadcastPacket(p);
        }
    }
}
