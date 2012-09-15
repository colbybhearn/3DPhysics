using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Helper.Multiplayer.Packets;
using Microsoft.Xna.Framework;
using Helper.Communication;

namespace Helper.Multiplayer
{
    public class CommClient
    {
        public int iPort;
        public string sAlias;
        IPAddress a;
        Helper.Communication.TcpEventClient client;
        ServerInfo Server;
        bool ShouldBeRunning = false;
        Queue<Packet> InputQueue = new Queue<Packet>();
        Thread inputThread;

        public CommClient(string ip, int port, string alias)
        {
            if (!IPAddress.TryParse(ip, out a))
                throw new ArgumentException("Unparsable IP");
            
            iPort = port;
            sAlias = alias;
            Server = new ServerInfo(new IPEndPoint(a, iPort));
        }

        public bool Connect()
        {
            Debug.WriteLine("Client: Connection " + Server.endPoint.Address.ToString() + " " + iPort);
            bool connected = false;
            try
            {
                ShouldBeRunning = true;
                inputThread = new Thread(new ThreadStart(inputWorker));
                inputThread.Start();
                client = new TcpEventClient();
                client.Connect(Server.endPoint);
                client.PacketReceived += new Helper.Handlers.PacketReceivedEH(PacketReceived);
                connected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Comm Client Error..... " + ex.Message);
                connected = false;
            }

            return connected;
        }

        void PacketReceived(Helper.Multiplayer.Packets.Packet p)
        {
            InputQueue.Enqueue(p);            
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

        private void ProcessInputPacket(Packet packet)
        {
            if (packet is ClientInfoRequestPacket)
            {
                ClientInfoResponsePacket clientInfoResponse = new ClientInfoResponsePacket(sAlias);
                client.Send(clientInfoResponse);
            }
            else if (packet is ChatPacket)
            {
                ChatPacket cp = packet as ChatPacket;
                CallChatMessageReceived(cp.message, cp.player);
            }
            else if (packet is ObjectResponsePacket)
            {
                ObjectResponsePacket corp = packet as ObjectResponsePacket;
                CallObjectRequestResponseReceived(corp.ID, corp.AssetName);
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
            else if (packet is ClientDisconnectPacket)
            {
                ClientDisconnectPacket cdp = packet as ClientDisconnectPacket;

                CallClientDisconnected(cdp.Alias);
            }
        }

        public event Handlers.StringEH ClientDisconnected;
        private void CallClientDisconnected(string alias)
        {
            if (ClientDisconnected == null)
                return;
            ClientDisconnected(alias);
        }

        public event Helper.Handlers.ObjectActionEH ObjectActionReceived;
        private void CallObjectActionReceived(int id, object[] parameters)
        {
            if (ObjectActionReceived == null)
                return;
            ObjectActionReceived(id, parameters);
        }


        public event Helper.Handlers.ObjectUpdateEH ObjectUpdateReceived;
        private void CallObjectUpdateReceived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            if (ObjectUpdateReceived == null)
                return;
            ObjectUpdateReceived(id, asset, pos, orient, vel);
        }

        public event Helper.Handlers.ObjectRequestResponseEH ObjectRequestResponseReceived;
        private void CallObjectRequestResponseReceived(int i, string asset)
        {
            if (ObjectRequestResponseReceived == null)
                return;
            ObjectRequestResponseReceived(i, asset);
        }

        
        public event Helper.Handlers.StringStringEH ChatMessageReceived;
        private void CallChatMessageReceived(string msg, string player)
        {
            if (ChatMessageReceived == null)
                return;
            ChatMessageReceived(msg, player);
        }


        public void Stop()
        {
            ShouldBeRunning = false;
            client.Stop();
            
        }

        public void SendChatPacket(string msg, string player)
        {
            // TODO, fix
            client.Send(new ChatPacket(msg, player));
        }

        public void SendObjectRequest(string assetname)
        {
            client.Send(new ObjectRequestPacket(assetname));
        }

        public void SendObjectUpdate(int id, Vector3 pos, Matrix orient, Vector3 vel)
        {
            client.Send(new ObjectUpdatePacket(id, string.Empty, pos, orient, vel));
        }

        public void SendObjectAction(int id, object[] actionvals)
        {
            client.Send(new ObjectActionPacket(id, actionvals));
        }
    }
}
