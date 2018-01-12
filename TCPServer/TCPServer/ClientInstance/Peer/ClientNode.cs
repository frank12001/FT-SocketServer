using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using TCPServer.ClientInstance.Packet;
using TCPServer.ClientInstance.Interface.Output;
using System.Windows.Forms;

namespace TCPServer.ClientInstance
{
    /// <summary>
    /// 網路基本功能
    /// </summary>
    public class ClientNode : IEquatable<string> , IManagedPeer
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

            //開始接受封包傳入
            BeginReadAsync();            
        }

        ~ClientNode()
        {
            application.PrintLine("ClientNode 解構子觸發.");
        }


        public bool Reading = false;

        /// <summary>
        /// 開始等待封包傳入
        /// </summary>
        private async void BeginReadAsync()
        {
            NetworkStream stream = tclient.GetStream();
            byte[] buff = new byte[application.InputBufferSize];

            while (true)
            {
                int nCountReadBytes = 0;
                try
                {
                    nCountReadBytes = await stream.ReadAsync(buff, 0, buff.Length);
                }
                catch (Exception e)
                {
                    application.PrintLine(" stream.ReadAsync錯誤 : " + e.Message);
                }

                ClientNode cn = this;

                //輸入長度為 0 ，代表斷線了
                if (nCountReadBytes == 0) //this happens when the client is disconnected
                {
                    //MessageBox.Show("Client disconnected.");
                    application.PrintLine("Client disconnected.");
                    application.MlClientSockets.Remove(cn);
                    application.LbClients.Items.Remove(cn.ToString());
                    OnDisconnect();
                    cn.tclient.Close();
                    return;
                }
                try
                {
                    //-------------------------------------
                    //解包
                    IPacket packet = (IPacket)TCPServer.Math.Serializate.ToObject(buff);
                    OperationRequest operationRequest =
                        new OperationRequest(packet.OperationCode, new Dictionary<byte, object>(packet.Parameters));
                    //回傳
                    OnOperationRequest(operationRequest);
                    //--------------------------------------
                }
                catch (Exception exception)
                {
                    application.PrintLine(" 解封包錯誤 : "+exception.Message);
                }              
            }
        }
        /// <summary>
        /// 將資料寫出
        /// </summary>
        /// <param name="eventData"></param>
        public void Write(EventData eventData)
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
                        
                        //寫出後不用做事 //如果需要的話使用 await 等待
                        cn.tclient.GetStream().WriteAsync(cn.Tx, 0, cn.Tx.Length);
                    }
                }
            }
            catch (Exception exc)
            {
                application.PrintLine(exc.Message);
                //MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// 將資料寫出
        /// </summary>
        /// <param name="eventData"></param>
        public async Task WriteAsync(EventData eventData)
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

                        //寫出後不用做事 //如果需要的話使用 await 等待
                        await cn.tclient.GetStream().WriteAsync(cn.Tx, 0, cn.Tx.Length);
                        //這裡可以做寫完後的事
                    }
                }
            }
            catch (Exception exc)
            {
                application.PrintLine(exc.Message);
                //MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        

        private byte[] Serializate(IPacket packet)
        {
            return TCPServer.Math.Serializate.ToByteArray(packet);
        }
        private IPacket DisSerializate(byte[] source)
        {
            return (IPacket)TCPServer.Math.Serializate.ToObject(source);
        }        

        public virtual void OnOperationRequest(OperationRequest operationRequest)
        {
            application.PrintLine(DateTime.Now + " - " + this.ToString() + ": " + operationRequest.ForTest);
        }

        public virtual void OnDisconnect()
        {

        }

    }
}
