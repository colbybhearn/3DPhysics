using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Helper.Communication;
using Helper.Multiplayer.Packets;
using Microsoft.Xna.Framework;

namespace Helper.Multiplayer
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
        TcpEventServer tcpServer;
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

            tcpServer = new TcpEventServer(ip, lobbyport);
            
            tcpServer.ClientAccepted += new TcpEventServer.ClientAcceptedEventHandler(listener_ClientAccepted);

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
            tcpServer.Start();
            tmrUpdateClients.Start();
            inputThread = new Thread(new ThreadStart(inputWorker));
            inputThread.Start();
        }

        public void Stop()
        {
            ShouldBeRunning = false;
            tcpServer.Stop();
            tmrUpdateClients.Stop();            
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
            ci.ClientDisconnected += new Helper.Handlers.StringEH(ci_ClientDisconnected);
            ci.Start();
            ci.PacketReceived += new ClientInfo.PacketReceivedEventHandler(PacketReceived);
            Clients.Add(id, ci);
            ClientInfoRequestPacket cirp = new ClientInfoRequestPacket(id);
            ci.Send(cirp);
        }

        void ci_ClientDisconnected(string alias)
        {
            BroadcastPacket(new ClientDisconnectPacket(alias));
        }


        private void inputWorker()
        {
            while (ShouldBeRunning)
            {
                while (InputQueue.Count > 0)
                {
                    ProcessInputPacket(InputQueue.Dequeue());
                }
                Thread.Sleep(1);
            }
        }

        #region Packet Receiving

        void PacketReceived(int id, Packet p)
        {
            if (!Clients.ContainsKey(id))
                return;
            ClientInfo ci = Clients[id];
            ClientPacketInfo cpi = new ClientPacketInfo(ref ci, p);
            InputQueue.Enqueue(cpi);
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
                CallChatMessageReceived(cp.message, cp.player);
                
            }
            else if (packet is ObjectRequestPacket)
            {
                ObjectRequestPacket corp = packet as ObjectRequestPacket;
                CallObjectRequestReceived(cpi.client.id,corp.AssetName);
            }
            else if (packet is ObjectUpdatePacket)
            {
                ObjectUpdatePacket oup = packet as ObjectUpdatePacket;
                CallObjectUpdateReceived(oup.objectId, oup.assetName, oup.position, oup.orientation, oup.velocity);
            }
            else if (packet is ObjectActionPacket)
            {
                ObjectActionPacket oap = packet as ObjectActionPacket;
                CallObjectActionReceived(oap.objectId, oap.actionParameters);
            }
            
        }

        public event Helper.Handlers.ObjectActionEH ObjectActionReceived;
        private void CallObjectActionReceived(int id, object[] parameters)
        {
            if (ObjectActionReceived == null)
                return;
            ObjectActionReceived(id, parameters);
        }

        public event Helper.Handlers.ObjectRequestEH ObjectRequestReceived;
        private void CallObjectRequestReceived(int clientId, string asset)
        {
            if (ObjectRequestReceived == null)
                return;
            ObjectRequestReceived(clientId, asset);
        }

        public event Helper.Handlers.StringStringEH ChatMessageReceived;
        private void CallChatMessageReceived(string msg, string player)
        {
            if (ChatMessageReceived == null)
                return;
            ChatMessageReceived(msg, player);
        }

        public event Helper.Handlers.StringEH ClientConnected;
        private void CallClientConnected(string alias)
        {
            if (ClientConnected == null)
                return;
            ClientConnected(alias);
        }

        public event Helper.Handlers.ObjectUpdateEH ObjectUpdateReceived;
        private void CallObjectUpdateReceived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            if (ObjectUpdateReceived == null)
                return;
            ObjectUpdateReceived(id, asset, pos, orient, vel);
        } 
        #endregion

        #region Packet Sending
        private void BroadcastPacket(Packet p)
        {
            foreach (ClientInfo ci in Clients.Values)
                ci.Send(p);
        }

        public void SendChatPacket(string msg, string player)
        {
            BroadcastPacket(new ChatPacket(msg, player));
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
        #endregion

        private int GetAvailableClientId()
        {
            int newId = 0;
            bool found = true;
            while (found)
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