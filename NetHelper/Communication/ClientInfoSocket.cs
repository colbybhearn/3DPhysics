using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Helper.Communication
{
    public class ClientInfoSocket : SocketComm
    {
        // Using new keywords suppresses warnings about "hiding" the SocketComm's versions
        new public delegate void PacketReceivedEventHandler(int id, byte[] data);
        new public event PacketReceivedEventHandler PacketReceived;
        new public event Helper.Handlers.IntEH ClientDisconnected;
        public int ClientID;

        public ClientInfoSocket(Socket s, int id) : base(s)
        {
            ClientID = id;
        }

        protected override void CallClientDisconnected()
        {
            if (ClientDisconnected == null)
                return;
            ClientDisconnected(ClientID);
        }

        public override void CallPacketReceived(byte[] data)
        {
            if (PacketReceived == null)
                return;
            PacketReceived(ClientID, data);
        }
    }
}
