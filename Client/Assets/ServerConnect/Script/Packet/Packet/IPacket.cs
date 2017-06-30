using System;
using System.Collections.Generic;

namespace TCPServer.ClientInstance.Packet
{
    [Serializable]
    public abstract class IPacket
    {
        /// <summary>
        /// 測試用
        /// </summary>
        public string ForTest { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        public byte OperationCode { get; set; }
        public Dictionary<byte, object> Parameters { get { return GetDictionary(); } }

        //----實際 Dictionary 存放的地方---//
        //----直接用 Dictionary 不能傳送---//
        private byte[] _key;
        private object[] _object;


        public IPacket(byte operstorCode, Dictionary<byte, object> dic)
        {
            this.OperationCode = operstorCode;
            _key = new byte[dic.Count];
            _object = new object[dic.Count];

            //將傳入的 Dictionary 做轉換，轉換成可傳送的格式
            byte index = 0;
            foreach (KeyValuePair<byte, object> value in dic)
            {
                _key[index] = value.Key;
                _object[index] = value.Value;
                index++;
            }
        }

        /// <summary>
        /// 將此 class 中的 Dictionary 資料型態做轉換
        /// </summary>
        /// <returns></returns>
        private Dictionary<byte, object> GetDictionary()
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
