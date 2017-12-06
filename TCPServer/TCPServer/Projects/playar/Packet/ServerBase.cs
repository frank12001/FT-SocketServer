using System;

namespace PhotonServerConnect.Packet
{
    [Serializable]
    public class DanceTime : ServerBase
    {
        public float Time = 0;
    }
    [Serializable]
    public class ServerBase
    {
        public byte ServerState = 0;
        public byte OperatorCode = 0;
        public string ErrorMessage = "";
    }
    [Serializable]
    public class ImagePacket
    {
        public byte[] texture2d;
        public int width, height, textureFormat;
    }
    [Serializable]
    public class Result
    {
        public byte Code;
        public string Message;
    }
}
