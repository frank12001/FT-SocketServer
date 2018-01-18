using System;
using System.Collections.Generic;
using TCPServer.ClientInstance;
using TCPServer;
using System.Net.Sockets;
using System.Threading.Tasks;
using TCPServer.ClientInstance.Packet;
using TCPServer.Math;

namespace startOnline
{
    /// <summary>
    /// 房間基本功能
    /// </summary>
    public class PeerBase : ClientNode
    {
        protected Form1 _server;

        //這裡需重構，將 Room 資訊/存取方法整理
        /// <summary>
        /// in which room , now  . if null = not in room
        /// </summary>
        public Room room;
        public byte playeridInRoom; //現在在room中的 id

        /// <summary>
        /// 封包暫存的格式
        /// </summary>
        private struct PacketTemp
        {
            public byte eventCode;
            public Dictionary<byte, object> packet;
        }
        /// <summary>
        /// 暫存封包空間
        /// </summary>
        private List<PacketTemp> PacketTempList = new List<PacketTemp>();
        /// <summary>
        /// 是否有包在傳送中
        /// </summary>
        private bool IsSending = false;

        public PeerBase(Form1 app, TcpClient _tclient, byte[] _tx, byte[] _rx, string _str, IApplication applicationInterface) : base(_tclient, _tx, _rx, _str, applicationInterface)
        {
            this._server = app;
        }

        public override void OnDisconnect()
        {
            //DisplayMessageBox(" peer is disconnect ");
            if (this.room != null) //if in room when disconnect
            {
                //DisplayMessageBox(" call room Exit ");
                //先呼叫自己斷線了
                this.room.DisConnect(this.playeridInRoom);
                //在呼叫離開此房間
                this.room.Room_Exit(this.playeridInRoom);
                //將此房間設為 null
                this.room = null;
            }
        }

        #region Room event 
        /// <summary>
        /// called when kicked by room
        /// </summary>
        public void RoomWasKicked()
        {
            this.room = null;
            this.playeridInRoom = 0;
            //Send Message
            //要發新的包
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                {0,3},
                {1,true},
            };
            SendEvent(1, packet);
        }
        #endregion 
        #region myFunction
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="packet"></param>
        public void SendEvent(byte eventCode, Dictionary<byte, object> packet)
        {
            EventData eventData = new EventData((byte)eventCode, packet);
            base.Write(eventData);
        }


        public async Task SendEventAsync(byte eventCode, Dictionary<byte, object> packet)
        {
            EventData eventData = new EventData((byte)eventCode, packet);
            await WriteAsync(eventData);
        }

        /// <summary>
        /// 使用排程傳送
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="packet"></param>
        public void SendEventList(byte eventCode, Dictionary<byte, object> packet)
        {
            PacketTempList.Add(new PacketTemp(){ eventCode = eventCode, packet = packet });
            Task.Run(() => SendTask(eventCode, packet));
        }
        private async Task SendTask(byte eventCode, Dictionary<byte, object> packet)
        {
            if (IsSending)
                return;
            while (PacketTempList.Count > 0)
            {
                IsSending = true;
                EventData eventData = new EventData(PacketTempList[0].eventCode, PacketTempList[0].packet);
                await WriteAsync(eventData);
                PacketTempList.RemoveAt(0);
            }
            IsSending = false;
        }
        /// <summary>
        /// 傳送伺服器訊息給此 Client。 Client 用 Connect.System.ReveiveServerLog 接
        /// </summary>
        public void SendServerLog(string message)
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {(byte)0,4}, //swicth Code (固定)
                {(byte)1,message }, //Message
            };
            EventData eventData = new EventData((byte)3, packet); //event Code = 3,System
            WriteAsync(eventData);
        }
        #endregion
    }
}

