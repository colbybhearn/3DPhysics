using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ChatPacket : Packet
    {
        public string message;

        public ChatPacket(string msg)
            : base(Types.Chat)
        {
            message = msg;
        }
    }
}
