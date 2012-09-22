using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ChatPacket : Packet
    {
        public string message;
        public int player;

        public ChatPacket(string msg, int pid)
            : base(Types.Chat)
        {
            message = msg;
            player = pid;
        }
    }
}
