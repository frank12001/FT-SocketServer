using FTServer.ClientInstance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FTServer.Network
{
    public class Tcp : Core
    {
        private const int BufferSize = 40960;

        private TcpListener mTCPListener;
        private Dictionary<string, TcpClient> clients;
        public Tcp(SocketServer socketServer, IPEndPoint iPEndPoint) : base(socketServer)
        {
            clients = new Dictionary<string, TcpClient>();
            //IPAddress.Any //ipaddr
            mTCPListener = new TcpListener(IPAddress.IPv6Any, iPEndPoint.Port);
            //mTCPListener = new TcpListener(iPEndPoint);
            mTCPListener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

            mTCPListener.Start();
        }

        public override async Task SendAsync(byte[] datagram, IPEndPoint endPoint)
        {
            if (clients.TryGetValue(endPoint.ToString(), out TcpClient tcpClient))
            {
                if (tcpClient.Connected)
                    await tcpClient.GetStream().WriteAsync(datagram, 0, datagram.Length);
                else
                    clients.Remove(endPoint.ToString());
            }
        }

        public override async Task<ReceiveResult> ReceiveAsync()
        {
            //等待有人連接
            TcpClient tcpc = await mTCPListener.AcceptTcpClientAsync();

            IPEndPoint iPEndPoint = CreateIPEndPoint(tcpc.Client.RemoteEndPoint.ToString());

            clients.Add(iPEndPoint.ToString(), tcpc);

            BeginReadAsync(tcpc, iPEndPoint);

            return new ReceiveResult(iPEndPoint);
        }

        public override void DisConnect(IPEndPoint iPEndPoint)
        {
            base.DisConnect(iPEndPoint);
            if (clients.TryGetValue(iPEndPoint.ToString(), out TcpClient tcpc))
            {
                tcpc.Dispose();
                tcpc.Close();
            }
        }

        /// <summary>
        /// 開始等待封包傳入
        /// </summary>
        private async Task BeginReadAsync(TcpClient tclient, IPEndPoint iPEndPoint)
        {
            NetworkStream stream = tclient.GetStream();

            while (true)
            {
                byte[] buff = new byte[BufferSize];


                try
                {
                    int bufferCount = await Task.Run(() =>
                    {
                        Task<int> i = null;
                        try
                        {
                            i = stream.ReadAsync(buff, 0, buff.Length);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Tcp.cs Line73");
                        }
                        return i;
                    }
                    );
                    Array.Resize(ref buff, bufferCount);
                    if (bufferCount.Equals(0))
                        continue;

                    // 將接收到的buff data送入client佇列等候處理
                    if (ClientInstance.TryGetValue(iPEndPoint.ToString(), out Instance instance))
                    {
                        instance._ClientNode.Rx.Enqueue(buff);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Tcp.cs Line91");
                    clients.Remove(iPEndPoint.ToString());
                    break;
                }
            }
        }

        private IPEndPoint CreateIPEndPoint(string endPoint)
        {
            //string[] ep = endPoint.Split(':');
            //if (ep.Length != 2) throw new FormatException("Invalid endpoint format");
            //IPAddress ip;
            //if (!IPAddress.TryParse(ep[0], out ip))
            //{
            //    throw new FormatException("Invalid ip-adress");
            //}
            //int port;
            //if (!int.TryParse(ep[1], System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.CurrentInfo, out port))
            //{
            //    throw new FormatException("Invalid port");
            //}
            //return new IPEndPoint(ip, port);
            return Parse(endPoint);
        }

        public IPEndPoint Parse(string endpointstring)
        {
            return Parse(endpointstring, -1);
        }

        public IPEndPoint Parse(string endpointstring, int defaultport)
        {
            if (string.IsNullOrEmpty(endpointstring)
                || endpointstring.Trim().Length == 0)
            {
                throw new ArgumentException("Endpoint descriptor may not be empty.");
            }

            if (defaultport != -1 &&
                (defaultport < IPEndPoint.MinPort
                || defaultport > IPEndPoint.MaxPort))
            {
                throw new ArgumentException(string.Format("Invalid default port '{0}'", defaultport));
            }

            string[] values = endpointstring.Split(new char[] { ':' });
            IPAddress ipaddy;
            int port = -1;

            //check if we have an IPv6 or ports
            if (values.Length <= 2) // ipv4 or hostname
            {
                if (values.Length == 1)
                    //no port is specified, default
                    port = defaultport;
                else
                    port = getPort(values[1]);

                //try to use the address as IPv4, otherwise get hostname
                if (!IPAddress.TryParse(values[0], out ipaddy))
                    ipaddy = getIPfromHost(values[0]);
            }
            else if (values.Length > 2) //ipv6
            {
                //could [a:b:c]:d
                if (values[0].StartsWith("[") && values[values.Length - 2].EndsWith("]"))
                {
                    string ipaddressstring = string.Join(":", values.Take(values.Length - 1).ToArray());
                    ipaddy = IPAddress.Parse(ipaddressstring);
                    port = getPort(values[values.Length - 1]);
                }
                else //[a:b:c] or a:b:c
                {
                    ipaddy = IPAddress.Parse(endpointstring);
                    port = defaultport;
                }
            }
            else
            {
                throw new FormatException(string.Format("Invalid endpoint ipaddress '{0}'", endpointstring));
            }

            if (port == -1)
                throw new ArgumentException(string.Format("No port specified: '{0}'", endpointstring));

            return new IPEndPoint(ipaddy, port);
        }

        private int getPort(string p)
        {
            int port;

            if (!int.TryParse(p, out port)
             || port < IPEndPoint.MinPort
             || port > IPEndPoint.MaxPort)
            {
                throw new FormatException(string.Format("Invalid end point port '{0}'", p));
            }

            return port;
        }

        private IPAddress getIPfromHost(string p)
        {
            var hosts = Dns.GetHostAddresses(p);

            if (hosts == null || hosts.Length == 0)
                throw new ArgumentException(string.Format("Host not found: {0}", p));

            return hosts[0];
        }
    }
}
