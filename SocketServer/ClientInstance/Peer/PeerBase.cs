using System.Net;
using System.Collections.Generic;
using FTServer.Network;
using FTServer.ClientInstance.Packet;

namespace FTServer.ClientInstance
{
    /// <summary>
    /// 房間基本功能
    /// </summary>
    public class PeerBase : ClientNode
    {
        public PeerBase(ISender sender, IPEndPoint iPEndPoint, SocketServer applicationInterface) : base(sender, iPEndPoint, applicationInterface)
        {}

        #region myFunction
        public void SendBytes(byte[] buff)
        {
            Write(buff);
        }
        /// <summary>
        /// 立即回傳事件資料
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="packet"></param>
        public void SendEvent(byte eventCode, Dictionary<byte, object> packet)
        {
            IPacket eventData = new IPacket(eventCode, packet);
            Write(eventData);
        }
        #endregion
    }
}

