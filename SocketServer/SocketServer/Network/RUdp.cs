using System;
using System.Net;
using System.Threading.Tasks;
using FTServer.Log;
using FTServer.ClientInstance;
using LiteNetLib;
using System.Timers;

namespace FTServer.Network
{
    public class RUdp : Core
    {
        EventBasedNetListener listener;
        NetManager server;
        IPEndPoint mIPEndPoint;
        int i = 0;
        public RUdp(SocketServer socketServer, IPEndPoint iPEndPoint) : base(socketServer)
        {
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

                ReceiveResult receiveResult = new ReceiveResult(peer.EndPoint);

                if (ClientInstance.ContainsKey(clientIp))
                    ClientInstance.Remove(clientIp);
                // 建立玩家peer實體
                ClientNode cNode = _SocketServer.GetPeer(this, receiveResult.RemoteEndPoint, _SocketServer);
                try
                {
                    //註冊到 mListener 中，讓他的 Receive 功能能被叫               
                    RUDPInstance instance = new RUDPInstance(this,cNode,peer);
                    //註冊到 mListener 中，讓他的 Receive 功能能被叫
                    ClientInstance.Add(clientIp, instance);
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
                if (ClientInstance.TryGetValue(peer.EndPoint.ToString(), out Instance instance))
                {
                    RUDPInstance client = (RUDPInstance)instance;
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
            if (ClientInstance.TryGetValue(endPoint.ToString(), out Instance instance))
            {
                await ((RUDPInstance)instance).Send(datagram);
            }
        }

        public override void DisConnect(IPEndPoint iPEndPoint)
        {
            lock (ClientInstance)
            {
                string key = iPEndPoint.ToString();
                if (ClientInstance.TryGetValue(key, out Instance instance))
                {
                    RUDPInstance rudpInstance = (RUDPInstance)instance;
                    rudpInstance.Dispose();
                    ClientInstance.Remove(key);
                }
            }
        }       
    }
    public class RUDPInstance : Instance , IDisposable
    {
        private const byte Tick_MainConnecting = 100;
        /// <summary>
        /// 斷線之time out時間長度
        /// </summary>
        private readonly ushort TimeLimit_Disconnect = 10000;
        /// <summary>
        /// 接收封包及維持連線之Timer
        /// </summary>
        private Timer maintainConnecting;
        /// <summary>
        /// 接收封包之時間間隔
        /// </summary>
        private ushort Timer_ReadPacket = 0;
        private NetPeer _NetPeer;
        private RUdp RUdp;
        public IPEndPoint IPEndPoint { get; private set; }
        public RUDPInstance(RUdp rudp,ClientNode clientNode, NetPeer netPeer) : base(clientNode)
        {
            RUdp = rudp;
            _NetPeer = netPeer;
            IPEndPoint = _NetPeer.EndPoint;
            BeginMaintainConnectingAsync();   // 開始進行維持連線之封包發送
        }
        /// <summary>
        /// 每隔一段時間定期進行連絡以確認維持連線
        /// </summary>
        private void BeginMaintainConnectingAsync()
        {
            maintainConnecting = new Timer(Tick_MainConnecting);
            maintainConnecting.Elapsed += Handler_MaintainConnecting;
            maintainConnecting.Start();
        }
        private void Handler_MaintainConnecting(object o, ElapsedEventArgs e)
        {
            // 當維持連線之訊號中斷直到timeout，作斷線處理
            if (Timer_ReadPacket >= TimeLimit_Disconnect)
            {
                maintainConnecting.Stop();
                RUdp.DisConnect(IPEndPoint);
            }
            // 如果長時間未收到維持訊號
            Timer_ReadPacket += Tick_MainConnecting;
            if (Timer_ReadPacket >= 2000)
            {
                // 對客戶端發送維持連線之訊號
                byte[] buff = new byte[] { 0 };
                Send(buff);
            }
        }
        public async Task Send(byte[] datagram)
        {
            _NetPeer.Send(datagram, DeliveryMethod.ReliableOrdered);
        }

        public void PassData(byte[] datagram)
        {
            Timer_ReadPacket = 0;
            _ClientNode.Rx.Enqueue(datagram);
        }

        public void Dispose()
        {
            maintainConnecting.Stop();
            if (_NetPeer.ConnectionState == ConnectionState.Connected)
                _NetPeer.Disconnect();
            _ClientNode.OnDisconnect();
        }
    }
}
