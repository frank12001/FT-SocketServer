using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;
using FTServer.ClientInstance;


namespace FTServer.Network
{
    public class Client
    {
        private WebSocketClient _WebSocket;
        private WebSocket _ServerCore;
        public IPEndPoint IPEndPoint { get; private set; }
        public ClientNode _ClientNode = null;
        public Client(WebSocketClient webSocket, WebSocket serverCore)
        {
            _WebSocket = webSocket;
            _ServerCore = serverCore;
            IPEndPoint = webSocket.Context.UserEndPoint;

            _WebSocket._OnMessage += PassData;
            _WebSocket._OnClose += closeMsg => { EndConnection(); };
            _WebSocket._OnError += exception => { Console.WriteLine(exception); EndConnection(); };           
        }

        public async Task Send(byte[] datagram)
        {
            _WebSocket.SendBytes(datagram);        
        }

        private void PassData(byte[] datagram)
        {
            _ClientNode.Rx.Enqueue(datagram);
        }

        private void EndConnection()
        {
            lock (_ServerCore.clients)
            {
                _ServerCore.clients.Remove(IPEndPoint.ToString());
            }
            _ServerCore.ClientInstance.Remove(IPEndPoint.ToString());
            //if (_ClientNode != null)
            //{
            //    _ClientNode.OnDisconnect();
            //}
        }

        public void Close()
        {
            _WebSocket.CloseConnection();
        }
    }

    public class WebSocket : Core
    {
        private WebSocketServer server;
        public Dictionary<string, Client> clients { get; private set; }

        public WebSocket(SocketServer socketServer, int port) : base(socketServer)
        {
            clients = new Dictionary<string, Client>();


            server = new WebSocketServer(port)
            {
                WaitTime = TimeSpan.FromMilliseconds(5000)
            };
            server.WebSocketServices.AddService<WebSocketClient>("/WebSocket", socket =>
            {
                socket._OnOpen += async () =>
                {
                    //Console.WriteLine("Open!");
                    Client client = new Client(socket, this);

                    lock (clients)
                    {
                        clients.Add(client.IPEndPoint.ToString(), client);
                    }

                    ReceiveResult receiveResult = new ReceiveResult(client.IPEndPoint);
                    string clientIp = receiveResult.RemoteEndPoint.ToString();

                    // Coinmouse : 直接去掉已存在的玩家實體(因為timeout可能還沒到)
                    if (ClientInstance.ContainsKey(clientIp))
                        ClientInstance.Remove(clientIp);
                    // 建立玩家peer實體
                    ClientNode cNode = _SocketServer.GetPeer(this, receiveResult.RemoteEndPoint, _SocketServer);
                    client._ClientNode = cNode;
                    try
                    {
                        //註冊到 mListener 中，讓他的 Receive 功能能被叫
                        ClientInstance.Add(clientIp, cNode);
                        //成功加入後傳送 Connect 事件給 Client
                        await SendAsync(new byte[] { 1 }, cNode.iPEndPoint);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                };
            });

            server.AddWebSocketService<Ping>("/Ping");
        }

        public override async Task StartListen()
        {
            server.Start();
        }

        public ClientNode GetClientNode(IPEndPoint iPEndPoint)
        {
            ClientNode result = null;
            ClientInstance.TryGetValue(iPEndPoint.ToString(), out result);
            return result;
        }

        public override async Task SendAsync(byte[] datagram, IPEndPoint endPoint)
        {
            //Console.WriteLine("Send datagram length = " + datagram.Length);
            if (clients.TryGetValue(endPoint.ToString(), out Client webClient))
            {
                await webClient.Send(datagram);
            }
        }

        public override void DisConnect(IPEndPoint iPEndPoint)
        {
            lock (ClientInstance)
            {
                if (iPEndPoint != null)
                {
                    ClientInstance.Remove(iPEndPoint.ToString());
                    if (clients.TryGetValue(iPEndPoint.ToString(), out Client client))
                    {
                        client.Close();
                    }
                }
            }
        }
    }
    public class WebSocketClient : WebSocketBehavior
    {
        public event Action _OnOpen;
        public event Action<byte[]> _OnMessage;
        public event Action<CloseEventArgs> _OnClose;
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

    public class Ping : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Console.WriteLine("WebSocketSharp Call Ping");
        }
    }
}
