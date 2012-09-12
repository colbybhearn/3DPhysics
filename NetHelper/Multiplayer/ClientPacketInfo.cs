using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer
{
    public class ClientPacketInfo
    {
        public ClientInfo client;
        public Packets.Packet packet;

        public ClientPacketInfo(ref ClientInfo ci, Packets.Packet p)
        {
            client = ci;
            packet = p;
        }
    }
}
