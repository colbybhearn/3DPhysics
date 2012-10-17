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
        Thread inputProcThread;
        
        public delegate void PacketReceivedEventHandler(byte[] data);
        public event PacketReceivedEventHandler PacketReceived;
        public event Helper.Handlers.voidEH ClientDisconnected;

        bool ShouldBeRunning = false;
        ThreadQueue<byte[]> DataReceived = new ThreadQueue<byte[]>();
        ThreadQueue<byte[]> DataToSendQueue;

        public SocketComm(Socket s, bool clientSide, string socketAlias)
        {
            ShouldBeRunning = true;
            DataToSendQueue = new ThreadQueue<byte[]>();
            socket = s;
            inputThread = new Thread(new ThreadStart(inputWorker));
            outputThread = new Thread(new ThreadStart(outputWorker));
            inputProcThread = new Thread(new ThreadStart(inputProcWorker));
            if (clientSide)
            {
                inputThread.Name = "Client Socket Input Worker (" + socketAlias + ")";
                outputThread.Name = "Client Socket Input Worker (" + socketAlias + ")";
            }
            else
            {
                inputThread.Name = "Server Socket Input Worker (" + socketAlias + ")";
                outputThread.Name = "Server Socket Input Worker (" + socketAlias + ")";
            }
            
            inputThread.Start();
            outputThread.Start();
            inputProcThread.Start();
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
            byte[] lenBytes = new byte[4];
            int length = -1;

            int packetSizeCount = 0;

            int packetSizeSum = 0;
            int packetSizeAvg = 0;
            int second = 0;
            int printed = 0;
            int count = 0;
            DateTime pre = new DateTime();
            socket.ReceiveBufferSize = 1000000;
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

                if (length > 0 && socket.Available >= length)
                {
                    byte[] data = new byte[length];

                    int datacount = socket.Receive(data);
                    /*
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
                    }*/

                    if (data != null)
                        CallPacketReceived(data);
                    length = -1;
                }
                else
                {
                    if (inputThread.Name.Contains("Client"))
                    {
                        //Debug.WriteLine("Client socketcomm sleeping");
                    }
                    Thread.Sleep(1);
                }
            }

            CallClientDisconnected();
            socket.Disconnect(false);
        }

        private void fasterTcpInputWorker()
        {
            int length = 1024;
            int count = 0;
            socket.ReceiveBufferSize = 1000000;
            byte[] data = new byte[length];
            socket.Blocking = true;
            while (ShouldBeRunning)
            {
                try
                {
                    if (!socket.Connected)
                        break;
                    count = socket.Receive(data);
                    if (data != null)
                    {
                        byte[] dr = new byte[count];
                        Array.Copy(data, 0, dr, 0, count);
                        CallDataReceived(dr);
                    }
                }
                catch (Exception E)
                {
                    Trace.WriteLine(E.Message);
                }
            }

            CallClientDisconnected();
            socket.Disconnect(false);
        }
        
        private void CallDataReceived(byte[] b)
        {
            DataReceived.EnQ(b);
        }

        private void inputProcWorker()
        {
            byte[] aPacket;
            byte[] lenBytes = new byte[4];
            byte[] data = new byte[0];
            int packetLength = -1;
            byte[] oldData = new byte[0];
            while (ShouldBeRunning)
            {
                while (DataReceived.Count > 0)
                {
                    try
                    {
                        byte[] inData = DataReceived.DeQ() as byte[];
                        int requiredLength = oldData.Length + inData.Length;
                        //if (data.Length < requiredLength)
                            data = new byte[requiredLength]; // make data large enough to contain old and new
                        Array.Copy(oldData, 0, data, 0, oldData.Length); // copy the existing data into the resized array, at the front
                        Array.Copy(inData, 0, data, oldData.Length, inData.Length); // copy the newly dequeued data into the resized array, at the end.

                        if (packetLength == -1 && // if the length of the next packet is unknown, and
                            data.Length >= 4) // we have that much data on hand,
                        {
                            Array.Copy(data, lenBytes, 4); // copy
                            packetLength = BitConverter.ToInt32(lenBytes, 0);
                            if (packetLength <= 0 || packetLength > 5000)
                                throw new FormatException("fast packet length " + packetLength + " is unreasonably long.");
                        }

                        if (packetLength != -1 &&  // we know how much data goes in the next packet, and
                            data.Length >= packetLength + 4) // we have that much data on hand,
                        {
                            //Trace.WriteLine("fast Processing " + packetLength + " / " + data.Length);
                            aPacket = new byte[packetLength]; // make a packet buffer
                            Array.Copy(data, 4, aPacket, 0, packetLength); // copy the data for this packet into the packet buffer
                            CallPacketReceived(aPacket); // send this packet off for processing
                            int unCopiedBytes = data.Length - (packetLength + 4); // calculate bytes that were not in that packet (length of packet + 4 byte length header)
                            oldData = new byte[unCopiedBytes];
                            Array.Copy(data, packetLength + 4, oldData, 0, unCopiedBytes); // shift the uncopied bytes up to the front (for queue-like behavior)
                            packetLength = -1; // signal the unknown length of the next packet
                        }
                    }
                    catch(Exception E)
                    {
                        Trace.WriteLine(E.Message);
                    }
                }
                //Trace.WriteLine("sleeping");
                Thread.Sleep(10);
            }
        }

        private void BytesToString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));
            //inputWorker.Trace.WriteLine(inputThread.Name +": "+bytes.Length +" : "+sb.ToString());
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
                    //packetSizeCount++;
                    //packetSizeSum += DataToSendQueue.Peek().Length;
                    dataToSend.AddRange(DataToSendQueue.DeQ());
                }                

                if (dataToSend.Count == 0)
                    continue;

                //packetSizeAvg = packetSizeSum / packetSizeCount;
                
                try
                {
                    //DateTime pre = DateTime.Now;
                    //Send ALL bytes at once instead of "per packet", should be better
                    int r = socket.Send(dataToSend.ToArray());
                    //DateTime post = DateTime.Now;
                    sent += r;
                    
                    //double ts = (post - pre).TotalSeconds;
                    //Trace.WriteLine("SocketComm output bytes actually sent: " + sent + " took " +ts + " seconds, Avg Packet Size:" + packetSizeAvg+ ", Packet Count"+packetSizeCount + " Rate="+(float)r/ts +"Bps");
                    //Trace.WriteLine("SocketComm output bytes actually sent: " + sent );
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
            //BytesToString(data);
            if (PacketReceived == null)
                return;
            PacketReceived(data);
        }
    }
}
