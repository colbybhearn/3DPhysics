using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Helper.Multiplayer.Packets;

namespace Helper.Communication
{
    class TcpEventClient2
    {           
        bool ShouldBeRunning = false;
        SocketComm socket;
        Helper.Collections.ThreadQueue DataToSendQueue = new Helper.Collections.ThreadQueue();

        public TcpEventClient2()
        {
            DataToSendQueue = new Collections.ThreadQueue();
        }

        public void Connect(IPEndPoint remoteEndPoint)
        {
            socket = new SocketComm();
            socket.PacketReceived += new SocketComm.PacketReceivedEventHandler(socket_PacketReceived);
            socket.ClientDisconnected += new Handlers.voidEH(socket_ClientDisconnected);
            socket.Connect(remoteEndPoint);
        }

        void socket_PacketReceived(byte[] data)
        {
            
        }

        void socket_ClientDisconnected()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            ShouldBeRunning = false;
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
