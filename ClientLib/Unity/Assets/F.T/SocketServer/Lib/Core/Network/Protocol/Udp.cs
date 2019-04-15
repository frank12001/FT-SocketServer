using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using UnityEngine;

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
        private Timer maintainConnecting;
        private bool udpMaintainConnecting = false;

        private IPEndPoint serverIPEndPoint;

        public Udp() : base(NetworkProtocol.UDP)
        { }

        public override void Connect(IPAddress addr, int port)
        {
            Debug.Log("<color=darkblue>Udp.Connect=>addr=" + addr + ",port=" + port + "</color>");

            if ((maintainConnecting == null) || !maintainConnecting.Enabled)
            {
                udpClient = new UdpClient(AddressFamily.InterNetworkV6);
                Socket newSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp) { DualMode = true };
                udpClient.Client = newSocket;

                if (!IPTool.IOSCheck(addr, out addr))
                    addr = addr.MapToIPv6();
                serverIPEndPoint = new IPEndPoint(addr.MapToIPv6(), port);

                //udpClient.Send(new byte[] { 1 }, 1, serverIPEndPoint);
                udpClient.Send(new byte[] { 67, 111, 105, 110 }, 4, serverIPEndPoint);
                udpClient.BeginReceive(onCompleteConnect, udpClient);
            }
        }

        public override void BeginSend(byte[] datagram, int bytes)
        {
            if (udpClient != null)
            {
                udpClient.BeginSend(datagram, bytes, serverIPEndPoint, iar =>
                {
                    UdpClient tcpc;
                    tcpc = (UdpClient) iar.AsyncState;
                    tcpc.EndSend(iar);

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
            }
            catch(Exception ex)
            {
                connectResult = false;
                Debug.LogError(ex.StackTrace);
            }
            finally
            {
                fireCompleteConnect(connectResult);
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
                Debug.Log(exc.Message);
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