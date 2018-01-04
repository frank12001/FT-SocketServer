﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using TCPServer.ClientInstance.Packet;
using TCPServer.ClientInstance.Interface.Input;
using TCPServer.ClientInstance.Interface.Output;
using System.Windows.Forms;

namespace TCPServer.ClientInstance
{
    /// <summary>
    /// 網路基本功能
    /// </summary>
    public class ClientNode : IEquatable<string> , IManagedPeer , ITCPClientCallBack
    {
        public TcpClient tclient;
        public byte[] Tx, Rx;
        /// <summary>
        /// 放 TcpClient 的 tclient.Client.RemoteEndPoint
        /// </summary>
        public string strId;

        private IApplication application;

        public ClientNode(TcpClient _tclient, byte[] _tx, byte[] _rx, string _str,IApplication applicationInterface)
        {
            tclient = _tclient;
            Tx = _tx;
            Rx = _rx;
            strId = _str;
            this.application = applicationInterface;

            //while (true)
            //{
            //    NetworkStream stream = tcpClient.GetStream();
            //    StreamReader reader = new StreamReader(stream);
            //    byte[] buff = new byte[64];

            //    int nRet = await stream.ReadAsync(buff, 0, buff.Length)
            //    string receivedText = new string(buff);
            //}
        }

        bool IEquatable<string>.Equals(string other)
        {
            if (string.IsNullOrEmpty(other)) return false;

            if (tclient == null) return false;

            return strId.Equals(other);
        }

        public override string ToString()
        {
            return strId;
        }

        public async void SendEvent(EventData eventData)
        {
            //取出選定的 ClientNode
            ClientNode cn = this;
            //初始化 ClientNode 的傳送暫存空間
            cn.Tx = new byte[application.InputBufferSize];
            try
            {
                //且 TCPClient 不為 null
                if (cn.tclient != null)
                {
                    //如果現在 TCPClient 有連線
                    if (cn.tclient.Client.Connected)
                    {
                        //將資料轉成 byte[] 
                        cn.Tx = Serializate(eventData);
                        //從暫存區把資料寫出，給對應到此 TCPClient 的 Client 端                        
                        await cn.tclient.GetStream().WriteAsync(cn.Tx, 0, cn.Tx.Length);
                        //MessageBox.Show("Send byte Array.length = " + cn.Tx.Length);
                        //application.PrintLine("Send byte Array.length = " + cn.Tx.Length);
                        //從暫存區把資料寫出，給對應到此 TCPClient 的 Client 端
                        //cn.tclient.GetStream().BeginWrite(cn.Tx, 0, cn.Tx.Length, cn.onCompleteWriteToClientStream, cn.tclient);
                    }
                }
            }
            catch (Exception exc)
            {
                application.PrintLine(exc.Message);
                //MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private byte[] Serializate(IPacket packet)
        {
            return TCPServer.Math.Serializate.ToByteArray(packet);
        }
        private IPacket DisSerializate(byte[] source)
        {
            return (IPacket)TCPServer.Math.Serializate.ToObject(source);
        }

        #region CallBack  onCompleteReadFromTCPClientStream , onCompleteWriteToClientStream

        /// <summary>
        /// 在 List 中的 ClientNode.tclient 接到 輸入的處理
        /// </summary>
        /// <param name="iar"></param>
        public void onCompleteReadFromTCPClientStream(IAsyncResult iar)
        {
            TcpClient tcpc;
            int nCountReadBytes = 0;
            string strRecv;
            ClientNode cn = null;

            try
            {
                lock (application.MlClientSockets)
                {
                    //將此次傳訊息進來的 TcpClient 存起來
                    tcpc = (TcpClient)iar.AsyncState;

                    //到這裡時對佔存區中的資料作處理，最後傳入 OnOperationRequest
                    //從 以連線 的 ClientNode 找出 strId = tcpc.Client.RemoteEndPoint.ToString()
                    cn = this;
                    //取得這次輸入串流的長度
                    nCountReadBytes = tcpc.GetStream().EndRead(iar);
                    //輸入長度為 0 ，代表斷線了
                    if (nCountReadBytes == 0)//this happens when the client is disconnected
                    {
                        //MessageBox.Show("Client disconnected.");
                        application.PrintLine("Client disconnected.");
                        application.MlClientSockets.Remove(cn);
                        application.LbClients.Items.Remove(cn.ToString());
                        OnDisconnect();
                        return;
                    }
                    //由於在上次的 BeginRead 已將cn.Rx當作緩存區 ， 所以直接將裡面的值拿出
                    //對傳入的 byte[] 做處理

                    //-------------------------------------
                    //解包
                    IPacket packet = (IPacket)TCPServer.Math.Serializate.ToObject(cn.Rx);
                    //application.PrintLine(packet.ForTest);
                    OperationRequest operationRequest = new OperationRequest(packet.OperationCode, new Dictionary<byte, object>(packet.Parameters)) { ForTest = packet.ForTest };
                    //回傳
                    OnOperationRequest(operationRequest);
                    //--------------------------------------


                    //重新給予緩存一個空間
                    cn.Rx = new byte[application.InputBufferSize];
                    //重新開始準備讀取
                    tcpc.GetStream().BeginRead(cn.Rx, 0, cn.Rx.Length, onCompleteReadFromTCPClientStream, tcpc);                   
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                application.PrintLine("Error onCompleteReadFromTCPClientStream : " + ex.ToString());

                //將此次傳訊息進來的 TcpClient 存起來
                tcpc = (TcpClient)iar.AsyncState;
                //重新給予緩存一個空間
                cn.Rx = new byte[application.InputBufferSize];
                //重新開始準備讀取
                tcpc.GetStream().BeginRead(cn.Rx, 0, cn.Rx.Length, onCompleteReadFromTCPClientStream, tcpc);

                //lock (application.MlClientSockets)
                //{
                //    application.PrintLine("Client disconnected: " + cn.ToString());
                //    application.MlClientSockets.Remove(cn);
                //    application.LbClients.Items.Remove(cn.ToString());
                //}
            }
        }

        #endregion 

        public virtual void OnOperationRequest(OperationRequest operationRequest)
        {
            application.PrintLine(DateTime.Now + " - " + this.ToString() + ": " + operationRequest.ForTest);
        }

        public virtual void OnDisconnect()
        {

        }

    }
}
