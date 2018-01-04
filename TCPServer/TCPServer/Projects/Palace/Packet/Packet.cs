using System;

namespace TCPServer.Projects.Palace.Packet
{
    /// <summary>
    /// 開始排隊時，傳入的資料
    /// </summary>
    [Serializable]
    public class QueueInfo
    {
        public string Key;
    }
    /// <summary>
    /// 遊戲時輸入的傳遞封包
    /// </summary>
    [Serializable]
    public class LoadingNextScene
    {
        public bool Start = true;
    }
}
