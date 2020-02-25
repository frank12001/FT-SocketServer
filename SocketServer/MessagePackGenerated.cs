#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Resolvers
{
    using System;
    using MessagePack;

    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        GeneratedResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(2)
            {
                {typeof(global::System.Collections.Generic.Dictionary<byte, object>), 0 },
                {typeof(global::FTServer.ClientInstance.Packet.IPacket), 1 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.DictionaryFormatter<byte, object>();
                case 1: return new MessagePack.Formatters.FTServer.ClientInstance.Packet.IPacketFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612



#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters.FTServer.ClientInstance.Packet
{
    using System;
    using MessagePack;


    public sealed class IPacketFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::FTServer.ClientInstance.Packet.IPacket>
    {

        public int Serialize(ref byte[] bytes, int offset, global::FTServer.ClientInstance.Packet.IPacket value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += MessagePackBinary.WriteByte(ref bytes, offset, value.OperationCode);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<byte, object>>().Serialize(ref bytes, offset, value.Parameters, formatterResolver);
            return offset - startOffset;
        }

        public global::FTServer.ClientInstance.Packet.IPacket Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __OperationCode__ = default(byte);
            var __Parameters__ = default(global::System.Collections.Generic.Dictionary<byte, object>);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __OperationCode__ = MessagePackBinary.ReadByte(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Parameters__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<byte, object>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::FTServer.ClientInstance.Packet.IPacket(__OperationCode__, __Parameters__);
            ____result.OperationCode = __OperationCode__;
            ____result.Parameters = __Parameters__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
