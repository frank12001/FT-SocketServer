using System;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using FTServer.ClientInstance.Packet;

namespace FTServer
{
    public abstract class NetworkBase 
    {
        protected enum StatusCode : byte { Connect, Disconnect };
        /// <summary>
        /// 伺服器的 ip : port
        /// </summary>
        public string Address = "104.199.181.106" + ":" + "5055";
        private INetwork Network;
        /// <summary>
        /// 接收封包暫存區
        /// </summary>
        private NetworkTempLine<IPacket> SocketReceiver = new NetworkTempLine<IPacket>();
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

        public NetworkBase(NetworkProtocol protocol)
        {
            // CompositeResolver is singleton helper for use custom resolver.
            // Ofcourse you can also make custom resolver.
            MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(
                // use generated resolver first, and combine many other generated/custom resolvers
                MessagePack.Resolvers.GeneratedResolver.Instance,
                MessagePack.Resolvers.BuiltinResolver.Instance,
                MessagePack.Resolvers.AttributeFormatterResolver.Instance,
                MessagePack.Resolvers.PrimitiveObjectResolver.Instance
            );

            switch (protocol)
            {
                case NetworkProtocol.WebSocket:
                    Network = new WebSocketLis();
                    break;
                case NetworkProtocol.UDP:
                    Network = new Udp();
                    break;
                case NetworkProtocol.TCP:
                    Network = new Tcp();
                    break;
                case NetworkProtocol.RUDP:
                    Network = new RUdp();
                    break;
            }
            
            Network.CompleteConnect += result =>
            {
                Debug.Log("Connection State = "+result);
                if (result)
                    OnStatusChanged(StatusCode.Connect);
            };
            Network.CompleteDisConnect += () =>
            {
                OnStatusChanged(StatusCode.Disconnect);
            };
            Network.CompleteSend += () => 
            {
                CanSend = true;
            };
            Network.CompleteReadFromServerStream += result =>
            {
                byte[] receiveBytes = result;
                if (!receiveBytes.Length.Equals(1))
                {
                    receiveBytes = Math.Serializate.Decompress(receiveBytes);
                    IPacket packet = null;
                    //解包成我定義的封包
                    try
                    {
                        packet = (IPacket)Math.Serializate.ToObject(receiveBytes);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    if (packet != null) //將資料存入全域暫存區
                    {
                        SocketReceiver.AddToCurrectLine(packet);
                    }
                }
                else
                {
                    //Debug.Log("小包到達");
                    //回傳一小包代表該 Client 還活者
                    Network.BeginSend(new byte[] { 0 }, 1);
                }
            };
        }

        /// <summary>
        /// 需要放在 Update 中一直執行的功能
        /// </summary>
        public void Service()
        {
            SocketReceiver.ChangeCurrectLine();
            List<IPacket> packetsList = SocketReceiver.GetUnUseLine();
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

            Network.Service();
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
        private void deliver(byte code, Dictionary<byte, object> dic)
        {    //普通的傳送
            CanSend = false;
            //宣告一個傳送暫存
            byte[] tx;
            //將傳入的 code , dic , 傳換為我定義的封包
            IPacket packet = new IPacket(code, new Dictionary<byte, object>(dic)) {};
            try
            {
                //將此封包，序列化並加入暫存區
                tx = Math.Serializate.ToByteArray(packet);
                tx = Math.Serializate.Compress(tx);
                Network.BeginSend(tx, tx.Length);
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
            try
            {
                if (Network.NetworkProtocol != NetworkProtocol.WebSocket)
                {
                    IPAddress ipa = null;
                    int nPort;
                    string ip = "";
                    string port = "";
                    string[] s = Address.Split(new Char[] { ':' });
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
                    Network.Connect(ipa, nPort);
                }
                else
                {
                    Uri uri = new Uri(Address);
                    Network.Connect(uri);
                }
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
            Network.DisConnect();
        }
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
        protected virtual void OnEvent(IPacket eventData)
        {}
    }
}