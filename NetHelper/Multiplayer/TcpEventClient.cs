using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using Helper.Multiplayer.Packets;

namespace Helper.Multiplayer
{
    public class TcpEventClient
    {

        
        Thread readThread;
        bool ShouldBeRunning = false;
        NetworkStream stream;

        public TcpEventClient()
        {
            readThread = new Thread(new ThreadStart(readWorker));

        }

        public void Connect(IPEndPoint remoteEndPoint)
        {
            ShouldBeRunning = true;
            readThread.Start();
            TcpClient client = new TcpClient();
            client.Connect(remoteEndPoint);
            stream = client.GetStream();

        }

        public void Stop()
        {
            ShouldBeRunning = false;
        }


        private void readWorker()
        {
            while (ShouldBeRunning)
            {
                try
                {
                    if (stream == null)
                    {
                        continue;
                    }
                    // if there are enough bytes to know how many bytes make the next packet,
                    if (stream.DataAvailable)
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
                        Packet p = Packet.Deserialize(data);
                        
                        if(p!=null)
                            // hand the packet off and get back to work
                            CallPacketsReceived(p);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                Thread.Sleep(10);
            }
        }

        
        public event Helper.Handlers.PacketReceivedEH PacketReceived;
        private void CallPacketsReceived(Packets.Packet p)
        {
            if(PacketReceived!=null)
            {
                PacketReceived(p);
            }
        }

        public void Send(Packet packet)
        {
            byte[] data = packet.Serialize();
            stream.Write(data, 0, data.Length);

        }
    }
}
