using System;
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
        public PeerBase(ISender _tclient, IPEndPoint iPEndPoint, SocketServer applicationInterface, ushort timeout = 8000) : base(_tclient, iPEndPoint, applicationInterface, timeout)
        {}

        #region myFunction
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="packet"></param>
        public void SendBytes(byte[] buff)
        {
            base.Write(buff);
        }
        /// <summary>
        /// 立即回傳事件資料
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="packet"></param>
        public void SendEvent(byte eventCode, Dictionary<byte, object> packet)
        {
            IPacket eventData = new IPacket((byte)eventCode, packet);
            base.Write(eventData);
        }
        #endregion
    }
}

