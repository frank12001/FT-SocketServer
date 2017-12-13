using System;
using System.Collections.Generic;
using TCPServer.ClientInstance;
using TCPServer;
using System.Net.Sockets;
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
            if (this.room != null)
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
        public void SendEvent(byte eventCode, Dictionary<byte, object> packet)
        {
            EventData eventData = new EventData((byte)eventCode, packet);
            base.SendEvent(eventData);
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
            SendEvent(eventData);
        }
        #endregion
    }
}

