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
            socket = s;
            inputThread = new Thread(new ThreadStart(inputWorker));
            outputThread = new Thread(new ThreadStart(outputWorker));
            inputThread.Start();
            outputThread.Start();
            DataToSendQueue = new ThreadQueue<byte[]>();
        }

        public void Disconnect()
        {
            ShouldBeRunning = false;
        }

        public void Send(byte[] data)
        {
            DataToSendQueue.Enqueue(data);
        }

        private void inputWorker()
        {
            List<byte[]> dataToSend = new List<byte[]>();
            byte[] lenBytes = new byte[4];
            int length = -1;
            
            while (ShouldBeRunning)
            {
                if (!socket.Connected)
                    break;

                if (length == -1 && socket.Available>=4)
                {
                    int count = socket.Receive(lenBytes);
                    length = BitConverter.ToInt32(lenBytes, 0);
                    //Debug.WriteLine(length);
                    if (length > 5000)
                        throw new FormatException("packet length "+length+" is unreasonably long.");
                }
                
                if (length > 0 && socket.Available>=length)
                {
                    byte[] data = new byte[length];
                    int datacount = socket.Receive(data);
                    
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

            while (ShouldBeRunning)
            {
                if (!socket.Connected)
                    break;

                dataToSend.Clear();
                while (DataToSendQueue.Count > 0)
                    dataToSend.AddRange(DataToSendQueue.DeQ());
                try
                {
                    //Send ALL bytes at once instead of "per packet", should be better
                    socket.Send(dataToSend.ToArray());
                }
                catch (SocketException E)
                {
                    ShouldBeRunning = false;
                    System.Diagnostics.Debug.WriteLine(E.StackTrace);
                }
                Thread.Sleep(1);
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
