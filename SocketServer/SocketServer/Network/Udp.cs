using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FTServer.ClientInstance;
using System;
using System.Timers;

namespace FTServer.Network
{
    public class UDPInstance : Instance, IDisposable
    {
        private const byte TickMainConnecting = 100;
        /// <summary>
        /// 斷線之time out時間長度
        /// </summary>
        private readonly ushort _timeLimitDisconnect = 5000;
        /// <summary>
        /// 接收封包及維持連線之Timer
        /// </summary>
        private Timer _maintainConnecting;
        /// <summary>
        /// 接收封包之時間間隔
        /// </summary>
        private ushort _timerReadPacket = 0;
        //private Udp 
        private readonly Udp _udp;
        private readonly UdpClient _udpClient;
        public readonly IPEndPoint IpEndPoint;
        public UDPInstance(Udp udp,UdpClient udpClient,IPEndPoint ipEndPoint, ClientNode clientNode) : base(clientNode)
        {
            _udp = udp;
            _udpClient = udpClient;
            IpEndPoint = ipEndPoint;
            BeginMaintainConnectingAsync();   // 開始進行維持連線之封包發送
        }
        /// <summary>
        /// 每隔一段時間定期進行連絡以確認維持連線
        /// </summary>
        private void BeginMaintainConnectingAsync()
        {
            _maintainConnecting = new Timer(TickMainConnecting);
            _maintainConnecting.Elapsed += Handler_MaintainConnecting;
            _maintainConnecting.Start();
        }
        private void Handler_MaintainConnecting(object o, ElapsedEventArgs e)
        {
            // 當維持連線之訊號中斷直到timeout，作斷線處理
            if (_timerReadPacket >= _timeLimitDisconnect)
            {
                _udp.DisConnect(IpEndPoint);
            }
            // 如果長時間未收到維持訊號
            _timerReadPacket += TickMainConnecting;
            if (_timerReadPacket >= 2000)
            {
                // 對客戶端發送維持連線之訊號
                byte[] buff = new byte[] { 0 };
                Send(buff);
            } 
        }
        public async Task Send(byte[] data)
        {             
            _udpClient.SendAsync(data, data.Length,IpEndPoint);
        }

        public void PassData(byte[] data)
        {
            _timerReadPacket = 0;
            lock (_ClientNode.Rx)
            {
                _ClientNode.Rx.Enqueue(data);
            }
        }

        public void Dispose()
        {
            _maintainConnecting.Stop();
            _ClientNode.OnDisconnect();
        }
    }
    public class Udp : Core
    {
        private readonly byte[] ReqConnect = new byte[] { 67, 111, 105, 110 };
        private readonly byte[] ReqDisconnect = new byte[] { 87, 241, 34, 124, 2 };
        public const int PacketLengthLimit = 1300;
        public const string ExceptionMsg1 = "packet length can't bigger than 1300.";
        public const int SioUdpConnectReset = -1744830452;
        private readonly UdpClient _udpClient;
 
        private Dictionary<string, ClientNode> ClientInstanceClone;
        private IPEndPoint M_IPEndPoint;
        private Queue<UdpReceiveResult> receiveResults;

        public Udp(SocketServer socketServer, IPEndPoint iPEndPoint) : base(socketServer)
        {
            _udpClient = new UdpClient(AddressFamily.InterNetworkV6);
            Socket newSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);

            //當客戶端斷線時，不要讓錯誤跳出來 (udp不應該在客戶斷線時抱錯)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                newSocket.IOControl((IOControlCode)SioUdpConnectReset, new byte[] { 0, 0, 0, 0 }, null);

            newSocket.DualMode = true;
            IPEndPoint point = new IPEndPoint(IPAddress.IPv6Any, iPEndPoint.Port);
            newSocket.Bind(point);
            _udpClient.Client = newSocket;
        }

        public override async Task SendAsync(byte[] data, IPEndPoint endPoint)
        {
            //修改成 Async //測試中 //不等候直接讓它過去 //測試當量大的時候 //不等後的風險
            if (ClientInstance.TryGetValue(endPoint.ToString(), out Instance instance))
            {
                await ((UDPInstance)instance).Send(data);
            }
        }

        public override async Task<ReceiveResult> ReceiveAsync()
        {
            UdpReceiveResult receiveResult;
            receiveResult = await _udpClient.ReceiveAsync();

            //這個地方可以順便帶 device Id 如果發現有重複就應該擋掉
            var result = new ReceiveResult(receiveResult.RemoteEndPoint);
            if (ReqConnect.Length == receiveResult.Buffer.Length)
            {
                for (int i = 0; i < receiveResult.Buffer.Length; i++)
                {
                    result.IsOk = result.IsOk && receiveResult.Buffer[i].Equals(ReqConnect[i]);
                }
            }
            else
                result.IsOk = false;

            bool b = true;
            if (ReqDisconnect.Length == receiveResult.Buffer.Length)
            {
                for (int i = 0; i < receiveResult.Buffer.Length; i++)
                {
                    b = b && receiveResult.Buffer[i].Equals(ReqConnect[i]);
                }
            }
            if (!b)
                DisConnect(receiveResult.RemoteEndPoint);

            lock (ClientInstance)
            {
                if (ClientInstance.TryGetValue(receiveResult.RemoteEndPoint.ToString(), out Instance instance))
                {
                    UDPInstance udpInstance = (UDPInstance)instance;
                    udpInstance.PassData(receiveResult.Buffer);
                }
            }

            string clientIp = receiveResult.RemoteEndPoint.ToString();
            if (!ClientInstance.ContainsKey(clientIp) && result.IsOk)
            {
                // 建立玩家peer實體                   
                ClientNode cNode = _SocketServer.GetPeer(this, receiveResult.RemoteEndPoint, _SocketServer);
                UDPInstance instance = new UDPInstance(this, _udpClient, receiveResult.RemoteEndPoint, cNode);
                //註冊到 mListener 中，讓他的 Receive 功能能被叫
                ClientInstance.Add(clientIp, instance);
                //成功加入後傳送 Connect 事件給 Client
                await SendAsync(new byte[] { 1 }, cNode.IpEndPoint);
            }

            return result;
        }

        public override async Task StartListen()
        {
            await Task.Run(async () =>
             {
                 while (true)
                 {
                     await ReceiveAsync();
                 }
             });
        }

        public override void DisConnect(IPEndPoint iPEndPoint)
        {
            lock (ClientInstance)
            {
                string key = iPEndPoint.ToString();
                if (ClientInstance.TryGetValue(key, out Instance instance))
                {
                    UDPInstance udpInstance = (UDPInstance)instance;
                    udpInstance.Dispose();
                    ClientInstance.Remove(key);
                }
            }
        }
    }
}
