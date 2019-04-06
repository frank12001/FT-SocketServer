using FTServer.ClientInstance.Packet;
using MessagePack;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace FTServer.Math
{
    internal class Serializate
    {
        #region 物件序列化
        public static byte[] ToByteArray(object source)
        {
            IPacket packet = (IPacket)source;
            return MessagePackSerializer.Serialize(packet);
            //var Formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            //using (var stream = new System.IO.MemoryStream())
            //{
            //    Formatter.Serialize(stream, source);
            //    return stream.ToArray();
            //}
        }

        public static object ToObject(byte[] source)
        {
            return MessagePackSerializer.Deserialize<IPacket>(source);
            //try
            //{
            //    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            //    using (var stream = new MemoryStream(source))
            //    {
            //        formatter.Binder = new CurrentAssemblyDeserializationBinder();
            //        formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            //        return formatter.Deserialize(stream);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Error in Serializate.ToObject");
            //    return new object();
            //}
        }

        private sealed class CurrentAssemblyDeserializationBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                //Console.WriteLine("Convert : " + String.Format("{0}, {1} ", typeName, Assembly.GetExecutingAssembly().FullName));
                return Type.GetType(String.Format("{0}, {1} ", typeName, Assembly.GetExecutingAssembly().FullName));
            }
        }

        public static byte[] JsonSerialize(object source)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            byte[] byteArray = Encoding.Unicode.GetBytes(json);
            return byteArray;
        }

        public static object JsonDeserialize(byte[] data)
        {
            string json = Encoding.Unicode.GetString(data);
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        }

        public static T JsonDeserialize<T>(byte[] data)
        {
            string json = Encoding.Unicode.GetString(data);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
        #endregion

        #region byte array 壓縮
        public static byte[] Compress(byte[] buffer)
        {
            return buffer;
            //MemoryStream ms = new MemoryStream();
            //GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
            //zip.Write(buffer, 0, buffer.Length);                // 寫入要被壓縮的資料
            //zip.Close();
            //ms.Position = 0;

            //MemoryStream outStream = new MemoryStream();

            //byte[] compressed = new byte[ms.Length];
            //ms.Read(compressed, 0, compressed.Length);          // 讀取壓縮後的資料

            //byte[] gzBuffer = new byte[compressed.Length + 4];
            //Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);                // 複製壓縮後的資料並在前方空出四個byte
            //Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            //return gzBuffer;
        }

        public static byte[] Decompress(byte[] gzBuffer)
        {
            return gzBuffer;
            //MemoryStream ms = new MemoryStream();
            //int msgLength = BitConverter.ToInt32(gzBuffer, 0);
            //ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

            //byte[] buffer = new byte[msgLength];

            //ms.Position = 0;
            //GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            //zip.Read(buffer, 0, buffer.Length);

            //return buffer;
        }
        #endregion
    }
}
