using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using FTServer.Log;
using FTServer.ClientInstance;

namespace FTServer.Network
{
    public abstract class Core : IListener, ISender
    {
        public Dictionary<string, ClientNode> ClientInstance { get; private set; }
        protected SocketServer _SocketServer;
        protected Core(SocketServer server)
        {
            ClientInstance = new Dictionary<string, ClientNode>();
            _SocketServer = server;
        }

        public virtual async Task StartListen()
        {
            while (true)
            {
                try
                {
                    //等待有人連接
                    ReceiveResult receiveResult = await ReceiveAsync();                    
                    string clientIp = receiveResult.RemoteEndPoint.ToString();
                    if (!ClientInstance.ContainsKey(clientIp) && receiveResult.isOk)
                    {
                        // 建立玩家peer實體
                        ClientNode cNode = _SocketServer.GetPeer(this, receiveResult.RemoteEndPoint, _SocketServer);                        
                        //註冊到 mListener 中，讓他的 Receive 功能能被叫
                        ClientInstance.Add(clientIp, cNode);
                        //成功加入後傳送 Connect 事件給 Client
                        await SendAsync(new byte[] { 1 }, cNode.iPEndPoint);
                    }
                }
                catch (ArgumentException ex)
                {
                    Printer.WriteLine(ex.StackTrace);
                }
                catch (NullReferenceException ex)
                {
                    Printer.WriteLine(ex.StackTrace);
                }
            }
        }

        public virtual Task<ReceiveResult> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task SendAsync(byte[] datagram, IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 主動斷線
        /// </summary>
        /// <param name="iPEndPoint"></param>
        public virtual void DisConnect(IPEndPoint iPEndPoint)
        {
            ClientInstance.Remove(iPEndPoint.ToString());
        }
    }
}
