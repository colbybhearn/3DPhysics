using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectUpdatePacket : Packet
    {
        public int objectId;
        public string assetName;
        public Vector3 position;
        public Matrix orientation;
        public Vector3 velocity;
        
        public ObjectUpdatePacket(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel) 
            : base(Types.scObjectUpdate)
        {
            objectId = id;
            assetName = asset;
            position = pos;
            orientation = orient;
            velocity = vel;
        }

        public ObjectUpdatePacket() 
            : base (Types.scObjectUpdate)
        {
            // TODO: Complete member initialization
        }

        public override byte[] Serialize()
        {
            int length = assetName.Length + (25 * 4);
            byte[] data = new byte[length+4];
            int index = 0;
            Array.Copy(BitConverter.GetBytes((int)length), 0, data, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes((int)Type), 0, data, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(objectId), 0, data, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(assetName.Length), 0, data, index, 4);
            index += 4;
            foreach (char c in assetName)
                Array.Copy(new byte[1] { (byte)c }, 0, data, index++, 1);
            Array.Copy(BitConverter.GetBytes(position.X), 0, data, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(position.Y), 0, data, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(position.Z), 0, data, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M11), 0, data, index, 4);index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M12), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M13), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M14), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M21), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M22), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M23), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M24), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M31), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M32), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M33), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M34), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M41), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M42), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M43), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(orientation.M44), 0, data, index, 4); index += 4;
            Array.Copy(BitConverter.GetBytes(velocity.X), 0, data, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(velocity.Y), 0, data, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(velocity.Z), 0, data, index, 4);
            return data;
                
            /*List<byte> data = new List<byte>;
            data.AddRange(BitConverter.GetBytes((int)Type));
            data.AddRange(BitConverter.GetBytes(objectId));
            data.AddRange(BitConverter.GetBytes(assetName.Length));
            foreach(char c in assetName)
                data.Add((byte)c);
            data.AddRange(BitConverter.GetBytes(position.X));
            data.AddRange(BitConverter.GetBytes(position.Y));
            data.AddRange(BitConverter.GetBytes(position.Z)); // 24 including this line
            data.AddRange(BitConverter.GetBytes(orientation.M11));
            data.AddRange(BitConverter.GetBytes(orientation.M12));
            data.AddRange(BitConverter.GetBytes(orientation.M13));
            data.AddRange(BitConverter.GetBytes(orientation.M14));
            data.AddRange(BitConverter.GetBytes(orientation.M21));
            data.AddRange(BitConverter.GetBytes(orientation.M22));
            data.AddRange(BitConverter.GetBytes(orientation.M23));
            data.AddRange(BitConverter.GetBytes(orientation.M24));
            data.AddRange(BitConverter.GetBytes(orientation.M31));
            data.AddRange(BitConverter.GetBytes(orientation.M32));
            data.AddRange(BitConverter.GetBytes(orientation.M33));
            data.AddRange(BitConverter.GetBytes(orientation.M34));
            data.AddRange(BitConverter.GetBytes(orientation.M41));
            data.AddRange(BitConverter.GetBytes(orientation.M42));
            data.AddRange(BitConverter.GetBytes(orientation.M43));
            data.AddRange(BitConverter.GetBytes(orientation.M44));
            data.AddRange(BitConverter.GetBytes(velocity.X));
            data.AddRange(BitConverter.GetBytes(velocity.Y));
            data.AddRange(BitConverter.GetBytes(velocity.Z));
            data.InsertRange(0, BitConverter.GetBytes(data.Count));
            return data.ToArray();*/
        }

        public Packet CustomDeserialize(byte[] data)
        {
            int index=0;
            objectId = BitConverter.ToInt32(data, index);  
            index+=4;
            int strLen = BitConverter.ToInt32(data, index);
            index+=4;
            assetName = string.Empty;
            for(int ci = 0;ci<strLen;ci++)
                assetName+=(char)data[ci+index];
            index+=strLen;
            position.X = BitConverter.ToSingle(data, index);
            index+=4;
            position.Y = BitConverter.ToSingle(data, index);
            index+=4;
            position.Z = BitConverter.ToSingle(data, index);
            index+=4;

            orientation = Matrix.Identity;
            orientation.M11 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M12 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M13 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M14 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M21 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M22 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M23 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M24 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M31 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M32 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M33 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M34 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M41 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M42 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M43 = BitConverter.ToSingle(data, index);
            index+=4;
            orientation.M44 = BitConverter.ToSingle(data, index);
            index+=4;

            velocity.X = BitConverter.ToSingle(data, index);
            index+=4;
            velocity.Y = BitConverter.ToSingle(data, index);
            index+=4;
            velocity.Z = BitConverter.ToSingle(data, index);
            index+=4;

            return this;
        }
    }
}
