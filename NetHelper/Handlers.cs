using Helper.Multiplayer.Packets;


namespace Helper
{
    public static class Handlers
    {

        // more generic and reusable
        public delegate void StringEH(string s);
        public delegate void PacketReceivedEH(Packet p);

        // more specific
        public delegate void ObjectRequestEH(int clientId, string asset);

    }
}
