using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace FTServer
{
    public class Serializate
    {
        public static byte[] ToByteArray(object source)
        {
            var Formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var stream = new System.IO.MemoryStream())
            {
                Formatter.Serialize(stream, source);
                return stream.ToArray();
            }
        }
        public static object ToObject(byte[] source)
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var stream = new MemoryStream(source))
            {
                formatter.Binder = new CurrentAssemblyDeserializationBinder();
                formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                return formatter.Deserialize(stream);
            }
        }
    }
    
public sealed class CurrentAssemblyDeserializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        return Type.GetType(String.Format("{0}, {1}", typeName, Assembly.GetExecutingAssembly().FullName));
    }
}
}
