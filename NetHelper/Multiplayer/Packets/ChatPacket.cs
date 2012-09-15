using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ChatPacket : Packet
    {
        public string message;
        public string player;

        public ChatPacket(string msg, string name)
            : base(Types.Chat)
        {
            message = msg;
            player = name;
        }
    }
}
