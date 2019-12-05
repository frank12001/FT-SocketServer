using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FTServer.Network
{
    public abstract class Core : ISender
    {
        public readonly Dictionary<string, Instance> ClientInstance;
        protected SocketServer _SocketServer;
        protected Core(SocketServer server)
        {
            ClientInstance = new Dictionary<string, Instance>();
            _SocketServer = server;
        }

        public virtual async Task StartListen()
        {
            throw new NotImplementedException();
        }

        public virtual Task SendAsync(byte[] data, IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public virtual Task RudpSendAsync(byte[] data, IPEndPoint endPoint, int type)
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
