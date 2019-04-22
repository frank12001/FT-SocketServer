using System;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Collections.Generic;

namespace FTServer
{
	public class Tcp : INetwork
	{
		//----tcp----//
		private TcpClient tcpClient;
		private const int BufferSize = 40960;
		private byte[] mRx;

		/// <summary>
		/// 多久從序列中寫出或讀入一包
		/// </summary>
		private const float Tick_MainConnecting = 10000f;
		private Timer maintainConnecting;

		public Tcp() : base(NetworkProtocol.TCP)
		{ }

		public override void Connect(IPAddress address, int port)
		{
            tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
            tcpClient.Client.DualMode = true;
            if (!IPTool.IOSCheck(address, out address))
                address = address.MapToIPv6();
            tcpClient.BeginConnect(address, port, onCompleteConnect, tcpClient);
        }

		public override void BeginSend(byte[] datagram, int bytes)
		{
			tcpClient.GetStream().BeginWrite(datagram, 0, bytes, iar =>
				{
					TcpClient tcpc;
					tcpc = (TcpClient)iar.AsyncState;
					tcpc.GetStream().EndWrite(iar);

					fireCompleteSend();
				}, tcpClient);
		}

		public override void DisConnect()
		{
			if (tcpClient.Client.Connected)
			{
				tcpClient.Close();
				tcpClient = null;
				fireCompleteDisconnect();
			}
		}

		protected override void onCompleteConnect(IAsyncResult iar)
		{
			try
			{
				TcpClient tcpcc = (TcpClient)iar.AsyncState;
				tcpcc.EndConnect(iar);
				mRx = new byte[BufferSize];
				tcpcc.ReceiveBufferSize = (int)BufferSize;
				tcpcc.ReceiveTimeout = 10000;
				UnityEngine.Debug.Log("Available = " + tcpcc.Available);
				tcpcc.GetStream().BeginRead(mRx, 0, mRx.Length, onCompleteReadFromServerStream, tcpcc);

				maintainConnecting = new Timer(Tick_MainConnecting);
				maintainConnecting.Elapsed += Handler_MaintainConnecting;
				maintainConnecting.Start();
                fireCompleteConnect();
            }
			catch (Exception exc)
			{
				Console.WriteLine(exc.Message);
                fireCompleteDisconnect();
			}
		}

		protected override void onCompleteReadFromServerStream(IAsyncResult iar)
		{
			try
			{
				TcpClient tcpcc = (TcpClient)iar.AsyncState;
				//取得這次傳入資料的長度
				int nCountBytesReceivedFromServer = tcpcc.GetStream().EndRead(iar);
				Array.Resize(ref mRx, nCountBytesReceivedFromServer);
				byte[] receiveBytes = (byte[])mRx.Clone();
				fireCompleteReadFromServerStream(receiveBytes);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.Message);
			}
			finally
			{
				//重新給予暫存一個大小
				mRx = new byte[BufferSize];
				//開始等待封包
				tcpClient.GetStream().BeginRead(mRx, 0, mRx.Length, onCompleteReadFromServerStream, tcpClient);
			}
		}

		private void Handler_MaintainConnecting(object sender, ElapsedEventArgs e)
		{
			if (!tcpClient.Connected)
				fireCompleteDisconnect();
		}
	}
}
