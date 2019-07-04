using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FTServer
{
	public class Tcp : INetwork
	{
		//----tcp----//
		private TcpClient tcpClient;
		private const int BufferSize = 40960;
		private byte[] mRx;
        bool callForTcpClientClose = false;

        public Tcp() : base(NetworkProtocol.TCP)
		{ }

		public override void Connect(IPAddress address, int port)
		{
            tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
            tcpClient.Client.DualMode = true;
            if (!IPTool.IOSCheck(address, out address))
                address = address.MapToIPv6();
            try
            {
                tcpClient.Connect(address, port);
                if (tcpClient.Connected)
                {
                    fireCompleteConnect();
                    StartReceiveAsync(tcpClient);
                }
                else
                    fireCompleteDisconnect();
            }
            catch (SocketException e)
            {
                switch (e.ErrorCode)
                {
                    case (int)SocketError.ConnectionRefused:
                        fireCompleteDisconnect();
                        LogProxy.WriteLine(e.Message);
                        break;
                }            
            }
        }

        public override void BeginSend(byte[] datagram, int bytes)
        {
            if (tcpClient.Connected)
            {
                try
                {
                    tcpClient.GetStream().Write(datagram, 0, bytes);
                    fireCompleteSend();
                }
                catch (Exception e)
                {
                    LogProxy.WriteLine(e.Message);
                    // tell tcpclient don't do tcpClient.GetStream().Read
                    tcpClient.GetStream().Flush();
                }
            }
        }

        private async Task StartReceiveAsync(TcpClient tcpc)
        {
            Task.Run(async () => {
                while (true)
                {
                    try
                    {
                        byte[] buff = new byte[BufferSize];
                        int count = await tcpClient.GetStream().ReadAsync(buff, 0, buff.Length);

                        Array.Resize(ref buff, count);
                        if (count.Equals(0))
                        {
                            LogProxy.WriteLine("Get Packet Length = 0");
                            DisConnect();
                            break;
                        }
                        Task.Run(() =>{ try { fireCompleteReadFromServerStream(buff); } catch (Exception e) { LogProxy.WriteError(e.Message); } });
                    }
                    catch (Exception e)
                    {                  
                        if(!callForTcpClientClose)
                            DisConnect();
                        callForTcpClientClose = false;
                        LogProxy.WriteLine(e.Message);
                        break;
                    }
                }
            });
        }

        public override void DisConnect()
		{
            if (tcpClient.Connected)
            {
                callForTcpClientClose = true;
                tcpClient.Close();                
            }
            fireCompleteDisconnect();
        }
	}
}
