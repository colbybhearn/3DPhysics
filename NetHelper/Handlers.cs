using Helper.Multiplayer.Packets;
using Microsoft.Xna.Framework;


namespace Helper
{
    public static class Handlers
    {

        // more generic and reusable
        public delegate void IntEH(int i);
        public delegate void StringEH(string s);

        public delegate void voidEH();
        public delegate void StringStringEH(string s1, string s2);
        public delegate void PacketReceivedEH(Packet p);

        // more specific
        public delegate void ObjectRequestEH(int clientId, string asset);
        public delegate void ObjectRequestResponseEH(int objectId, string asset);
        public delegate void ObjectUpdateEH(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel);

    }
}
