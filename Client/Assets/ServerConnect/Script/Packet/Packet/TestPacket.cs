using System;
using System.Collections.Generic;

namespace TCPServer.ClientInstance.Packet
{
    [Serializable]
    public class TestPacket
    {
        public string Text = "";

        private byte[] _key;
        private object[] _object;

        public TestPacket(Dictionary<byte, object> dic)
        {
            _key = new byte[dic.Count];
            _object = new object[dic.Count];

            byte index = 0;
            foreach (KeyValuePair<byte, object> value in dic)
            {
                _key[index] = value.Key;
                _object[index] = value.Value;
                index++;
            }
        }

        public Dictionary<byte, object> GetDictionary()
        {
            Dictionary<byte, object> result = new Dictionary<byte, object>();
            for (byte i = 0; i < _key.Length; i++)
            {
                result.Add(_key[i], _object[i]);
            }
            return result;
        }
    }
}
