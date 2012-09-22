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
        SocketComm socket;
        public event Helper.Handlers.PacketReceivedEH PacketReceived;
        public event Helper.Handlers.voidEH Disconnected;

        public TcpEventClient2()
        {

        }

        public void Connect(IPEndPoint remoteEndPoint)
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(remoteEndPoint);
                socket = new SocketComm(client.Client);
                socket.PacketReceived += new SocketComm.PacketReceivedEventHandler(socket_PacketReceived);
                socket.ClientDisconnected += new Handlers.voidEH(socket_ClientDisconnected);
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        void socket_PacketReceived(byte[] data)
        {
            Packet p = Packet.Read(data);
            if (p != null && PacketReceived != null)
                PacketReceived(p);
        }

        void socket_ClientDisconnected()
        {
            if(Disconnected == null)
                return;
            Disconnected();
        }

        public void Stop()
        {
            socket.Disconnect();
        }

        public void Send(Packet packet)
        {
            if (socket == null)
                return;

            socket.Send(packet.Serialize());
        }
    }
}
