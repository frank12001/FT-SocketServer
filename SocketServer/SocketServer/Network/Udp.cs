using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FTServer.ClientInstance;

namespace FTServer.Network
{
    public class Udp : Core
    {
        private UdpClient _UdpClient;

        public const int SIO_UDP_CONNRESET = -1744830452;
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
            _UdpClient.SendAsync(datagram, datagram.Length, endPoint);
        }

        public readonly byte[] password = new byte[]{ 67, 111, 105, 110 };
        public override async Task<ReceiveResult> ReceiveAsync()
        {
            UdpReceiveResult receiveResult;
            receiveResult = await _UdpClient.ReceiveAsync();

            //System.Console.WriteLine("Udp Receive : "+receiveResult.RemoteEndPoint);
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
                if (ClientInstance.TryGetValue(receiveResult.RemoteEndPoint.ToString(), out ClientNode clientNode))
                {
                    clientNode.Rx.Enqueue(receiveResult.Buffer);
                }
            }

            return result;
        }

        public override void DisConnect(IPEndPoint iPEndPoint)
        {
            lock (ClientInstance)
            {
                base.ClientInstance.Remove(iPEndPoint.ToString());
            }
        }

        /// <summary>
        /// 開始將該 UdpClient ，取得的封包加入排成
        /// </summary>
        /// <param name="udp">指定的 UdpClient </param>
        private void StartListen(UdpClient udp)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    UdpReceiveResult r = await udp.ReceiveAsync();
                    if (r != null)
                    {
                        if (r.RemoteEndPoint != null && r.Buffer != null)
                        {
                            lock (receiveResults)
                            {
                                receiveResults.Enqueue(r);
                            }
                        }
                    }
                }
            });
        }
    }
}
