using System.Timers;
using FTServer.ClientInstance.Packet;

namespace FTServer.ClientInstance.Peer
{
    /// <summary>
    /// 客戶端封包接收器
    /// </summary>
    public class ClientNodeListener
    {
        /// <summary>
        /// 多久從序列中寫出或讀入一包
        /// </summary>
        private const byte Tick_Read = 5, Tick_MainConnecting = 100;

        /// <summary>
        /// 接收封包之時間間隔
        /// </summary>
        private ushort Timer_ReadPacket = 0;

        /// <summary>
        /// 斷線之time out時間長度
        /// </summary>
        private ushort TimeLimit_Disconnect = 5000;//1000;
        /// <summary>
        /// 客戶端節點
        /// </summary>
        private ClientNode clientNode;

        /// <summary>
        /// 接收封包及維持連線之Timer
        /// </summary>
        private Timer receiver,maintainConnecting;

        /// <summary>
        /// 客戶端節點封包接收器
        /// </summary>
        /// <param name="clientNode"></param>
        public ClientNodeListener(ClientNode clientNode,ushort timeout)
        {
            this.clientNode = clientNode;
            this.TimeLimit_Disconnect = timeout;

            BeginReadAsync();                 // 開始接收封包
            //BeginMaintainConnectingAsync();   // 開始進行維持連線之封包發送
        }

        /// <summary>
        /// 開始等待封包傳入，每隔一段時間處理封包
        /// </summary>
        private void BeginReadAsync()
        {
            receiver = new Timer(Tick_Read);
            receiver.Elapsed += Handler_Read;
            receiver.Start();
        }

        /// <summary>
        /// 每隔一段時間定期進行連絡以確認維持連線
        /// </summary>
        private void BeginMaintainConnectingAsync()
        {
            /*
            * 可考慮在接收到資料封包時reset timer，藉此減少封包傳遞
            */
            maintainConnecting = new Timer(Tick_MainConnecting);
            maintainConnecting.Elapsed += Handler_MaintainConnecting;
            maintainConnecting.Start();
        }

        /// <summary>
        /// 進行封包處理
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void Handler_Read(object o, ElapsedEventArgs e)
        {
            //clientNode.Rx.Clear();
            if (!clientNode.Rx.Count.Equals(0))
            {
                // 收到任何一個封包都將維持連線訊號的timer重置
                Timer_ReadPacket = 0;
                byte[] buff = clientNode.Rx.Dequeue();
                if (buff != null)
                {
                    // 如果是維持連線的訊號封包，則不予處理
                    if (!buff.Length.Equals(1))
                    {
                        buff = Math.Serializate.Decompress(buff);
                        IPacket packet = (IPacket)Math.Serializate.ToObject(buff);
                        clientNode.OnOperationRequest(packet); // 客戶端節點執行接收事件
                    }
                }
            }
            Timer_ReadPacket += Tick_Read;
        }


        private void Handler_MaintainConnecting(object o, ElapsedEventArgs e)
        {
            // 當維持連線之訊號中斷直到timeout，作斷線處理
            if (Timer_ReadPacket >= TimeLimit_Disconnect)
            {
                Disconnect();
                //Printer.WriteLine($"Client timed out {TimeLimit_Disconnect} (ms), diconnected.");
            }
            // 如果長時間未收到維持訊號
            if (Timer_ReadPacket >= 2000)
            {
                // 對客戶端發送維持連線之訊號
                byte[] buff = new byte[] { 0 };
                clientNode._Sender.SendAsync(buff, clientNode.iPEndPoint);
            }           
        }

        /// <summary>
        /// 主動斷線
        /// </summary>
        public void Disconnect()
        {
            //停止所有的 Timer 運作
            receiver.Stop();
            receiver.Close();
            maintainConnecting.Stop();
            maintainConnecting.Close();

            clientNode.OnDisconnect();
            clientNode.socketServer.CloseClient(clientNode.iPEndPoint);
        }
    }
}
