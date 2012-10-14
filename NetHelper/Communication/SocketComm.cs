using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using Helper.Collections;
using System.Net;
using System.Diagnostics;

namespace Helper.Communication
{
    public class SocketComm
    {
        Socket socket;
        Thread inputThread;
        Thread outputThread;
        //IPEndPoint endPoint;
        public delegate void PacketReceivedEventHandler(byte[] data);
        public event PacketReceivedEventHandler PacketReceived;
        public event Helper.Handlers.voidEH ClientDisconnected;

        bool ShouldBeRunning = false;
        ThreadQueue<byte[]> DataToSendQueue;

        public SocketComm(Socket s)
        {
            ShouldBeRunning = true;
            DataToSendQueue = new ThreadQueue<byte[]>();
            socket = s;
            inputThread = new Thread(new ThreadStart(inputWorker));
            outputThread = new Thread(new ThreadStart(outputWorker));
            
            inputThread.Start();
            outputThread.Start();            
        }

        public void Disconnect()
        {
            ShouldBeRunning = false;
        }

        public void Send(byte[] data)
        {
            DataToSendQueue.EnQ(data);
        }

        private void inputWorker()
        {
            List<byte[]> dataToSend = new List<byte[]>();
            byte[] lenBytes = new byte[4];
            int length = -1;

            int packetSizeCount = 0;

            int packetSizeSum = 0;
            int packetSizeAvg = 0;
            int second = 0;
            int printed = 0;
            int count = 0;
            DateTime pre = new DateTime();
            while (ShouldBeRunning)
            {
                if (!socket.Connected)
                    break;

                if (length == -1 && socket.Available >= 4)
                {
                    pre = DateTime.Now;
                    count = socket.Receive(lenBytes);
                    length = BitConverter.ToInt32(lenBytes, 0);

                    //Trace.WriteLine("Reading " + length + " / " + socket.Available);
                    if (length > 5000)
                        throw new FormatException("packet length " + length + " is unreasonably long.");
                }
                else
                {
                }
                
                if (length > 0 && socket.Available>=length)
                {
                    byte[] data = new byte[length];
                    
                    int datacount = socket.Receive(data);
                    DateTime post = DateTime.Now;

                    packetSizeSum += datacount + count;
                    packetSizeCount++;

                    second = DateTime.Now.Second;
                    if (second != printed)
                    {
                        double ts = (post - pre).TotalSeconds;
                        if (ts == 0)
                            ts = .00001;
                        packetSizeAvg = packetSizeSum / packetSizeCount;
                        //Trace.WriteLine("SocketComm input bytes received: " + packetSizeSum + " Avg Packet Size:" + packetSizeAvg + ", Packet Count" + packetSizeCount + " took " + ts + " seconds, Rate=" + (float)packetSizeSum / ts + "Bps");
                        packetSizeCount = 0;
                        packetSizeSum = 0;
                        printed = second;
                    }


                    if (data != null)
                        CallPacketReceived(data);
                    length = -1;
                }
                Thread.Sleep(1);
            }

            CallClientDisconnected();
            socket.Disconnect(false);
        }

        private void outputWorker()
        {
            List<byte> dataToSend = new List<byte>();
            int sent = 0;

            int packetSizeCount=0;
            int packetSizeSum=0;
            int packetSizeAvg=0;
            while (ShouldBeRunning)
            {
                Thread.Sleep(1);
                if (!socket.Connected)
                    break;

                dataToSend.Clear();

                packetSizeSum = 0;
                packetSizeCount = 0;
                while (DataToSendQueue.Count > 0)
                {
                    packetSizeCount++;
                    packetSizeSum += DataToSendQueue.Peek().Length;
                    dataToSend.AddRange(DataToSendQueue.DeQ());
                }                

                if (dataToSend.Count == 0)
                    continue;

                packetSizeAvg = packetSizeSum / packetSizeCount;
                
                try
                {
                    DateTime pre = DateTime.Now;
                    //Send ALL bytes at once instead of "per packet", should be better
                    int r = socket.Send(dataToSend.ToArray());
                    DateTime post = DateTime.Now;
                    sent += r;
                    
                    double ts = (post - pre).TotalSeconds;
                    //Trace.WriteLine("SocketComm output bytes actually sent: " + sent + " took " +ts + " seconds, Avg Packet Size:" + packetSizeAvg+ ", Packet Count"+packetSizeCount + " Rate="+(float)r/ts +"Bps");
                    sent = 0;

                }
                catch (Exception E)
                {
                    System.Diagnostics.Debug.WriteLine(E.StackTrace);
                }
            }
        }

        protected virtual void CallClientDisconnected()
        {
            if (ClientDisconnected == null)
                return;
            ClientDisconnected();
        }

        public virtual void CallPacketReceived(byte[] data)
        {
            if (PacketReceived == null)
                return;
            PacketReceived(data);
        }
    }
}
