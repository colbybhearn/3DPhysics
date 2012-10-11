using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Helper.Multiplayer.Packets;
using Helper.Collections;

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

        public bool Connect(IPEndPoint remoteEndPoint)
        {
            TcpClient client = new TcpClient();
            bool connected = false;
            try
            {
                client.Connect(remoteEndPoint);
                socket = new SocketComm(client.Client);
                socket.PacketReceived += new SocketComm.PacketReceivedEventHandler(socket_PacketReceived);
                socket.ClientDisconnected += new Handlers.voidEH(socket_ClientDisconnected);
                connected = true;
            }
            catch (Exception E)
            {
                connected = false;
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
            return connected;
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
            if(socket != null)
                socket.Disconnect();
        }

        public void Send(Packet packet)
        {
            Counter.AddTick("pps_out");
            //Counter.AddTick("average_pps_out", Counter.GetAverageValue("pps_out"));

            if (socket == null)
                return;

            socket.Send(packet.Serialize());
        }
    }
}
