using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using TCPServer.ClientInstance.Packet;

namespace FTServer
{
    public abstract class NetWorkBase 
    {
        /// <summary>
        /// 伺服器的 ip : port
        /// </summary>
        public string Address = "104.199.181.106" + ":" + "5055";
        /// <summary>
        /// .Net TCP Client 物件
        /// </summary>
        private TcpClient mTcpClient = null;
        /// <summary>
        /// 接收封包暫存區
        /// </summary>
        private byte[] mRx = new byte[InputBufferSize];
        /// <summary>
        /// 接收封包暫存區大小 (單位 : byte)
        /// </summary>
        private const uint InputBufferSize = 40960;

        private SmallPacketSender smallPacketSender=null;

        /// <summary>
        /// 接收封包暫存區
        /// </summary>
        private QueueLine<EventData> SocketReceiver = new QueueLine<EventData>();
        /// <summary>
        /// 發送封包暫存區
        /// </summary>
        private List<PacketTemp> SocketSender = new List<PacketTemp>();

        private struct PacketTemp
        {
            public byte code;
            public Dictionary<byte, object> PacketDictionary;
        }
        /// <summary>
        /// 是否可以開始寫出封包
        /// </summary>
        private bool CanSend = true;

        /// <summary>
        /// 需要放在 Update 中一直執行的功能
        /// </summary>
        public void Service()
        {

            SocketReceiver.ChangeCurrectLine();
            List<EventData> packetsList = SocketReceiver.GetUnUseLine();
            SocketReceiver.ClearUnUseLine();

            if (packetsList != null)
            {
                while (packetsList.Count > 0)
                {
                    OnEvent(packetsList[0]);
                    packetsList.RemoveAt(0);
                }
            }

            //每偵只送一包
            if (SocketSender.Count > 0 && CanSend)
            {
                deliver(SocketSender[0].code, SocketSender[0].PacketDictionary);
                SocketSender.RemoveAt(0);
            }

            //if (smallPacketSender != null)
            //{
            //    smallPacketSender.Update();
            //}
        }


        /// <summary>
        /// 傳送
        /// </summary>
        /// <param name="code">Operation Code</param>
        /// <param name="dic"></param>
        public void Deliver(byte code, Dictionary<byte, object> dic)
        {
            SocketSender.Add(new PacketTemp(){ code = code, PacketDictionary = dic });
        }

