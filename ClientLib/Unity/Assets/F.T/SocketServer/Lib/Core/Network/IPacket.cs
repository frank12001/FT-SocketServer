using MessagePack;
using System;
using System.Collections.Generic;

namespace FTServer.ClientInstance.Packet
{
    /// <summary>
    /// 封包類別
    /// </summary>
    /// <remarks>
    /// 打包封包訊號類型碼及資料
    /// </remarks>
    [Serializable, MessagePackObject]
    public class IPacket
    {
        [Key(0)]
        public byte OperationCode;
        [Key(1)]
        public Dictionary<byte, object> Parameters;

        /// <summary>
        /// 封包建構子
        /// </summary>
        /// <param name="operatorCode"></param>
        /// <param name="dic"></param>
        public IPacket(byte operatorCode, Dictionary<byte, object> dic)
        {
            OperationCode = operatorCode;
            Parameters = dic;
        }
    }
    ///// <summary>
    ///// 封包類別
    ///// </summary>
    ///// <remarks>
    ///// 打包封包訊號類型碼及資料
    ///// </remarks>
    //[Serializable]
    //public class IPacket
    //{
    //    /// <summary>
    //    /// 測試用
    //    /// </summary>
    //    public string ForTest { get; set; }

    //    /// <summary>
    //    /// 索引
    //    /// </summary>
    //    public byte OperationCode { get; set; }

    //    /*
    //    * Dictionary 無法被序列化?
    //    */
    //    public Dictionary<byte, object> Parameters => GetDictionary();

    //    //----實際 Dictionary 存放的地方---//
    //    //----直接用 Dictionary 不能傳送---//
    //    private byte[] _key;
    //    private object[] _object;

    //    /// <summary>
    //    /// 封包建構子
    //    /// </summary>
    //    /// <param name="operatorCode"></param>
    //    /// <param name="dic"></param>
    //    public IPacket(byte operatorCode, Dictionary<byte, object> dic)
    //    {
    //        this.OperationCode = operatorCode;
    //        _key = new byte[dic.Count];
    //        _object = new object[dic.Count];

    //        //將傳入的 Dictionary 做轉換，轉換成可傳送的格式
    //        byte index = 0;
    //        foreach (KeyValuePair<byte, object> value in dic)
    //        {
    //            _key[index] = value.Key;
    //            _object[index] = value.Value;
    //            index++;
    //        }
    //    }

    //    /// <summary>
    //    /// 將此 class 中的 Dictionary 資料型態做轉換
    //    /// </summary>
    //    /// <returns></returns>
    //    private Dictionary<byte, object> GetDictionary()
    //    {
    //        var result = new Dictionary<byte, object>();
    //        for (byte i = 0; i < _key.Length; i++)
    //        {
    //            result.Add(_key[i], _object[i]);
    //        }
    //        return result;
    //    }
    //}
}
