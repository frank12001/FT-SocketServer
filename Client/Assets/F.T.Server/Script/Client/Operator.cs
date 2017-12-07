using UnityEngine;
using System;
using System.Timers;
using System.Collections.Generic;

namespace FTServer.Operator
{
    /// <summary>
    /// 發送時的 Operation Code
    /// </summary>
    public enum OperationCode : byte
    {
        Member = 0,
        Room = 1,
        Gaming = 2,
        System = 3,
        Queue = 6,
    }
    //基底 class
    public abstract class NetWorkBase : ISendMessage, IServerCallBack
    {
        protected GameNetWorkService gameService;
        protected OperationCode operationCode;
        public NetWorkBase(GameNetWorkService gameService)
        {
            this.gameService = gameService;
        }

        public void SendMessageToServer(Dictionary<byte, object> packet)
        {
            gameService.Deliver((byte)this.operationCode, packet);
        }
        public abstract void ServerCallBack(Dictionary<byte, object> server_packet);
    }
    //會員操作
    public class _Member : NetWorkBase
    {
        #region 接收事件
        public event ReceiveBoolbyteStringHandler ReceiveRegister;
        public event ReceiveObjectArrayHandler ReceiveQueryByCompanyName;
        public event ReceiveObjectArrayHandler ReceiveQueryByCompanyQRCode;
        #endregion
        #region 建構子
        public _Member(GameNetWorkService gameService) : base(gameService)
        {
            this.operationCode = OperationCode.Member;
            this.gameService.MemberEvent += this.ServerCallBack; // Server 回乎掛勾
        }
        #endregion
        #region 主動呼叫
        /// <summary>
        /// 註冊公司/場品 (當註冊的公司名為中文時會有問題)
        /// </summary>
        /// <param name="serialId">android 裝置上的 serialId 識別碼</param>
        /// <param name="roomType">房間種類</param>
        public void Register(string companyName,string phone, string address, bool product_tv,string serialId)
        {
            /* packet
             * 0 = switch code , create (0) or add (1)
             * 1 = if(switchcode == 0) customName , if(switch == 1) TargetRoom.guid
            */
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                    {(byte)0,(byte)1}, //switch code
                    {(byte)1,companyName},
                    {(byte)2,phone},
                    {(byte)3,address},
                    {(byte)4,product_tv},
                    {(byte)5,serialId},
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        /// <summary>
        /// 用公司名，搜尋公司資料
        /// </summary>
        /// <param name="companyName"></param>
        public void QueryByCompanyName(string companyName)
        {
            /* packet
             * 0 = switch code , create (0) or add (1)
             * 1 = if(switchcode == 0) customName , if(switch == 1) TargetRoom.guid
             */
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                    {(byte)0,(byte)2}, //switch code
                    {(byte)1,companyName},
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }

        public void QueryByCompanyQRCode(string qrcode)
        {
            /* packet
                * 0 = switch code , create (0) or add (1)
                * 1 = if(switchcode == 0) customName , if(switch == 1) TargetRoom.guid
            */
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                    {(byte)0,(byte)3}, //switch code
                    {(byte)1,qrcode},
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        #endregion 
        #region 介面實作
        public override void ServerCallBack(Dictionary<byte, object> server_packet)
        {
            byte switchCode = byte.Parse(server_packet[0].ToString());
            switch (switchCode)
            {
                case 0:
                    //testX(server_packet);
                    break;
                case 1:
                    receive_Register(server_packet);
                    break;
                case 2:
                    receive_QueryByCompanyName(server_packet);
                    break;
                case 3:
                    receive_QueryByCompanyQRCode(server_packet);
                    break;
            }
        }

        private void receive_Register(Dictionary<byte, object> server_packet)
        {
            bool sucess = bool.Parse(server_packet[1].ToString());
            string qrcode = server_packet[2].ToString();
            byte errorCode = byte.Parse(server_packet[3].ToString());
            if (ReceiveRegister != null)
                ReceiveRegister(sucess, errorCode,qrcode);
            Debug.Log("sucess = " + sucess);
            Debug.Log("errorCode = " + errorCode);
        }
        private void receive_QueryByCompanyName(Dictionary<byte, object> server_packet)
        {
            if (server_packet.Count <= 1)
            {
                if (ReceiveQueryByCompanyName != null)
                    ReceiveQueryByCompanyName(null);
                return;
            }
            object[] result = new object[server_packet.Count-1];
            for (byte i = 1; i <= result.Length; i++)
            {
                //server_packet[0] 是 switch Code
                result[i-1] = server_packet[i];
            }
            if (ReceiveQueryByCompanyName != null)
                ReceiveQueryByCompanyName(result);
        }
        private void receive_QueryByCompanyQRCode(Dictionary<byte, object> server_packet)
        {
            if (server_packet.Count <= 1)
            {
                if (ReceiveQueryByCompanyQRCode != null)
                    ReceiveQueryByCompanyQRCode(null);
                return;
            }
            object[] result = new object[server_packet.Count - 1];
            for (byte i = 1; i <= result.Length; i++)
            {
                //server_packet[0] 是 switch Code
                result[i - 1] = server_packet[i];
            }
            if (ReceiveQueryByCompanyQRCode != null)
                ReceiveQueryByCompanyQRCode(result);
        }
        #endregion
    }
    //房間操作
    [System.Serializable]
    public class _Room : NetWorkBase
    {
        #region Receive Server CallBack Event
        //創房
        public event ReceiveStringHandler ReceiveCreateRoom;
        //加房
        public event ReceiveBoolbyteHandler ReceiveJoinRoom;
        //離房
        public event ReceiveHanlder ReceiceLeaveRoom;
        //被踢
        public event ReceiveHanlder ReceiveDisbandRoom;
        //取得玩家數量
        public event ReceiveByteHandler ReceivePlayersCount;
        #endregion
        #region 建構子
        public _Room(GameNetWorkService gameService) : base(gameService)
        {
            operationCode = OperationCode.Room;
            this.gameService.RoomEvent += this.ServerCallBack; // Server 回乎掛勾
        }
        #endregion
        #region Room Operation (主動呼叫)
        /// <summary>
        /// 創建房間
        /// </summary>
        /// <param name="serialId">android 裝置上的 serialId 識別碼</param>
        /// <param name="roomType">房間種類</param>
        public void CreateRoom(string serialId, RoomTypes roomType)
        {
            /* packet
             * 0 = switch code , create (0) or add (1)
             * 1 = if(switchcode == 0) customName , if(switch == 1) TargetRoom.guid
            */
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                    {(byte)0,(byte)0},
                    {(byte)1,serialId},
                    {(byte)2,(byte)roomType },
            };
            gameService.Deliver((byte)OperationCode.Room, packet);
        }
        public void JoinRoom(string roomUid)
        {
            /* packet
             * 0 = switch code , create (0) or add (1)
             * 1 = if(switchcode == 0) customName , if(switch == 1) TargetRoom.guid
             */
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                    {(byte)0,(byte)1},
                    {(byte)1,Serializate.ToByteArray(roomUid)},
            };
            gameService.Deliver((byte)OperationCode.Room, packet);
        }
        public void ExitRoom()
        {

            /* packet
            * 0 = switch code , create (0) or add (1)
            * 1 = if(switchcode == 0) customName , if(switch == 1) TargetRoom.guid
            */
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                    {(byte)0,(byte)2},
            };
            gameService.Deliver((byte)OperationCode.Room, packet);
        }
        /// <summary>
        /// 從 Server 取得，現在房間中有多少人 (包含房主 TV )
        /// </summary>
        public void GetPlayersCount()
        {
            /* packet
             * 0 = switch code , create (0) or add (1)
             */
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                    {(byte)0,(byte)4},
            };
            gameService.Deliver((byte)OperationCode.Room, packet);
        }

        #endregion
        #region Receive Server CallBack (被動呼叫)
        /// <summary>
        /// 主要 CallBack 流程
        /// </summary>
        /// <param name="server_packet"></param>
        public override void ServerCallBack(Dictionary<byte, object> server_packet)
        {
            byte switchCode = byte.Parse(server_packet[0].ToString());
            switch (switchCode)
            {
                case 0: //create room
                    receive_CreateRoom(server_packet);
                    break;
                case 1: //join room 
                    receive_JoinRoom(server_packet);
                    break;
                case 2: //leave room
                    receive_LeaveRoom(server_packet);
                    break;
                case 3: //Disband room
                    receive_DisbandRoom(server_packet);
                    break;
                case 4: //Get Players Count
                    receive_PlayersCount(server_packet);
                    break; 
            }
        }
        #region Server CallBack 各項處理縮排
        private void receive_CreateRoom(Dictionary<byte, object> packet)
        {
            bool success = bool.Parse(packet[1].ToString());
            if (success)
            {
                string roomGUid = packet[2].ToString();
                if (ReceiveCreateRoom != null)
                    ReceiveCreateRoom(roomGUid);
            }
            else
            {
                if (ReceiveCreateRoom != null)
                    ReceiveCreateRoom("");
            }
        }
        private void receive_JoinRoom(Dictionary<byte, object> packet)
        {
            bool sucess = bool.Parse(packet[1].ToString());
            byte? playerId = null;
            bool isMyId = false;
            if (sucess)
            {
                playerId = byte.Parse(packet[2].ToString());
                isMyId = bool.Parse(packet[3].ToString());
            }
            if (ReceiveJoinRoom != null)
                ReceiveJoinRoom(isMyId, playerId);            
        }
        private void receive_LeaveRoom(Dictionary<byte, object> packet)
        {
            if (ReceiceLeaveRoom != null)
                ReceiceLeaveRoom();
        }
        private void receive_DisbandRoom(Dictionary<byte, object> packet)
        {
            if (ReceiveDisbandRoom != null)
                ReceiveDisbandRoom();
        }
        private void receive_PlayersCount(Dictionary<byte, object> packet)
        {
            byte playersCount = byte.Parse(packet[2].ToString());
            if (ReceivePlayersCount != null)
                ReceivePlayersCount(playersCount);
        }
        #endregion 
        #endregion
    }
    //遊戲中
    [System.Serializable]
    public class _Gaming : NetWorkBase
    {
        #region Receive Server CallBack Event
        //客製化封包
        public event ReceiveObjectHandler ReceiveCustomPacket;
        //如果房間種類有變更的話，接收新的房間種類
        public event ReceiveRoomTypeHanlder ReceiveRoomType;
        /// <summary>
        /// 斷線後觸發
        /// </summary>
        public event ReceiveByteHandler ReceiveDisConnecter;
        /// <summary>
        /// 傳送伺服器封包
        /// </summary>
        public event ReceiveObjectHandler ReceiveServerPacket;
        #endregion
        #region 建構子
        public _Gaming(GameNetWorkService gameService) : base(gameService)
        {
            operationCode = OperationCode.Gaming;
            this.gameService.GamingEvent += this.ServerCallBack; // Server 回乎掛勾
        }
        #endregion
        #region 傳送遊戲封包 (主動)
        #region 客製化封包 (轉傳對象不同)
        /// <summary>
        /// 發送客製化封包給，在遊戲房中的所有人。除了自己
        /// </summary>
        /// <param name="custom_packet"></param>
        public void BroadcastCustomPacket_EveryOne(object custom_packet)
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                { (byte)0,0 }, //switch code 客製化封包
                { (byte)1,0 }, //deliver code 傳送給誰 0 = 所有人 、 1 = 除了自己之外的所有人、2 = RoomOwner
                { (byte)2,Serializate.ToByteArray(custom_packet) }
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        /// <summary>
        /// 發送客製化封包給，在遊戲房的主人(TV端)。
        /// </summary>
        /// <param name="custom_packet"></param>
        public void BroadcastCustomPacket_RoomOwner(object custom_packet)
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                { (byte)0,0 }, //switch code 客製化封包
                { (byte)1,2 }, //deliver code 傳送給誰 0 = 所有人 、 1 = 除了自己之外的所有人、2 = RoomOwner
                { (byte)2,Serializate.ToByteArray(custom_packet) }
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        public void SendToServer(object custom_packet)
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                { (byte)0,0 }, //switch code 客製化封包
                { (byte)1,2 }, //deliver code 傳送給誰 0 = 所有人 、 1 = 除了自己之外的所有人 、2 = 單傳給 Server
                { (byte)2,Serializate.ToByteArray(custom_packet) }
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        /// <summary>
        /// 發送客製化封包給，在遊戲房中的所有人。除了自己
        /// </summary>
        /// <param name="custom_packet"></param>
        public void BroadcastPacket_Server(Dictionary<byte,object> custom_packet)
        {
            gameService.Deliver((byte)this.operationCode, custom_packet);
        }
        /// <summary>
        /// 測試用
        /// </summary>
        /// <param name="custom_packet"></param>
        public void TestPacket(Dictionary<byte,object> custom_packet)
        {
            gameService.Deliver((byte)this.operationCode, custom_packet);
        }
        #endregion 
        public void ChangeType(RoomTypes type)
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                { (byte)0,1 }, //switch code 客製化封包
                { (byte)1,(byte)type }, //房間種類
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        #endregion
        #region Receive Server CallBack (被動呼叫)
        public override void ServerCallBack(Dictionary<byte, object> server_packet)
        {
            byte switchCode = byte.Parse(server_packet[0].ToString());
            switch (switchCode)
            {
                case 0: //客製化封包
                    Receive_CustomPacket(server_packet);
                    break;
                case 1: //改變成甚麼房間種類
                    Receive_RoomType(server_packet);
                    break;
                case 2: //有人斷線 
                    //回傳斷線那個人的 id
                    Receive_DisconnecterId(server_packet);
                    break;
                case 3: //回傳個房間不同的包裹
                    Receive_ServerPacket(server_packet);
                    break;
            }
        }

        #region Server CallBack 各項處理縮排
        private void Receive_CustomPacket(Dictionary<byte, object> packet)
        {
            object custom_class = Serializate.ToObject((byte[])packet[2]);
            if (ReceiveCustomPacket != null)
                ReceiveCustomPacket(custom_class);
        }
        private void Receive_RoomType(Dictionary<byte, object> packet)
        {
            //Debug.Log("新房間 = " + packet[1].ToString());
            RoomTypes type = (RoomTypes)byte.Parse(packet[1].ToString());
            if (ReceiveRoomType != null)
                ReceiveRoomType(type);
        }
        private void Receive_DisconnecterId(Dictionary<byte, object> packet)
        {
            byte disconnecterId = byte.Parse(packet[1].ToString());
            if (ReceiveDisConnecter != null)
                ReceiveDisConnecter(disconnecterId);
        }
        private void Receive_ServerPacket(Dictionary<byte, object> packet)
        {
            object o = Serializate.ToObject((byte[])packet[1]);
            if (ReceiveServerPacket != null)
                ReceiveServerPacket(o);
        }
        #endregion
        #endregion
    }
    //系統
    public class _System : NetWorkBase
    {
        private float ping = 0.0f;
        public float Ping { get { return this.ping; } }
        //計算 Ping 用的 Timer
        private Timer timer = new Timer();
        private float time = 0.0f;

        private bool isConnect = false;
        public bool IsConnect { get { return this.isConnect; } }
        //連線時觸發
        public event ReceiveHanlder Connect;
        //斷線時觸發
        public event ReceiveHanlder Disconnect;
        //取得房間總數
        public event ReceiveByteHandler ReceiveRoomsCount;

        public event ReceiveObjectArrayHandler ReceiveCompanyInfo;
        /// <summary>
        /// 單純只接收 Server 傳來的訊息 (沒有呼叫，訊息都由Server 發)
        /// </summary>
        public event ReceiveStringHandler ReceiveServerLog;

        public _System(GameNetWorkService gameService) : base(gameService)
        {
            this.operationCode = OperationCode.System;
            this.gameService.SystemEvent += this.ServerCallBack; // Server 回乎掛勾
            //多掛勾個 連線/斷線 觸發
            this.gameService.ConnectFromServer += this.connect;
            this.gameService.DisconnectFromServer += this.disConnect;

            setTimer(ref this.timer, 15);
        }
        #region 傳送遊戲封包 (主動)
        /// <summary>
        /// 連線到伺服器
        /// </summary>
        public void ConnectToServer()
        {
            gameService.ConnectToServer();
        }
        /// <summary>
        /// 開始計算新的 Ping
        /// </summary>
        /// <param name="assign_packet">平時用的封包大小。有正確的平寬需求，才有更準的Ping</param>
        public void ComputePing(object assign_packet)
        {
            if (this.timer.Enabled)
                return; //如果正在計時，則跳出
            this.timer.Start();
            //開始計算
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                {(byte)0,(byte)0 },
                {(byte)1,Serializate.ToByteArray(assign_packet) },
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        /// <summary>
        /// 取得房間總數
        /// </summary>
        public void GetRoomsCount()
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                {(byte)0,(byte)2 },
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        /// <summary>
        /// 用 qrcode 取得公司資訊
        /// </summary>
        /// <param name="qrcode"></param>
        public void GetCompanyByQrcode(string qrcode)
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                {(byte)0,(byte)3 },
                { (byte)1,qrcode},
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        #endregion                
        #region Receive Server CallBack (被動呼叫)
        public override void ServerCallBack(Dictionary<byte, object> server_packet)
        {
            byte switchCode = byte.Parse(server_packet[0].ToString());
            switch (switchCode)
            {
                case 0: //回傳計算 Ping 用封包
                    Receive_ComputePing(server_packet);
                    break;
                case 2: //回傳有多少間房
                    Receive_RoomsCount(server_packet);
                    break;
                case 3: //回傳公司資訊
                    Receive_CompanyInfo(server_packet);
                    break;
                case 4: //被動接收 ServerLog
                    Receive_ServerLog(server_packet);
                    break;
            }
        }
        /// <summary>
        /// 當剛連線時呼叫
        /// </summary>
        private void connect()
        {
            isConnect = true;
            if (Connect != null)
                Connect();
        }
        /// <summary>
        /// 掛勾給 gameService 當斷線時呼叫
        /// </summary>
        private void disConnect()
        {
            isConnect = false;
            if (Disconnect!=null)
                 Disconnect();
        }
        #region Server CallBack 各項處理縮排
        private void Receive_ComputePing(Dictionary<byte, object> server_packet)
        {
            this.ping = time;
            this.timer.Stop();
            time = 0.0f;
        }
        private void Receive_RoomsCount(Dictionary<byte, object> server_packet)
        {
            if(ReceiveRoomsCount!=null)
                ReceiveRoomsCount(byte.Parse(server_packet[1].ToString()));
        }
        private void Receive_CompanyInfo(Dictionary<byte, object> server_packet)
        {            
            if (ReceiveCompanyInfo != null)
            {
                object[] o = (object[])Serializate.ToObject((byte[])server_packet[1]);
                ReceiveCompanyInfo(o);
            }
        }
        private void Receive_ServerLog(Dictionary<byte, object> server_packet)
        {
            string serverMessage = server_packet[1].ToString();
            //Debug.Log(serverMessage);
            if (ReceiveServerLog != null)
            {                
                ReceiveServerLog(serverMessage);
            }
        }
        #endregion 
        #endregion
        #region 私有方法 縮排用
        /// <summary>
        /// 初始化計時器
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="interval"></param>
        private void setTimer(ref Timer timer, double interval)
        {
            // Create a timer with a two second interval.
            timer = new System.Timers.Timer(interval);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimer;
            timer.AutoReset = true;
        }
        /// <summary>
        /// 用來計算時間的功能
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            this.time += (float)timer.Interval;
        }
        #endregion 
    }

    //系統
    public class _Queue : NetWorkBase
    {
        public event ReceiveBoolHandler ReceiveJoinQueue;
        public _Queue(GameNetWorkService gameService) : base(gameService)
        {
            this.operationCode = OperationCode.Queue;
            this.gameService.QueueEvent += ServerCallBack;
        }
        #region 傳送遊戲封包 (主動)
        /// <summary>
        /// 開始計算新的 Ping
        /// </summary>
        /// <param name="assign_packet">平時用的封包大小。有正確的平寬需求，才有更準的Ping</param>
        public void JoinQueue()
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                {(byte)0,(byte)1 },
            };
            gameService.Deliver((byte)this.operationCode, packet);
        }
        #endregion                
        #region Receive Server CallBack (被動呼叫)
        public override void ServerCallBack(Dictionary<byte, object> server_packet)
        {
            byte switchCode = byte.Parse(server_packet[0].ToString());
            switch (switchCode)
            {
                case 1: 
                    Receive_JoinQueue(server_packet);
                    break;
            }
        }
        #region Server CallBack 各項處理縮排
        private void Receive_JoinQueue(Dictionary<byte, object> server_packet)
        {            
            if (ReceiveJoinQueue != null)
            {
                ReceiveJoinQueue(bool.Parse(server_packet[1].ToString()));
            }
        }
        #endregion 
        #endregion
        
    }

    public enum RoomTypes : byte
    {
        Base = 0,
        /// <summary>
        /// 展場用
        /// </summary>
        Exhibition,
    }

    interface ISendMessage
    {
        void SendMessageToServer(Dictionary<byte, object> packet);
    }
    interface IServerCallBack
    {
        void ServerCallBack(Dictionary<byte, object> server_packet);
    }

    public delegate void ReceiveHanlder();
    public delegate void ReceiveObjectHandler(object o);
    public delegate void ReceiveStringHandler(string s);
    public delegate void ReceiveBoolHandler(bool b);
    public delegate void ReceiveByteHandler(byte? b);
    public delegate void ReceiveObjectArrayHandler(object[] o);
    public delegate void ReceiveBoolbyteHandler(bool b, byte? bb);
    public delegate void ReceiveBoolbyteStringHandler(bool b, byte? bb,string s);
    public delegate void ReceiveRoomTypeHanlder(RoomTypes type);
}

    //[System.Serializable]
    //public class PassBoolAndString : UnityEvent<bool, string> { }
    //[System.Serializable]
    //public class PassByteAndString : UnityEvent<byte, string> { }
    //[System.Serializable]
    //public class PassTexture2D : UnityEvent<Texture2D> { }
    //[System.Serializable]
    //public class PassObject : UnityEvent<object> { }
    //[System.Serializable]
    //public class PassUid : UnityEvent<int> { }
    //[System.Serializable]
    //public class PassStringTexture2D : UnityEvent<string, Texture2D> { }
    //[System.Serializable]
    //public class ReceiveByteStringImage : UnityEvent<byte, string, Texture2D> { }