        /// <summary>
        /// 傳送一個 Dictionary 給 Server ，目前 Dictionary.Value 不支援不能序列化的物件，例如 Dictionary 。不能使用多層 Dictionary
        /// </summary>
        /// <param name="code">此封包的索引碼，在伺服器中用此作為索引</param>
        /// <param name="dic">傳給伺服器的封包</param>
        public void deliver(byte code, Dictionary<byte, object> dic)
        {    //普通的傳送
            CanSend = false;
            //宣告一個傳送暫存
            byte[] tx;
            //將傳入的 code , dic , 傳換為我定義的封包
            OperationRequest packet = new OperationRequest(code, new Dictionary<byte, object>(dic)) {};
            try
            {
                //將此封包，序列化並加入暫存區
                tx = Serializate(packet);
                Debug.Log("發送 : 封包長度 " + tx.Length);
                if (mTcpClient != null)
                {
                    if (mTcpClient.Client.Connected) //如果有連線
                    {
                        //寫道輸出串流中
                        mTcpClient.GetStream().BeginWrite(tx, 0, tx.Length, onCompleteWriteToServer, mTcpClient);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogError(exc.Message);
            }
        }
        /// <summary>
        /// 連線到伺服器。連線到伺服器時， isConnect == true
        /// </summary>
        /// <returns>是否執行成功</returns>
        public bool ConnectToServer()
        {
            IPAddress ipa = null;
            int nPort;

            string ip = "";
            string port = "";
            string[] s = Address.Split(new Char[] { ':' });

            try
            {
                ip = s[0];
                port = s[1];
                if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
                    return false;
                if (!IPAddress.TryParse(ip, out ipa))
                {
                    Debug.Log("Please supply an IP Address.");
                    return false;
                }

                if (!int.TryParse(port, out nPort))
                {
                    nPort = 23000;
                }

                mTcpClient = new TcpClient();
                mTcpClient.BeginConnect(ipa, nPort, onCompleteConnect, mTcpClient);
                return true;
            }
            catch (Exception exc)
            {
                Debug.LogError(exc.Message);
                return false;
            }
        }
        /// <summary>
        /// 強制斷線
        /// </summary>
        public void DisConnect()
        {
            if (mTcpClient != null)
            {
                if (mTcpClient.Client.Connected)
                {
                    mTcpClient.Close();
                    mTcpClient = null;
                    OnStatusChanged(StatusCode.Disconnect);
                }
            }
        }

        protected enum StatusCode : byte { Connect, Disconnect };
        /// <summary>
        /// 當連線狀態改變時呼叫
        /// </summary>
        /// <param name="statusCode"></param>
        protected virtual void OnStatusChanged(StatusCode statusCode)
        {}

        /// <summary>
        /// 在主執行續執行的處理
        /// </summary>
        /// <param name="eventData"></param>
        protected virtual void OnEvent(EventData eventData)
        {}

        #region 回呼處理
        /// <summary>
        /// 連線伺服器成功後的回呼
        /// </summary>
        /// <param name="iar"></param>
        private void onCompleteConnect(IAsyncResult iar)
        {
            TcpClient tcpc;

            try
            {
                tcpc = (TcpClient)iar.AsyncState;
                tcpc.EndConnect(iar);
                mRx = new byte[InputBufferSize];
                mTcpClient.ReceiveBufferSize = (int)InputBufferSize;
                mTcpClient.ReceiveTimeout = 10000;
                Debug.Log("Available = " + mTcpClient.Available);
                tcpc.GetStream().BeginRead(mRx, 0, mRx.Length, onCompleteReadFromServerStream, tcpc);
                //this.smallPacketSender = new SmallPacketSender (Deliver,2);
                //smallPacketSender.StartSend();
                OnStatusChanged(StatusCode.Connect);
            }
            catch (Exception exc)
            {
                Debug.Log(exc.Message);
                OnStatusChanged(StatusCode.Disconnect);
            }
        }
        /// <summary>
        /// 有資料送進來時的回呼
        /// </summary>
        /// <param name="iar"></param>
        private void onCompleteReadFromServerStream(IAsyncResult iar)
        {
            TcpClient tcpc;
            int nCountBytesReceivedFromServer;

            try
            {
                tcpc = (TcpClient)iar.AsyncState;
                //取得這次傳入資料的長度
                nCountBytesReceivedFromServer = tcpc.GetStream().EndRead(iar);
                if (nCountBytesReceivedFromServer == 0) //如果傳入資料的等於零
                {
                    Debug.Log("Connection broken");
                    OnStatusChanged(StatusCode.Disconnect);
                    return;
                }
                //解包成我定義的封包
                IPacket packet = DisSerializate(mRx);
                //if (!this.smallPacketSender.Ignore(packet))
                //{
                //轉換為 EventData 並回傳
                EventData eventData =
                    new EventData(packet.OperationCode, new Dictionary<byte, object>(packet.Parameters));
                //將資料存入全域暫存區
                SocketReceiver.AddToCurrectLine(eventData);
                //}
                //else
                //{
                //Debug.Log("SP Packet In");
                //}
                //重新給予暫存一個大小
                mRx = new byte[InputBufferSize];
                tcpc.ReceiveBufferSize = (int)InputBufferSize;
                //開始等待封包
                tcpc.GetStream().BeginRead(mRx, 0, mRx.Length, onCompleteReadFromServerStream, tcpc);
            }
            catch (Exception exc)
            {

                Debug.LogError("onCompleteReadFromServerStream Error " + exc.Message);

                tcpc = (TcpClient)iar.AsyncState;
                if (tcpc.Connected)
                {
                    //重新給予暫存一個大小
                    mRx = new byte[InputBufferSize];
                    tcpc.ReceiveBufferSize = (int) InputBufferSize;
                    //開始等待封包
                    tcpc.GetStream().BeginRead(mRx, 0, mRx.Length, onCompleteReadFromServerStream, tcpc);
                }
            }

        }
        /// <summary>
        /// 將資料成功寫出後的回呼
        /// </summary>
        /// <param name="iar"></param>
        private void onCompleteWriteToServer(IAsyncResult iar)
        {
            TcpClient tcpc;
            try
            {
                tcpc = (TcpClient)iar.AsyncState;
                tcpc.GetStream().EndWrite(iar);
                CanSend = true;
            }
            catch (Exception exc)
            {
                Debug.LogError(exc.Message);
            }
        }

        #endregion 

        protected byte[] Serializate(IPacket packet)
        {
            return FTServer.Serializate.ToByteArray(packet);
        }
        protected IPacket DisSerializate(byte[] source)
        {
            return (IPacket)FTServer.Serializate.ToObject(source);
        }

    }

    public class SmallPacketSender
    {
        private bool StartTimer = true;

        private float Timer = 0, MaxTimer = 2;
        private bool canSend = false;

        private Action<byte, Dictionary<byte, object>> Deliver;

        public SmallPacketSender(Action<byte, Dictionary<byte, object>> deliver, float sendInterval)
        {
            this.Deliver = deliver;
            this.MaxTimer = sendInterval;
        }

        public void Update()
        {
            if (StartTimer && canSend)
            {
                if (Timer >= MaxTimer)
                {
                    Dictionary<byte, object> packet = new Dictionary<byte, object>()
                    {
                        { (byte)0,200 }, //switch code 維持長連線封包
                        { (byte)1,0 }, 
                        { (byte)2,1 }
                    };
                    Deliver(200, packet);
                    Timer = 0;
                }
                Timer += Time.deltaTime;
            }
        }

        public bool Ignore(IPacket packet)
        {
            return packet.OperationCode.Equals(200);
        }

        public void StartSend()
        {
            canSend = true;
        }

        public void StopSend()
        {
            canSend = false;
        }
    }
}