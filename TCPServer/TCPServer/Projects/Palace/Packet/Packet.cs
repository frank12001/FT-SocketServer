using System;

namespace TCPServer.Projects.Palace.Packet
{
    /// <summary>
    /// 開始排隊時，傳入的資料
    /// </summary>
    [Serializable]
    public class PalaceTest
    {
        public string Key;
    }
    /// <summary>
    /// 遊戲時輸入的傳遞封包
    /// </summary>
    [Serializable]
    public class GamingTest
    {
        public string Gaming1 = "";
    }
    /// <summary>
    /// 遊戲時輸入的傳遞封包
    /// </summary>
    [Serializable]
    public class GamingStart
    {
        public bool Start = true;
    }
}
