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
        NetworkStream stream;
        Queue<byte[]> DataToSendQueue;

        public TcpEventClient()
        {
            readThread = new Thread(new ThreadStart(readWorker));
            DataToSendQueue = new Queue<byte[]>();

        }

        public void Connect(IPEndPoint remoteEndPoint)
        {
            bool connected = false;
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(remoteEndPoint);
                stream = client.GetStream();
                connected = true;
            }
            catch (Exception E)
            {                
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
            while (ShouldBeRunning)
            {
                try
                {
                    if (stream == null)
                    {
                        continue;
                    }
                    // if there are enough bytes to know how many bytes make the next packet,
                    while (stream.DataAvailable)
                    {

                        //if(stream.
                        byte[] lenBytes = new byte[4];
                        // read in the packet length bytes (4 of them)
                        stream.Read(lenBytes, 0, 4);
                        // convert the bytes into an integer
                        int length = BitConverter.ToInt32(lenBytes, 0);
                        if (length > 10000)
                            throw new FormatException("packet length is unreasonably long");
                        
                        // wait for that many more bytes of pure golden-brown data goodness
                        //while (stream.Length < length)
                            // Take a power nap
                            //Thread.Sleep(3);
                                                
                        byte[] data = new byte[length];
                        // read in the packet data
                        stream.Read(data, 0, length);
                        // deserialize the packet data into a packet
                        Packet p = Packet.Read(data);                        
                        if(p!=null)
                            // hand the packet off and get back to work
                            CallPacketsReceived(p);
                    }

                    dataToSend.Clear();
                    lock (DataToSendQueue)
                    {
                        while (DataToSendQueue.Count > 0)
                            dataToSend.Add(DataToSendQueue.Dequeue());
                    }
                    foreach(byte[] b in dataToSend)
                        stream.Write(b, 0, b.Length);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                Thread.Sleep(10);
            }
        }

        
        public event Helper.Handlers.PacketReceivedEH PacketReceived;
        private void CallPacketsReceived(Packet p)
        {
            if(PacketReceived!=null)
            {
                PacketReceived(p);
            }
        }

        public void Send(Packet packet)
        {
            if (stream == null)
                return;
            byte[] data = packet.Serialize();
            //stream.Write(data, 0, data.Length);
            DataToSendQueue.Enqueue(data);
        }
    }
}
