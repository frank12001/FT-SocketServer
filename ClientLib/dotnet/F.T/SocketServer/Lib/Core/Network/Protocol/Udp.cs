using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace FTServer
{
    public class Udp : INetwork
    {
        //----udp----//
        private UdpClient udpClient;
        /// <summary>
        /// 多久從序列中寫出或讀入一包
        /// </summary>
        private const float Tick_MainConnecting = 5000f;
        private const string SocketErrorMsgs = "Cannot access a disposed object.";
        private const string SocketErrorMsgs2 = "The socket is not connected";
        private const string SocketErrorMsgs3 = "The socket has been shut down";
        private readonly byte[] ReqConnect = new byte[] { 67, 111, 105, 110 };
        private readonly byte[] ReqDisconnect = new byte[] { 87, 241, 34, 124, 2 };
        private Timer maintainConnecting;
        private bool udpMaintainConnecting = false;

        private IPEndPoint serverIPEndPoint;

        public Udp() : base(NetworkProtocol.UDP)
        { }

        public override void Connect(IPAddress addr, int port)
        {
            LogProxy.WriteLine("<color=darkblue>Udp.Connect=>addr=" + addr + ",port=" + port + "</color>");

            if ((maintainConnecting == null) || !maintainConnecting.Enabled)
            {
                udpClient = new UdpClient(AddressFamily.InterNetworkV6);
                Socket newSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp) { DualMode = true };
                udpClient.Client = newSocket;

                if (!IPTool.IOSCheck(addr, out addr))
                    addr = addr.MapToIPv6();
                serverIPEndPoint = new IPEndPoint(addr.MapToIPv6(), port);

                udpClient.Send(ReqConnect, ReqConnect.Length, serverIPEndPoint);
                udpClient.BeginReceive(onCompleteConnect, udpClient);
            }
        }

        public override void BeginSend(byte[] datagram, int bytes)
        {
            if (udpClient != null)
            {
                udpClient.BeginSend(datagram, bytes, serverIPEndPoint, iar =>
                    {
                        try
                        {
                            UdpClient tcpc;
                            tcpc = (UdpClient)iar.AsyncState;
                            tcpc.EndSend(iar);
                        }
                        catch (SocketException socketException)
                        {
                            if (!socketException.Message.Contains(SocketErrorMsgs3))
                            {
                                LogProxy.WriteLine("Begin Send Socket Error : " + socketException.Message);
                            }
                            else
                            {
                                DisConnect();
                            }
                        }
                        fireCompleteSend();
                    }, udpClient);
            }
        }

        public override void DisConnect()
        {
            if (udpClient != null)
            {
                if (udpClient.Client.Connected)
                {
                    udpClient.Send(ReqDisconnect, ReqDisconnect.Length, serverIPEndPoint);
                    udpClient.Close();
                    udpClient = null;
                }
            }
            MaintainConnecting_Stop();
            fireCompleteDisconnect();
        }

        protected override void onCompleteConnect(IAsyncResult iar)
        {
            bool connectResult = false;
            UdpClient tcpc;
            try
            {
                tcpc = (UdpClient)iar.AsyncState;
                IPEndPoint iPEndPoint = null;
                byte[] receiveBytes = tcpc.EndReceive(iar, ref iPEndPoint);

                if (receiveBytes[0].Equals(1))
                {
                    udpClient.BeginReceive(onCompleteReadFromServerStream, udpClient);
                    connectResult = true;
                }

                MaintainConnecting_Start();
                fireCompleteConnect();
            }
            catch(Exception ex)
            {
                connectResult = false;
                LogProxy.WriteError(ex.StackTrace);
                fireCompleteDisconnect();

            }
        }
        protected override void onCompleteReadFromServerStream(IAsyncResult iar)
        {
            UdpClient tcpc;
            try
            {
                tcpc = (UdpClient)iar.AsyncState;
                //取得這次傳入資料的長度
                IPEndPoint iPEndPoint = null;
                byte[] receiveBytes = tcpc.EndReceive(iar, ref iPEndPoint);

                udpMaintainConnecting = true;
                fireCompleteReadFromServerStream(receiveBytes);
            }
            catch (Exception exc)
            {
                if (!exc.Message.Contains(SocketErrorMsgs) && !exc.Message.Contains(SocketErrorMsgs2))
                    LogProxy.WriteLine(exc.Message);
            }
            finally
            {
                if(udpClient !=null && udpClient.Client.Connected)
                    udpClient.BeginReceive(onCompleteReadFromServerStream, udpClient);
            }
        }

        private void Handler_MaintainConnecting(object sender, ElapsedEventArgs e)
        {
            if (!udpMaintainConnecting)            
                DisConnect();            
            else
                udpMaintainConnecting = false;
        }

        private void MaintainConnecting_Start()
        {
            MaintainConnecting_Stop();
            
            maintainConnecting = new Timer(Tick_MainConnecting);
            maintainConnecting.Elapsed += Handler_MaintainConnecting;
            maintainConnecting.Start();
        }
        
        private void MaintainConnecting_Stop()
        {
            if (maintainConnecting != null)
            {
                maintainConnecting.Stop();
                maintainConnecting = null;
            }
        }
    }
}