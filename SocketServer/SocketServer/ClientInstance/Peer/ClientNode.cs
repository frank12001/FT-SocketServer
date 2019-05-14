using System.Net;
using System.Collections.Generic;
using FTServer.Network;
using FTServer.ClientInstance.Peer;
using FTServer.ClientInstance.Packet;
using System.Net.Sockets;

namespace FTServer.ClientInstance
{
    /// <summary>
    /// 網路基本功能
    /// </summary>
    public class ClientNode
    {
        public readonly ISender Sender;
        public readonly Queue<byte[]> Rx;
        public readonly IPEndPoint IpEndPoint;
        public readonly SocketServer SocketServer;
        private readonly ClientNodeListener _listener;

        /// <summary>
        /// 客戶端節點建構子
        /// </summary>
        /// <param name="sender">外部發送者</param>
        /// <param name="iPEndPoint"></param>
        /// <param name="socketServer"></param>
        /// <remarks>
        /// 用於處理與客戶端溝通，並觸發接收封包事件
        /// </remarks>
        public ClientNode(ISender sender, IPEndPoint iPEndPoint,
                          SocketServer socketServer)
        {           
            Rx = new Queue<byte[]>();                       // 初始化接收佇列
            IpEndPoint = iPEndPoint;
            SocketServer = socketServer;
            Sender = sender;
            // 建立客戶端節點並開始接受封包傳入
            _listener = new ClientNodeListener(this);            
        }

        /// <summary>
        /// 將資料寫出
        /// </summary>
        /// <param name="eventData">繼承IPacket之封包實體</param>
        public void Write(IPacket eventData)
        {
            byte[] buff = Math.Serialize.Compress(Math.Serialize.ToByteArray(eventData));
            if (Sender is Udp)
            {
                if (buff.Length > Udp.PacketLengthLimit)
                {
                    throw new SocketException((int)SocketError.MessageSize);
                }
            }
            Sender.SendAsync(buff, IpEndPoint);
        }

        /// <summary>
        /// 將資料寫出
        /// </summary>
        /// <param name="buff">要傳送的 byte array </param>
        public void Write(byte[] buff)
        {
            //Console.WriteLine("封包大小 : " + buff.Length);
            Sender.SendAsync(buff, IpEndPoint);
        }

        /// <summary>
        /// 接收封包時的處理函式
        /// </summary>
        /// <param name="packet"></param>
        public virtual void OnOperationRequest(IPacket packet)
        {}

        public virtual void OnDisconnect()
        {
            _listener.Dispose();
        }

        /// <summary>
        /// 主動斷線
        /// </summary>
        public void Disconnect()
        {
            SocketServer.CloseClient(IpEndPoint);
        }

        public override string ToString()
        {
            return IpEndPoint.ToString();
        }
    }
}
