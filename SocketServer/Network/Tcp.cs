using FTServer.ClientInstance;
using FTServer.Log;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FTServer.Network
{
    public class Tcp : Core
    {
        private const string ErrorMsg1 = "Unable to read data from the transport connection";
        private const int BufferSize = 40960;

        private readonly TcpListener _listener;
        public Tcp(SocketServer socketServer, IPEndPoint iPEndPoint) : base(socketServer)
        {
            _listener = new TcpListener(IPAddress.IPv6Any, iPEndPoint.Port);
            _listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);      
        }

        public override async Task StartListen()
        {
            _listener.Start();
            await Task.Run(async () =>
             {
                 while (true)
                 {
                     try
                     {
                         TcpClient tcp = await _listener.AcceptTcpClientAsync();
                         //每個 client 都有一個 task 去等他的 StartReceiveAsync 
                         Task.Run(async () =>
                          {
                              if (CreateInstance(tcp))
                              {
                                  await StartReceiveAsync(tcp);
                              }
                          });
                     }
                     catch (Exception e)
                     {
                         Printer.WriteError($"Accept connection failed : {e.Message}\n{e.StackTrace}");
                     }
                 }
             });
        }

        public override async Task SendAsync(byte[] data, IPEndPoint endPoint)
        {
            if (ClientInstance.TryGetValue(endPoint.ToString(), out Instance instance))
            {
                await ((TCPInstance)instance).Send(data);
            }
            else
            {
                Console.WriteLine("GG");
            }
        }

        public override void DisConnect(IPEndPoint iPEndPoint)
        {
            lock (ClientInstance)
            {
                string key = iPEndPoint.ToString();
                if (ClientInstance.TryGetValue(key, out Instance instance))
                {
                    TCPInstance tcpInstance = (TCPInstance)instance;
                    tcpInstance.Dispose();
                    ClientInstance.Remove(key);
                }
            }
        }

        private bool CreateInstance(TcpClient tcp)
        {
            try
            {
                IPEndPoint ipEndPoint = tcp.Client.RemoteEndPoint as IPEndPoint;
                if (ipEndPoint == null)
                {
                    return false;
                }
                string clientIp = ipEndPoint.ToString();
                if (ClientInstance.ContainsKey(clientIp))
                    ClientInstance.Remove(clientIp);
                // 建立玩家peer實體
                ClientNode cNode = _SocketServer.GetPeer(this, ipEndPoint, _SocketServer);
                //註冊到 mListener 中，讓他的 Receive 功能能被叫               
                TCPInstance instance = new TCPInstance(this, cNode, tcp);
                //註冊到 mListener 中，讓他的 Receive 功能能被叫
                ClientInstance.Add(clientIp, instance);
                //成功加入後傳送 Connect 事件給 Client
                byte[] packet = new byte[] { 1 };
                tcp.GetStream().Write(packet, 0, packet.Length);
                cNode.Initialize();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
       
            return true;
        }

        private async Task StartReceiveAsync(TcpClient tcp)
        {
            IPEndPoint ipEndPoint = tcp.Client.RemoteEndPoint as IPEndPoint;

            if (ClientInstance.TryGetValue(ipEndPoint.ToString(), out Instance instance))
            {
                TCPInstance tcpInstance = (TCPInstance)instance;
              
                NetworkStream stream = tcp.GetStream();
                await Task.Run(async () =>
                {
                    try
                    {
                        while (tcp.Client.Connected)
                        {
                            byte[] buff = new byte[BufferSize];
                            int count = await stream.ReadAsync(buff, 0, buff.Length);
                            Array.Resize(ref buff, count);
                            if (count.Equals(0))
                            {
                                DisConnect(ipEndPoint);
                                break;
                            }
                            // 將接收到的buff data送入client佇列等候處理
                            tcpInstance.PassData(buff);
                        }
                    }
                    catch (IOException e)
                    {
                        if (!e.Message.Contains(ErrorMsg1))
                            Console.WriteLine(e.Message + "," + e.HResult);
                        else
                        {
                            DisConnect(ipEndPoint);
                        }
                    }
                });
            }
        }
    }
    public class TCPInstance : Instance, IDisposable
    {
        private readonly TcpClient _TcpClient;
        private readonly Tcp _tcp;
        public readonly IPEndPoint IpEndPoint;
        public TCPInstance(Tcp tcp, ClientNode clientNode, TcpClient tcpClient) : base(clientNode)
        {
            _tcp = tcp;
            _TcpClient = tcpClient;
            IpEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
        }

        public async Task Send(byte[] data)
        {
            if (_TcpClient.Client.Connected)
                await _TcpClient.GetStream().WriteAsync(data, 0, data.Length);
        }

        public void PassData(byte[] data)
        {
            _ClientNode.Rx.Enqueue(data);
        }

        public void Dispose()
        {
            if (_TcpClient.Client.Connected)
                _TcpClient.Close();
            _ClientNode.OnDisconnect();
        }
    }
}
