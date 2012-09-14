using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Helper.Multiplayer;
using Helper.Multiplayer.Packets;
using Microsoft.Xna.Framework;
using Helper;


namespace Multiplayer
{
    public class CommClient
    {
        public int iPort;
        public string sAlias;
        IPAddress a;
        TcpEventClient client;
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

        public void Connect()
        {
            Debug.WriteLine("Client: Connection " + Server.endPoint.Address.ToString() + " " + iPort);
            try
            {
                ShouldBeRunning = true;
                inputThread = new Thread(new ThreadStart(inputWorker));
                inputThread.Start();
                client = new TcpEventClient();
                client.Connect(Server.endPoint);
                client.PacketReceived += new Helper.Handlers.PacketReceivedEH(PacketReceived);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error..... " + ex.Message);
            }

        }

        void PacketReceived(Helper.Multiplayer.Packets.Packet p)
        {
            InputQueue.Enqueue(p);            
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
                CallChatMessageReceived(cp.message);
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

        
        public event Helper.Handlers.StringEH ChatMessageReceived;
        private void CallChatMessageReceived(string msg)
        {
            if (ChatMessageReceived == null)
                return;
            ChatMessageReceived(msg);
        }


        public void Stop()
        {
            ShouldBeRunning = false;
        }

        public void SendChatPacket(string msg, string player)
        {
            client.Send(new ChatPacket(msg, "Someone Else"));
        }

        public void SendObjectRequest(string assetname)
        {
            client.Send(new ObjectRequestPacket(assetname));
        }

        public void SendObjectUpdate(int id, Vector3 pos, Matrix orient, Vector3 vel)
        {
            client.Send(new ObjectUpdatePacket(id, string.Empty, pos, orient, vel));
        }
    }
}
