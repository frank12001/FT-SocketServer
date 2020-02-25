using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;
using FTServer.ClientInstance;
using NLog;

namespace FTServer.Network
{
    public class WebSocket : Core
    {
        private readonly WebSocketServer _server;

        private NLog.Logger mlogger = LogManager.GetCurrentClassLogger();

        public WebSocket(SocketServer socketServer, int port) : base(socketServer)
        {
            _server = new WebSocketServer(port)
            {
                WaitTime = TimeSpan.FromSeconds(1)
            };
            _server.Log.KnownFatal = new List<string>()
                {
                    "The header of a frame cannot be read from the stream.",
                    "Object name: 'System.Net.Sockets.NetworkStream'."
                };
            _server.WebSocketServices.AddService<WSPeer>("/WebSocket", peer =>
            {
                peer._OnOpen += () =>
                {
                    IPEndPoint iPEndPoint = peer.Context.UserEndPoint;
                    string clientIp = iPEndPoint.ToString();

                    if (ClientInstance.ContainsKey(clientIp))
                        ClientInstance.Remove(clientIp);

                    // 建立玩家peer實體
                    ClientNode cNode = _SocketServer.GetPeer(this, iPEndPoint, _SocketServer);
                    try
                    {
                        //註冊到 mListener 中，讓他的 Receive 功能能被叫               
                        WSInstance instance = new WSInstance(cNode, peer);
                        //註冊到 mListener 中，讓他的 Receive 功能能被叫
                        ClientInstance.Add(clientIp, instance);
                        //成功加入後傳送 Connect 事件給 Client
                        peer.SendBytes(new byte[] { 1 });
                        cNode.Initialize();
                    }
                    catch (Exception e)
                    {
                        mlogger.Error($"Accept connection failed : {e.Message}\n{e.StackTrace}");
                    }
                };

                peer._OnMessage += packet =>
                {
                    if (ClientInstance.TryGetValue(peer.Context.UserEndPoint.ToString(), out Instance instance))
                    {
                        WSInstance client = (WSInstance)instance;
                        client.PassData(packet);
                    }
                };

                peer._OnClose += e =>
                {
                    DisConnect(peer.Context.UserEndPoint);
                };

                peer._OnError += e =>
                {
                    DisConnect(peer.Context.UserEndPoint);
                };
            });
        }

        public override async Task StartListen()
        {
            _server.Start();
        }

        public override async Task SendAsync(byte[] data, IPEndPoint endPoint)
        {
            if (ClientInstance.TryGetValue(endPoint.ToString(), out Instance instance))
            {
                await ((WSInstance)instance).Send(data);
            }
        }

        public override void DisConnect(IPEndPoint iPEndPoint)
        {
            lock (ClientInstance)
            {
                string key = iPEndPoint.ToString();
                if (ClientInstance.TryGetValue(key, out Instance instance))
                {
                    WSInstance wsInstance = (WSInstance)instance;
                    wsInstance.Dispose();
                    ClientInstance.Remove(key);
                }
            }
        }
    }
    public class WSInstance : Instance, IDisposable
    {
        private readonly WSPeer _wsPeer;
        public readonly IPEndPoint IpEndPoint;
        public WSInstance( ClientNode clientNode, WSPeer wsPeer) : base(clientNode)
        {
            _wsPeer = wsPeer;
            IpEndPoint = _wsPeer.Context.UserEndPoint;
        }

        public async Task Send(byte[] data)
        {
            _wsPeer.SendBytes(data);
        }

        public void PassData(byte[] data)
        {
            _ClientNode.Rx.Enqueue(data);
        }

        public void Dispose()
        {
            _wsPeer.CloseConnection();
            _ClientNode.OnDisconnect();
        }
    }
    //WebSocketSharp 需要定義的使用者
    public class WSPeer : WebSocketBehavior
    {
        public event Action _OnOpen;
        public event Action<byte[]> _OnMessage;
        public Action<CloseEventArgs> _OnClose;
        public event Action<ErrorEventArgs> _OnError;

        public void SendBytes(byte[] data)
        {
            if (ConnectionState == WebSocketState.Open)
                Send(data);
        }

        protected override void OnOpen()
        {
            _OnOpen?.Invoke();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            _OnMessage?.Invoke(e.RawData);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            _OnClose?.Invoke(e);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            _OnError?.Invoke(e);
        }

        public void CloseConnection()
        {            
            Close();
        }
    }
}
