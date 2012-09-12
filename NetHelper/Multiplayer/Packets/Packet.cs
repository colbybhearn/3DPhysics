using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class Packet
    {
        public enum Types
        {
            scClientInfoRequest,
            csClientInfoResponse,
            csObjectUpdate,
            scObjectUpdate
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
                Debug.WriteLine(e.Message);
            }
            int datalen = (int)ms.Length;
            byte[] lenbytes = BitConverter.GetBytes(datalen);
            if (lenbytes.Length != 4)
            {

            }
            List<byte> dataList = new List<byte>();
            dataList.AddRange(lenbytes);
            dataList.AddRange(ms.ToArray());
            //ms.Write(lenbytes, 0, lenbytes.Length);
            
            //   | len of serialised data | Serialized Data |

            return dataList.ToArray();
        }

        public static Packet Deserialize(byte[] data)
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
                Debug.WriteLine(e.Message);
            }
            return null;
        }

    }
}
