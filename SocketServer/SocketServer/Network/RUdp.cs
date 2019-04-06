using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using FTServer.Log;
using FTServer.ClientInstance;
using LiteNetLib;

namespace FTServer.Network
{
    public class RUdp : Core
    {
        EventBasedNetListener listener;
        NetManager server;
        Dictionary<string,RUdpClient> clients;
        IPEndPoint mIPEndPoint;
        int i = 0;
        public RUdp(SocketServer socketServer, IPEndPoint iPEndPoint) : base(socketServer)
        {
            clients = new Dictionary<string, RUdpClient>();
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            mIPEndPoint = iPEndPoint;

            listener.ConnectionRequestEvent += request =>
            {
                request.AcceptIfKey("SomeConnectionKey");
            };

            listener.PeerConnectedEvent += peer =>
            {
                i++;
                Console.WriteLine("Count : " + i);
                //Console.WriteLine("We got connection: {0}", peer.EndPoint); // Show peer ip
                string clientIp = peer.EndPoint.ToString();
                RUdpClient client = new RUdpClient(peer);
                lock (clients)
                {
                    // 請使用「取代」方法來取代「刪除後增加」,Dictionary會在刪除及增加時進行樹平衡
                    if (clients.ContainsKey(clientIp))
                        clients[clientIp] = client;
                    else
                        clients.Add(clientIp, client);
                }
                ReceiveResult receiveResult = new ReceiveResult(peer.EndPoint);

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
                    peer.Send(new byte[] { 1 },DeliveryMethod.ReliableOrdered);
                }
                catch (Exception e)
                {
                    Printer.WriteError($"Accept connection failed : {e.Message}\n{e.StackTrace}");
                }
            };

            listener.NetworkReceiveEvent += (NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)=> 
            {
                //reader.GetBytesWithLength
                if (clients.TryGetValue(peer.EndPoint.ToString(), out RUdpClient client))
                {
                    byte[] b = new byte[reader.AvailableBytes];
                    reader.GetBytes(b, reader.AvailableBytes);
                    //Console.WriteLine("We got something 2 : {0},Length: {1}", peer.EndPoint, b.Length); // Show peer ip
                    client.PassData(b);
                }
                reader.Recycle();
            };

            listener.PeerDisconnectedEvent += (NetPeer peer, DisconnectInfo disconnectInfo) =>
            {
                i--;
                Console.WriteLine("Count : " + i);
            };

            Task.Run(async () =>
            {
                while (true)
                {
                    server.PollEvents();
                    await Task.Delay(15);
                }
            });
        }

        public override async Task StartListen()
        {
            server.Start(mIPEndPoint.Port);
        }

        public override async Task SendAsync(byte[] datagram, IPEndPoint endPoint)
        {
            if (clients.TryGetValue(endPoint.ToString(), out RUdpClient webClient))
            {
                await webClient.Send(datagram);
            }
        }

        public override void DisConnect(IPEndPoint iPEndPoint)
        {
            string key = iPEndPoint.ToString();
            if (clients.TryGetValue(key, out RUdpClient client))
            {
                client.DisConnect();
                clients.Remove(key);
            }
        }
    }

    public class RUdpClient
    {
        private NetPeer _NetPeer;
        public IPEndPoint IPEndPoint { get; private set; }
        public ClientNode _ClientNode = null;
        public RUdpClient(NetPeer netPeer)
        {
            _NetPeer = netPeer;
            IPEndPoint = _NetPeer.EndPoint;            
        }

        public async Task Send(byte[] datagram)
        {
            _NetPeer.Send(datagram, DeliveryMethod.Unreliable);
        }

        public void PassData(byte[] datagram)
        {
            _ClientNode.Rx.Enqueue(datagram);
        }

        public void DisConnect()
        {
            if(_NetPeer.ConnectionState == ConnectionState.Connected)
                _NetPeer.Disconnect();
        }
    }
}
