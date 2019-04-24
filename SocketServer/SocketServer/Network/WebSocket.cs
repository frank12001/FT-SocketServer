using System;
using System.Net;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using FTServer.ClientInstance;
using FTServer.Log;

namespace FTServer.Network
{
    public class WSInstance : Instance , IDisposable
    {
        private WSPeer WSPeer;
        private WebSocket WebSocket;
        public IPEndPoint IPEndPoint { get; private set; }
        public WSInstance(WebSocket webSocket, ClientNode clientNode, WSPeer wsPeer) : base(clientNode)
        {
            WSPeer = wsPeer;
            WebSocket = webSocket;
            IPEndPoint = WSPeer.Context.UserEndPoint;      
        }

        public async Task Send(byte[] datagram)
        {
            WSPeer.SendBytes(datagram);        
        }

        public void PassData(byte[] datagram)
        {
            _ClientNode.Rx.Enqueue(datagram);
        }

        public void Dispose()
        {
            WSPeer.CloseConnection();
            _ClientNode.OnDisconnect();
        }
    }

    public class WebSocket : Core
    {
        private WebSocketServer server;
        public WebSocket(SocketServer socketServer, int port) : base(socketServer)
        {
            server = new WebSocketServer(port)
            {
                WaitTime = TimeSpan.FromMilliseconds(5000)
            };           
            server.WebSocketServices.AddService<WSPeer>("/WebSocket", peer =>
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
                        WSInstance instance = new WSInstance(this, cNode, peer);
                        //註冊到 mListener 中，讓他的 Receive 功能能被叫
                        ClientInstance.Add(clientIp, instance);
                        //成功加入後傳送 Connect 事件給 Client
                        peer.SendBytes(new byte[] { 1 });
                    }
                    catch (Exception e)
                    {
                        Printer.WriteError($"Accept connection failed : {e.Message}\n{e.StackTrace}");
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
            server.Start();
        }

        public override async Task SendAsync(byte[] datagram, IPEndPoint endPoint)
        {
            if (ClientInstance.TryGetValue(endPoint.ToString(), out Instance instance))
            {
                await ((WSInstance)instance).Send(datagram);
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
    public class WSPeer : WebSocketBehavior
    {
        public event Action _OnOpen;
        public event Action<byte[]> _OnMessage;
        public Action<CloseEventArgs> _OnClose;
        public event Action<ErrorEventArgs> _OnError;

        public void SendBytes(byte[] data)
        {
            if (this.ConnectionState == WebSocketState.Open)
                this.Send(data);
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
            Console.WriteLine("WSPeer OnClose");
            _OnClose?.Invoke(e);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine("WSPeer OnError");
            _OnError?.Invoke(e);
        }

        public void CloseConnection()
        {            
            Close();
        }
    }
}
