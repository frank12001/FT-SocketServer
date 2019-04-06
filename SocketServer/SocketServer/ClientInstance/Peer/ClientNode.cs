using System;
using System.Net;
using System.Collections.Generic;
using FTServer.Log;
using FTServer.Network;
using FTServer.ClientInstance.Peer;
using FTServer.ClientInstance.Packet;

namespace FTServer.ClientInstance
{
    /// <summary>
    /// 網路基本功能
    /// </summary>
    public class ClientNode
    {
        /// <summary>
        /// 多久從序列中寫出或讀入一包
        /// </summary>
        private const byte Tick_Read = 10, Tick_Write = 10, Tick_MainConnecting = 10;
        public readonly ISender _Sender;
        public readonly Queue<byte[]> Rx;
        public readonly IPEndPoint iPEndPoint;
        public readonly SocketServer socketServer;
        private readonly ClientNodeListener listener;

        /// <summary>
        /// 客戶端節點建構子
        /// </summary>
        /// <param name="sender">外部發送者</param>
        /// <param name="iPEndPoint"></param>
        /// <param name="socketServer"></param>
        /// <param name="timeout"></param>
        /// <remarks>
        /// 用於處理與客戶端溝通，並觸發接收封包事件
        /// </remarks>
        public ClientNode(ISender sender, IPEndPoint iPEndPoint,
                          SocketServer socketServer, ushort timeout=1000)
        {           
            Rx = new Queue<byte[]>();                       // 初始化接收佇列
            this.iPEndPoint = iPEndPoint;
            this.socketServer = socketServer;
            this._Sender = sender;
            // 建立客戶端節點並開始接受封包傳入
            listener = new ClientNodeListener(this,timeout);            
        }      

        ~ClientNode()
        {
            //Printer.WriteLine("Trigger ClientNode Deconstructor.");
        }

        /// <summary>
        /// 將資料寫出
        /// </summary>
        /// <param name="eventData">繼承IPacket之封包實體</param>
        public void Write(IPacket eventData)
        {
            byte[] buff = Math.Serializate.Compress(Math.Serializate.ToByteArray(eventData));
            //Console.WriteLine("封包大小 : " + buff.Length);
            _Sender.SendAsync(buff, iPEndPoint);
        }

        /// <summary>
        /// 將資料寫出
        /// </summary>
        /// <param name="eventData"></param>
        public void Write(byte[] buff)
        {
            //Console.WriteLine("封包大小 : " + buff.Length);
            _Sender.SendAsync(buff, iPEndPoint);
        }

        /// <summary>
        /// 接收封包時的處理函式
        /// </summary>
        /// <param name="packet"></param>
        public virtual void OnOperationRequest(IPacket packet)
        {}

        public virtual void OnDisconnect()
        {}


        /// <summary>
        /// 主動斷線
        /// </summary>
        public void Disconnect()
        {
            listener.Disconnect();
        }

        public override string ToString()
        {
            return iPEndPoint.ToString();
        }
    }
}
