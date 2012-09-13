using Helper.Multiplayer.Packets;


namespace Helper
{
    public static class Handlers
    {
        public delegate void StringEH(string s);
        public delegate void PacketReceivedEH(Packet p);

    }
}
