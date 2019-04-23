using FTServer.ClientInstance;
using FTServer.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FTServer.Network
{
    public class Tcp : Core
    {
        private const string ErrorMsg1 = "Unable to read data from the transport connection";
        private const int BufferSize = 40960;

        private TcpListener listener;
        public Tcp(SocketServer socketServer, IPEndPoint iPEndPoint) : base(socketServer)
        {
            listener = new TcpListener(IPAddress.IPv6Any, iPEndPoint.Port);
            listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);      
        }

        public override async Task StartListen()
        {
            listener.Start();
            await Task.Run(async () =>
             {
                 while (true)
                 {
                     try
                     {
                         TcpClient tcpc = await listener.AcceptTcpClientAsync();
                         //每個 client 都有一個 task 去等他的 StartReceiveAsync 
                         Task.Run(async () =>
                          {
                              CreateInstance(tcpc);
                              await StartReceiveAsync(tcpc);                             
                          });
                     }
                     catch (Exception e)
                     {
                         Printer.WriteError($"Accept connection failed : {e.Message}\n{e.StackTrace}");
                     }
                 }
             });
        }

        public override async Task SendAsync(byte[] datagram, IPEndPoint endPoint)
        {
            if (ClientInstance.TryGetValue(endPoint.ToString(), out Instance instance))
            {
                await ((TCPInstance)instance).Send(datagram);
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

        private TCPInstance CreateInstance(TcpClient tcpc)
        {
            IPEndPoint ipendpoint = tcpc.Client.RemoteEndPoint as IPEndPoint;
            string clientIp = ipendpoint.ToString();
            if (ClientInstance.ContainsKey(clientIp))
                ClientInstance.Remove(clientIp);
            // 建立玩家peer實體
            ClientNode cNode = _SocketServer.GetPeer(this, ipendpoint, _SocketServer);
            //註冊到 mListener 中，讓他的 Receive 功能能被叫               
            TCPInstance instance = new TCPInstance(this, cNode, tcpc);
            //註冊到 mListener 中，讓他的 Receive 功能能被叫
            ClientInstance.Add(clientIp, instance);
            //成功加入後傳送 Connect 事件給 Client
            byte[] packet = new byte[] { 1 };
            tcpc.GetStream().Write(packet, 0, packet.Length);            
            return instance;
        }

        private async Task StartReceiveAsync(TcpClient tcpc)
        {
            IPEndPoint ipendpoint = tcpc.Client.RemoteEndPoint as IPEndPoint;

            if (ClientInstance.TryGetValue(ipendpoint.ToString(), out Instance instance))
            {
                TCPInstance tcpInstance = (TCPInstance)instance;
              
                NetworkStream stream = tcpc.GetStream();
                await Task.Run(async () =>
                {
                    try
                    {
                        while (true &&  tcpc.Client.Connected)
                        {
                            byte[] buff = new byte[BufferSize];
                            int count = await stream.ReadAsync(buff, 0, buff.Length);
                            Array.Resize(ref buff, count);
                            if (count.Equals(0))
                            {
                                DisConnect(ipendpoint);
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
                            DisConnect(ipendpoint);
                        }
                    }
                });
            }
        }
    }
    public class TCPInstance : Instance, IDisposable
    {
        private TcpClient _TcpClient;
        private Tcp Tcp;
        public IPEndPoint IPEndPoint { get; private set; }
        public TCPInstance(Tcp tcp, ClientNode clientNode, TcpClient tcpClient) : base(clientNode)
        {
            Tcp = tcp;
            _TcpClient = tcpClient;
            IPEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
        }

        public async Task Send(byte[] datagram)
        {
            if (_TcpClient.Client.Connected)
                await _TcpClient.GetStream().WriteAsync(datagram, 0, datagram.Length);
        }

        public void PassData(byte[] datagram)
        {
            _ClientNode.Rx.Enqueue(datagram);
        }

        public void Dispose()
        {
            if (_TcpClient.Client.Connected)
                _TcpClient.Close();
            _ClientNode.OnDisconnect();
        }
    }
}
