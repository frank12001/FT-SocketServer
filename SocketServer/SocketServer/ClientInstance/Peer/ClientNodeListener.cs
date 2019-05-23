using System;
using System.Timers;
using FTServer.ClientInstance.Packet;

namespace FTServer.ClientInstance.Peer
{
    /// <summary>
    /// 客戶端封包接收器
    /// </summary>
    public class ClientNodeListener : IDisposable
    {
        /// <summary>
        /// 多久從序列中寫出或讀入一包
        /// </summary>
        private const byte TickRead = 5;
        /// <summary>
        /// 客戶端節點
        /// </summary>
        private readonly ClientNode _clientNode;

        /// <summary>
        /// 將封包從暫存區讀出
        /// </summary>
        private Timer _receiver;

        /// <summary>
        /// 客戶端節點封包接收器
        /// </summary> 
        /// <param name="clientNode"></param>
        public ClientNodeListener(ClientNode clientNode)
        {
            _clientNode = clientNode;
            BeginReadAsync();                 // 開始接收封包
        }

        /// <summary>
        /// 開始等待封包傳入，每隔一段時間處理封包
        /// </summary>
        private void BeginReadAsync()
        {
            _receiver = new Timer(TickRead);
            _receiver.Elapsed += Handler_Read;
            _receiver.Start();
        }

        /// <summary>
        /// 進行封包處理
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void Handler_Read(object o, ElapsedEventArgs e)
        {
            lock (_clientNode.Rx)
            {
                if (!_clientNode.Rx.Count.Equals(0))
                {
                    byte[] buff = _clientNode.Rx.Dequeue();
                    if (buff != null)
                    {
                        // 如果是維持連線的訊號封包，則不予處理
                        if (!buff.Length.Equals(1))
                        {
                            buff = Math.Serialize.Decompress(buff);
                            IPacket packet = (IPacket) Math.Serialize.ToObject(buff);

                            _clientNode.OnOperationRequest(packet); // 客戶端節點執行接收事件
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            //停止所有的 Timer 運作
            _receiver.Stop();
            _receiver.Close();
        }
    }
}
