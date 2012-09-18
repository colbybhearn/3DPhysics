using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Helper.Multiplayer.Packets;
using System.Collections.Generic;

namespace Helper.Multiplayer
{
    public class ClientInfo
    {
        public IPEndPoint endPoint;
        public string alias;
        public int id;
        public Socket socket;
        public enum ConnectionModes
        {
            Accepted,
            ClientInfoRequested,
            Synced,
            Disconnected
        }

        Thread inputThread;

        public delegate void PacketReceivedEventHandler(int id, Packet p);
        public event PacketReceivedEventHandler PacketReceived;

        public ConnectionModes connectionMode;
        bool ShouldBeRunning = false;
        Queue<byte[]> DataToSendQueue;

        public ClientInfo(int id, IPEndPoint ep, string alias, Socket s)
        {
            this.id = id;
            this.endPoint = ep;
            this.alias = alias;
            this.socket = s;
            connectionMode = ConnectionModes.Accepted;
            DataToSendQueue = new Queue<byte[]>();
        }

        public void Start()
        {
            ShouldBeRunning = true;
            inputThread = new Thread(new ThreadStart(inputWorker));
            inputThread.Start();
        }
        public void Stop()
        {
            ShouldBeRunning = false;
        }

        public void Send(Packet p)
        {
            lock (DataToSendQueue)
            {
                DataToSendQueue.Enqueue(p.Serialize());
            }
            //socket.SendAsync(p.Serialize());            
        }

        private void inputWorker()
        {
            int length = -1;
            byte[] lenBytes = new byte[4];
            List<byte[]> dataToSend = new List<byte[]>();
            while (ShouldBeRunning)
            {
                if (length == -1 && socket.Available>=4)
                {
                    int count = socket.Receive(lenBytes);
                    length = BitConverter.ToInt32(lenBytes, 0);
                    if (length > 10000)
                        throw new FormatException("packet length is unreasonably long");
                }
                else if (length>0 && socket.Available>=length)
                {
                    byte[] data = new byte[length];
                    int datacount = socket.Receive(data);

                    Packet p = Packet.Read(data);
                    if (p != null)
                        CallPacketReceived(p);
                    length = -1;
                }

                dataToSend.Clear();
                lock (DataToSendQueue)
                {
                    while (DataToSendQueue.Count > 0)
                        dataToSend.Add(DataToSendQueue.Dequeue());
                }
                if (!socket.Connected)
                    continue;
                try
                {
                    foreach (byte[] b in dataToSend)
                        socket.Send(b);
                }
                catch(SocketException E)
                {
                    ShouldBeRunning = false;
                    connectionMode = ConnectionModes.Disconnected;
                    System.Diagnostics.Debug.WriteLine(E.StackTrace);
                    // no longer connected.
                }
                Thread.Sleep(10);
            }

            if (connectionMode == ConnectionModes.Disconnected)
            {
                CallClientDisconnected();
            }
        }

        private void CallClientDisconnected( )
        {
            if (ClientDisconnected == null)
                return;
            ClientDisconnected(this.alias);
        }
        public event Helper.Handlers.StringEH ClientDisconnected;

        public void CallPacketReceived(Packet p)
        {
            if (PacketReceived == null)
                return;
            PacketReceived(id, p);
        }
    }
}
