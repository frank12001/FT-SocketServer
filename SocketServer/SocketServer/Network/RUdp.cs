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
        private readonly EventBasedNetListener _listener;
        private readonly NetManager _server;
        private readonly IPEndPoint _mIpEndPoint;
        public RUdp(SocketServer socketServer, IPEndPoint iPEndPoint) : base(socketServer)
        {
            _listener = new EventBasedNetListener();
            _server = new NetManager(_listener){ DisconnectTimeout = 60 * 1000};
            _mIpEndPoint = iPEndPoint;

            _listener.ConnectionRequestEvent += request =>
            {
                request.AcceptIfKey("SomeConnectionKey");
            };

            _listener.PeerConnectedEvent += peer =>
            {
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
                    RUdpInstance instance = new RUdpInstance(this,cNode,peer);
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

            _listener.NetworkReceiveEvent += (peer, reader, deliveryMethod)=> 
            {
                //reader.GetBytesWithLength
                if (ClientInstance.TryGetValue(peer.EndPoint.ToString(), out Instance instance))
                {
                    RUdpInstance client = (RUdpInstance)instance;
                    byte[] b = new byte[reader.AvailableBytes];
                    reader.GetBytes(b, reader.AvailableBytes);
                    //Console.WriteLine("We got something 2 : {0},Length: {1}", peer.EndPoint, b.Length); // Show peer ip
                    client.PassData(b);
                }
                reader.Recycle();
            };

            _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
            };

            Task.Run(async () =>
            {
                while (true)
                {
                    _server.PollEvents();
                    await Task.Delay(15);

                    if (_server.IsRunning)
                    {
                        return;
                    }
                }
            });
        }

        public override async Task StartListen()
        {
            _server.Start(_mIpEndPoint.Port);
        }

        public override async Task SendAsync(byte[] data, IPEndPoint endPoint)
        {
            if (ClientInstance.TryGetValue(endPoint.ToString(), out Instance instance))
            {
                await ((RUdpInstance)instance).Send(data);
            }
        }

        public override void DisConnect(IPEndPoint iPEndPoint)
        {
            lock (ClientInstance)
            {
                string key = iPEndPoint.ToString();
                if (ClientInstance.TryGetValue(key, out Instance instance))
                {
                    RUdpInstance rUdpInstance = (RUdpInstance)instance;
                    rUdpInstance.Dispose();
                    ClientInstance.Remove(key);
                }
            }
        }       
    }
    public class RUdpInstance : Instance , IDisposable
    {
        private const byte Tick_MainConnecting = 100;
        /// <summary>
        /// 斷線之time out時間長度
        /// </summary>
        private readonly ushort TimeLimit_Disconnect = 20 * 000;
        /// <summary>
        /// 接收封包及維持連線之Timer
        /// </summary>
        private Timer _maintainConnecting;
        /// <summary>
        /// 接收封包之時間間隔
        /// </summary>
        private ushort _timerReadPacket = 0;
        private readonly NetPeer _netPeer;
        private readonly RUdp _rUdp;
        public readonly IPEndPoint IpEndPoint;
        public RUdpInstance(RUdp rUdp,ClientNode clientNode, NetPeer netPeer) : base(clientNode)
        {
            _rUdp = rUdp;
            _netPeer = netPeer;
            IpEndPoint = _netPeer.EndPoint;
            BeginMaintainConnectingAsync();   // 開始進行維持連線之封包發送
        }
        /// <summary>
        /// 每隔一段時間定期進行連絡以確認維持連線
        /// </summary>
        private void BeginMaintainConnectingAsync()
        {
            _maintainConnecting = new Timer(Tick_MainConnecting);
            _maintainConnecting.Elapsed += Handler_MaintainConnecting;
            _maintainConnecting.Start();
        }
        private void Handler_MaintainConnecting(object o, ElapsedEventArgs e)
        {
            // 當維持連線之訊號中斷直到timeout，作斷線處理
            if (_timerReadPacket >= TimeLimit_Disconnect)
            {
                _maintainConnecting.Stop();
                _rUdp.DisConnect(IpEndPoint);
            }
            // 如果長時間未收到維持訊號
            _timerReadPacket += Tick_MainConnecting;
            if (_timerReadPacket >= 2000)
            {
                // 對客戶端發送維持連線之訊號
                byte[] buff = new byte[] { 0 };
                Send(buff);
            }
        }
        public async Task Send(byte[] data)
        {
            _netPeer.Send(data, DeliveryMethod.ReliableOrdered);
        }

        public void PassData(byte[] data)
        {
            _timerReadPacket = 0;
            _ClientNode.Rx.Enqueue(data);
        }

        public void Dispose()
        {
            _maintainConnecting.Stop();
            if (_netPeer.ConnectionState == ConnectionState.Connected)
                _netPeer.Disconnect();
            _ClientNode.OnDisconnect();
        }
    }
}
