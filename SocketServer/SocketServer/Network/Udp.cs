using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FTServer.ClientInstance;
using System;
using System.Timers;
using FTServer.Log;

namespace FTServer.Network
{
    public class UDPInstance : Instance, IDisposable
    {
        private const byte Tick_MainConnecting = 100;
        /// <summary>
        /// 斷線之time out時間長度
        /// </summary>
        private readonly ushort TimeLimit_Disconnect = 5000;
        /// <summary>
        /// 接收封包及維持連線之Timer
        /// </summary>
        private Timer maintainConnecting;
        /// <summary>
        /// 接收封包之時間間隔
        /// </summary>
        private ushort Timer_ReadPacket = 0;
        //private Udp 
        private Udp _Udp;
        private UdpClient _UdpClient;
        public IPEndPoint IPEndPoint { get; private set; }
        public UDPInstance(Udp udp,UdpClient udpClient,IPEndPoint ipendPoint, ClientNode clientNode) : base(clientNode)
        {
            _Udp = udp;
            _UdpClient = udpClient;
            IPEndPoint = ipendPoint;
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
                _Udp.DisConnect(IPEndPoint);
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
            _UdpClient.SendAsync(datagram, datagram.Length,IPEndPoint);
        }

        public void PassData(byte[] datagram)
        {
            Timer_ReadPacket = 0;
            _ClientNode.Rx.Enqueue(datagram);
        }

        public void Dispose()
        {
            maintainConnecting.Stop();
            _ClientNode.OnDisconnect();
        }
    }
    public class Udp : Core
    {
        public readonly byte[] password = new byte[] { 67, 111, 105, 110 };
        public const int SIO_UDP_CONNRESET = -1744830452;
        private UdpClient _UdpClient;
 
        private Dictionary<string, ClientNode> ClientInstanceClone;
        private IPEndPoint M_IPEndPoint;
        private Queue<UdpReceiveResult> receiveResults;

        public Udp(SocketServer socketServer, IPEndPoint iPEndPoint) : base(socketServer)
        {
            _UdpClient = new UdpClient(AddressFamily.InterNetworkV6);
            Socket newSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);

            //當客戶端斷線時，不要讓錯誤跳出來 (udp不應該在客戶斷線時抱錯)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                newSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);

            newSocket.DualMode = true;
            IPEndPoint point = new IPEndPoint(IPAddress.IPv6Any, iPEndPoint.Port);
            newSocket.Bind(point);
            _UdpClient.Client = newSocket;
        }

        public override async Task SendAsync(byte[] datagram, IPEndPoint endPoint)
        {
            //修改成 Async //測試中 //不等候直接讓它過去 //測試當量大的時候 //不等後的風險
            if (ClientInstance.TryGetValue(endPoint.ToString(), out Instance instance))
            {
                await ((UDPInstance)instance).Send(datagram);
            }
        }

        public override async Task<ReceiveResult> ReceiveAsync()
        {
            UdpReceiveResult receiveResult;
            receiveResult = await _UdpClient.ReceiveAsync();

            //這個地方可以順便帶DEVICEID 如果發現有重複就應該擋掉
            var result = new ReceiveResult(receiveResult.RemoteEndPoint);
            if (password.Length == receiveResult.Buffer.Length)
            {
                for (int i = 0; i < receiveResult.Buffer.Length; i++)
                {
                    result.isOk = result.isOk && receiveResult.Buffer[i].Equals(password[i]);
                }
            }
            else
                result.isOk = false;

            lock (ClientInstance)
            {
                if (ClientInstance.TryGetValue(receiveResult.RemoteEndPoint.ToString(), out Instance instance))
                {
                    UDPInstance udpInstance = (UDPInstance)instance;
                    udpInstance.PassData(receiveResult.Buffer);
                }
            }

            string clientIp = receiveResult.RemoteEndPoint.ToString();
            if (!ClientInstance.ContainsKey(clientIp) && result.isOk)
            {
                // 建立玩家peer實體                   
                ClientNode cNode = _SocketServer.GetPeer(this, receiveResult.RemoteEndPoint, _SocketServer);
                UDPInstance instance = new UDPInstance(this, _UdpClient, receiveResult.RemoteEndPoint, cNode);
                //註冊到 mListener 中，讓他的 Receive 功能能被叫
                ClientInstance.Add(clientIp, instance);
                //成功加入後傳送 Connect 事件給 Client
                await SendAsync(new byte[] { 1 }, cNode.iPEndPoint);
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
