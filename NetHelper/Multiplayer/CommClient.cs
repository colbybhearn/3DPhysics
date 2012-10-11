using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Helper.Multiplayer.Packets;
using Microsoft.Xna.Framework;
using Helper.Communication;
using Helper.Collections;

namespace Helper.Multiplayer
{
    public class CommClient
    {
        public int iPort;
        public string sAlias;
        IPAddress a;
        Helper.Communication.TcpEventClient2 client;
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
                client = new TcpEventClient2();
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
                int count = 0; // Using this since its possible one can get added to the queue while another is being processed
                while (InputQueue.Count > 0)                    
                {
                    ProcessInputPacket(InputQueue.Dequeue());
                    count++;
                }
                if (count > 0)
                {
                    Counter.AddTick("pps_in", count);
                    //Counter.AddTick("average_pps_in", Counter.GetAverageValue("pps_in"));
                }

                Thread.Sleep(1);
            }
        }

        private void ProcessInputPacket(Packet packet)
        {
            if (packet is ClientInfoRequestPacket)
            {
                ClientInfoRequestPacket cir = packet as ClientInfoRequestPacket;
                ClientInfoResponsePacket clientInfoResponse = new ClientInfoResponsePacket(sAlias);
                client.Send(clientInfoResponse);
                CallClientInfoRequestReceived(cir.ID);
            }
            else if (packet is ChatPacket)
            {
                ChatPacket cp = packet as ChatPacket;
                CallChatMessageReceived(new ChatMessage(cp.message, cp.player));
            }
            else if (packet is ObjectAddedPacket)
            {
                ObjectAddedPacket corp = packet as ObjectAddedPacket;
                CallObjectRequestResponseReceived(corp.Owner, corp.ID, corp.AssetName);
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

                CallDisconnectedFromServer(cdp.id);
            }
            else if (packet is ClientConnectedPacket)
            {
                ClientConnectedPacket ccp = packet as ClientConnectedPacket;

                CallConnectedToServer(ccp.ID, ccp.Alias);
            }
            else if (packet is ObjectAttributePacket)
            {
                ObjectAttributePacket oap = packet as ObjectAttributePacket;
                CallObjectAttributeReceived(oap);
            }
            else if (packet is ObjectDeletedPacket)
            {
                ObjectDeletedPacket odp = packet as ObjectDeletedPacket;
                CallObjectDeleteReceived(odp);
            }
        }

        public event Handlers.IntEH ObjectDeleteReceived;
        private void CallObjectDeleteReceived(ObjectDeletedPacket odp)
        {
            if (ObjectDeleteReceived == null)
                return;
            ObjectDeleteReceived(odp.objectId);
        }

        public event Handlers.ObjectAttributeEH ObjectAttributeReceived;
        private void CallObjectAttributeReceived(ObjectAttributePacket oap)
        {
            if (ObjectAttributeReceived == null)
                return;
            ObjectAttributeReceived(oap);
        }

        public event Handlers.ClientConnectedEH ConnectedToServer;
        private void CallConnectedToServer(int id, string alias)
        {
            if(ConnectedToServer == null)
                return;
            ConnectedToServer(id, alias);
        }

        public event Handlers.IntEH DisconnectedFromServer;
        private void CallDisconnectedFromServer(int id)
        {
            if (DisconnectedFromServer == null)
                return;
            DisconnectedFromServer(id);
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

        public event Helper.Handlers.ObjectAddedResponseEH ObjectAddedReceived;
        private void CallObjectRequestResponseReceived(int owner, int id, string asset)
        {
            if (ObjectAddedReceived == null)
                return;
            ObjectAddedReceived(owner, id, asset);
        }

        
        public event Helper.Handlers.ChatMessageEH ChatMessageReceived;
        private void CallChatMessageReceived(ChatMessage cm)
        {
            if (ChatMessageReceived == null)
                return;
            ChatMessageReceived(cm);
        }

        public event Helper.Handlers.IntEH ClientInfoRequestReceived;
        private void CallClientInfoRequestReceived(int id)
        {
            if (ClientInfoRequestReceived == null)
                return;
            ClientInfoRequestReceived(id);
        }


        public void Stop()
        {
            ShouldBeRunning = false;
            client.Stop();
            
        }

        public void SendChatPacket(string msg, int player)
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
