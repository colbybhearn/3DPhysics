
namespace Helper.Multiplayer
{
    public class ClientPacketInfo
    {
        //public ClientInfo client;
        public int id;
        public Packets.Packet packet;

        public ClientPacketInfo(int i, Packets.Packet p)
        {
            id = i;
            packet = p;
        }
    }
}
