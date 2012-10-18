using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class Packet
    {
        public enum Types
        {
            KeepAlive, // not used yet (could be used to determine ping)
            Chat,
            scClientInfoRequest,
            csClientInfoResponse,
            csObjectUpdate,  // may not be needed, use scObjectUpdate
            scObjectUpdate,
            csObjectRequest,
            scObjectResponse,
            csObjectAction, // may not be needed either, cs and sc should handle action updates the same?
            scObjectAction,
            ClientDisconnectPacket,
            ClientConnectedPacket,
            Attributes,
            scObjectDeleted,
            csClientReady,
        }

        public Types Type;

        public Packet(Types type)
        {
            Type = type;
        }

        public virtual byte[] Serialize()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(ms, this);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }

            List<byte> dataList = new List<byte>();
            // serialization
            dataList.AddRange(ms.ToArray());
            // type
            dataList.InsertRange(0, BitConverter.GetBytes((int)Type));
            
            byte[] lenbytes = BitConverter.GetBytes(dataList.Count);
            // length
            dataList.InsertRange(0,lenbytes);
            
            //   | len of data to follow | Type | Serialized Data Or Custom Data|

            return dataList.ToArray();
        }

        public static Packet Read(byte[] data)
        {
            try
            {
                int itype = BitConverter.ToInt32(data, 0);
                Types ptype = (Types)itype;
                byte[] inner = new byte[data.Length - 4];
                for (int i = 4; i < data.Length; i++)
                    inner[i - 4] = data[i];
                switch (ptype)
                {
                    case Types.scObjectUpdate:
                        ObjectUpdatePacket oup = new ObjectUpdatePacket();
                        return oup.CustomDeserialize(inner);
                    default:
                        Packet p = new Packet(Types.KeepAlive);
                        return p.Deserialize(inner);
                }
            }
            catch(Exception E)
            {                
            }
            return null;
        }

        public virtual Packet Deserialize(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data); 
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                object o = formatter.Deserialize(ms);
                if (o is Packets.Packet)
                    return o as Packets.Packet;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            return null;
        }

    }
}
