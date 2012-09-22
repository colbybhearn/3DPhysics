using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Helper.Multiplayer.Packets;

namespace Helper.Communication
{
    public class TcpEventClient
    {        
        Thread readThread;
        bool ShouldBeRunning = false;
        Socket socket;
        Helper.Collections.ThreadQueue DataToSendQueue = new Helper.Collections.ThreadQueue();

        public TcpEventClient()
        {
            readThread = new Thread(new ThreadStart(readWorker));
            DataToSendQueue = new Collections.ThreadQueue();
        }

        public void Connect(IPEndPoint remoteEndPoint)
        {
            bool connected = false;
            TcpClient client = new TcpClient();
            client.NoDelay = true; // Test
            try
            {
                client.Connect(remoteEndPoint);
                socket = client.Client;
                connected = true;
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
            if (connected)
            {
                ShouldBeRunning = true;
                readThread.Start();
            }

        }

        public void Stop()
        {
            ShouldBeRunning = false;
        }


        private void readWorker()
        {
            List<byte[]> dataToSend = new List<byte[]>();
            byte[] lenBytes = new byte[4];
            int length = -1;
            while (ShouldBeRunning)
            {
                try
                {
                    if (socket == null)
                        continue;

                    // if there are enough bytes to know how many bytes make the next packet,
                    if (length == -1 && socket.Available >= 4)
                    {
                        int count = socket.Receive(lenBytes);
                        length = BitConverter.ToInt32(lenBytes, 0);
                        if (length > 10000)
                            throw new FormatException("packet length is unreasonably long");
                    }
                    else if (length>0 && socket.Available>=length)
                    {
                        //System.Diagnostics.Debug.WriteLine(socket.Available / 107 + " packets queued");
                        byte[] data = new byte[length];
                        int datacount = socket.Receive(data);

                        Packet p = Packet.Read(data);
                        if (p != null)
                            CallPacketReceived(p);
                        length = -1;
                    }

                    dataToSend.Clear();
                    // now a ThreadQueue
                    while (DataToSendQueue.Count > 0)
                        dataToSend.Add(DataToSendQueue.DeQ() as byte[]);
                    foreach(byte[] b in dataToSend)
                        socket.Send(b);
                }
                catch (Exception e)
                {
                    // 2012.09.15   Colby got the following exception (actually showed in commClient at "inputQueue.Enqueue(p)" originally)
                    // couldn't reproduce that the time
                    //Argument Excpetion: Source array was not long enough. Check srcIndex and length, and the array's lower bounds.
                    Debug.WriteLine(e.StackTrace);
                }
                Thread.Sleep(0);
            }
        }

        
        public event Helper.Handlers.PacketReceivedEH PacketReceived;
        private void CallPacketReceived(Packet p)
        {
            if(PacketReceived!=null)
            {
                PacketReceived(p);
            }
        }

        public void Send(Packet packet)
        {
            if (socket == null)
                return;
            byte[] data = packet.Serialize();
            //stream.Write(data, 0, data.Length);
            DataToSendQueue.EnQ(data);
        }
    }
}
