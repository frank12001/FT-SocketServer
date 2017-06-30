using System;
using System.Timers;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using startOnline.playar.Rooms;
using TVEducation.ServerClientSwap;
using TVEducation.TVBoxSwap;
using TCPServer;

namespace startOnline
{
    public class Room
    {
        #region 參數
        public readonly RoomTypes Type;
        /// <summary>
        /// owner index in players
        /// </summary>
        public const byte Ownerid = 0;
        /// <summary>
        /// Room's custom name 
        /// </summary>
        public string RoomName;
        /// <summary>
        /// players in this room. Index = 0 => Room owner
        /// </summary>
        public Dictionary<byte, PlayarPeer> players;
        /// <summary>
        /// Room Index In Application
        /// </summary>
        public string RoomIndexInApplication;
        /// <summary>
        /// applicition pointer
        /// </summary>
        protected Form1 _server;
        /// <summary>
        /// 遊戲進程
        /// </summary>
        protected System.Timers.Timer timer;
        /// <summary>
        /// 遊戲進程呼叫間格 (毫秒)
        /// </summary>
        protected const float timer_interal = 30;
        #endregion

        #region 建構子
        public Room(string customName, PlayarPeer ownerPeer, string roomIndexInApplication, Form1 applicationPointer)
        {
            applicationPointer.PrintLine("Create Room");

            this.RoomName = customName;
            players = new Dictionary<byte, PlayarPeer>() { { Ownerid, ownerPeer } };
            this.RoomIndexInApplication = roomIndexInApplication;
            this._server = applicationPointer;
            this.Type = getRoomType();

            #region 開啟遊戲主線程
            // Create a timer with a two second interval.
            timer = new System.Timers.Timer(timer_interal);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += mainThread;
            timer.AutoReset = true;
            timer.Enabled = true;
            #endregion

            #region Console
            try
            {
                //Console
                applicationPointer.PrintLine("BaseRoomPlayer_Total = "+ (uint)players.Count);
                applicationPointer.PrintLine("RoomPlayer_Total = " + (uint)players.Count);
            }
            catch (Exception e)
            {
                applicationPointer.PrintLine(" Room建構子裡的 Console 錯誤  Error = " + e.Message);
            }
            #endregion
        }
        /// <summary>
        /// 透過 room 建構另一個 room
        /// </summary>
        /// <param name="room"></param>
        public Room(Room room, Form1 applicationPointer)
        {
            //DisplayMessageBox("r 1");
            this.RoomName = room.RoomName;
            this.players = new Dictionary<byte, PlayarPeer>();
            foreach (KeyValuePair<byte, PlayarPeer> player in room.players)
            {
                this.players.Add(player.Key, player.Value);
                player.Value.room = this; //重設所有 peer 中的 room                
            }
            //DisplayMessageBox("r 2");
            this.RoomIndexInApplication = room.RoomIndexInApplication; //將原本房間的index給他
            this._server = applicationPointer;
            this.Type = getRoomType();
            #region 傳給所有人新的房間類型
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {(byte)0,1}, //switch code
                {(byte)1,(byte)this.Type},
            };
            BroadcastPacket(packet);
            #endregion
            #region 開啟遊戲主線程
            // Create a timer with a two second interval.
            timer = new System.Timers.Timer(timer_interal);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += mainThread;
            timer.AutoReset = true;
            timer.Enabled = true;
            #endregion
            //DisplayMessageBox("r 4");

            #region Console
            try
            {
                //Console
                applicationPointer.PrintLine("BaseRoomPlayer_Total = " + (uint)players.Count);
                applicationPointer.PrintLine("RoomPlayer_Total = " + (uint)players.Count);
            }
            catch (Exception e)
            {
                applicationPointer.PrintLine(" Room建構子裡的 Console 錯誤  Error = " + e.Message);
            }
            #endregion
        }
        #endregion
        #region 解構子
        ~Room() //解構子
        {
            
        }
        #endregion

        #region 遊戲邏輯處理

        #region 主遊戲流程
        /// <summary>
        /// 主執行續 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void mainThread(object sender, ElapsedEventArgs e)
        {

        }
        #endregion

        #region Client 封包接收
        /// <summary>
        /// Called when Peer access case 2 (接收回傳)
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="packet"></param>
        public virtual void GamingProcess(byte playerId, Dictionary<byte, object> packet)
        {
            byte switchcode_1 = byte.Parse(packet[0].ToString()); //switch code
            switch (switchcode_1)
            {
                #region case 0 客製化封包 switch code 2 = 轉傳對象， 0 = 所有人 、 1 = 自己之外所有人、2 = RoomOwner(房主)
                case 0: //客製化封包
                    /* Receive packet
                     * 0 = switch code , 0 = 客製化封包
                     * 1 = 傳送給誰  0 = transform to every one  , 1 = transform to every . Exception someone
                     * 2 = 遊戲封包
                     */
                    byte switchcode_2 = byte.Parse(packet[1].ToString());
                    switch (switchcode_2)
                    {
                        case 0:    //傳給所有人
                            BroadcastPacket(packet);
                            break;
                        case 1:    //傳給除了自己之外的人
                            BroadcastPacket(packet, playerId);
                            break;
                        case 2:    //轉傳給 RoomOwner
                            SendToAssignPlayer(packet, Ownerid);
                            break;     
                    }
                    break;
                #endregion
                #region case 1 改變房間種類
                case 1: //change room type
                    RoomTypes type = (RoomTypes)byte.Parse(packet[1].ToString());
                    Room_ChangeType(type);
                    //傳給所有人新的房間類型 - 在創建新房時做
                    break;
                #endregion
                case 254: //Test
                    //MessageBox.Show("254 is active");
                    //DataBase.DataBase.Operator.GetCompanyInfo("qrcodeTest1");
                    break;
            }
        }
        #endregion

        #endregion

        #region Gaming Function - Base
        /// <summary>
        /// Broadcast assign Image to everyone. (預設OperatorCode.Gaming)
        /// </summary>
        /// <param name="image"></param>
        public void BroadcastPacket(Dictionary<byte, object> packet)
        {
            foreach (KeyValuePair<byte, PlayarPeer> player in this.players)
            {
                player.Value.SendEvent((byte)OperationCode.Gaming, packet);
            }
        }
        /// <summary>
        /// Broadcast assign Image to everyone. Except id
        /// </summary>
        /// <param name="image"></param>
        /// <param name="id"></param>
        public void BroadcastPacket(Dictionary<byte, object> packet, byte id)
        {
            foreach (KeyValuePair<byte, PlayarPeer> player in this.players)
            {
                if (player.Key != id)
                    player.Value.SendEvent((byte)OperationCode.Gaming, packet);
            }
        }
        /// <summary>
        /// Broadcast assign Image to everyone. Except id
        /// </summary>
        /// <param name="EventCodePlayar"></param>
        /// <param name="image"></param>
        /// <param name="id"></param>
        public void BroadcastPacket(OperationCode eventcode, Dictionary<byte, object> packet, byte id)
        {
            foreach (KeyValuePair<byte, PlayarPeer> player in this.players)
            {
                if (player.Key != id)
                    player.Value.SendEvent((byte)eventcode, packet);
            }
        }
        /// <summary>
        /// Broadcast assign Image to everyone. 
        /// </summary>
        /// <param name="EventCodePlayar"></param>
        /// <param name="image"></param>
        public void BroadcastPacket(OperationCode eventcode, Dictionary<byte, object> packet)
        {
            foreach (KeyValuePair<byte, PlayarPeer> player in this.players)
            {
                player.Value.SendEvent((byte)eventcode, packet);
            }
        }
        /// <summary>
        /// 傳給指定的 id 的玩家
        /// </summary>
        /// <param name="EventCodePlayar"></param>
        /// <param name="image"></param>
        /// <param name="id"></param>
        public void SendToAssignPlayer(Dictionary<byte, object> packet, byte id)
        {
            PlayarPeer targetPlayer;
            if (this.players.TryGetValue(id, out targetPlayer))
                targetPlayer.SendEvent((byte)OperationCode.Gaming, packet);
        }
        /// <summary>
        /// 傳給指定的 id 的玩家
        /// </summary>
        /// <param name="EventCodePlayar"></param>
        /// <param name="image"></param>
        /// <param name="id"></param>
        public void SendToAssignPlayer(OperationCode operatorCode,Dictionary<byte, object> packet, byte id)
        {
            PlayarPeer targetPlayer;
            if (this.players.TryGetValue(id, out targetPlayer))
                targetPlayer.SendEvent((byte)operatorCode, packet);
        }
        #endregion 

        #region Operation Room Function 
        /// <summary>
        /// Exit this room
        /// </summary>
        /// <param name="playarindex"></param>
        public void Room_Exit(byte playarindex)
        {
            if (playarindex == Ownerid) //if the exit person is owner
            {
                Room_Disband(); //先呼叫解散
                this.players.Remove(playarindex); //再將自己移出
                _server.PrintLine("房主關房間");
            }
            else
            {
                this.players.Remove(playarindex);

                #region Console
                try
                {
                    //Console
                    _server.PrintLine("BaseRoomPlayer_Total -= 1");
                    _server.PrintLine("RoomPlayer_Total -= 1");
                }
                catch (Exception e)
                {
                    _server.PrintLine(" Room_Exit裡的 Console 錯誤  Error = " + e.Message);
                }
                #endregion
            }
        }
        /// <summary>
        /// join this room
        /// </summary>
        /// <param name="peer"> me peer </param>
        /// <param name="playId">return my playerid in this room</param>
        /// <returns>sucess or not</returns>
        public bool Room_Join(PlayarPeer peer, out byte playId)
        {
            byte? id = GetAbleId();

            if (id == null)
            {
                playId = 0;
                return false;
            }
            players.Add((byte)id, peer);
            playId = (byte)id;

            //SendMessage To EveryOne In This Room
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
              { (byte)0, 1 },  //switch code 1 = Join Room
              { (byte)1, true },
              { (byte)2, playId},
              { (byte)3,false }, //是不是自己的 playerid
            };

            #region Log To Txt
            try
            {
                _server.PrintLine("playId = " + playId);
            }
            catch (Exception e)
            {
                _server.PrintLine(" log進 playerId 出問題 Error = " + e.Message);
            }
            #endregion

            BroadcastPacket(OperationCode.Room, packet,playId);
            packet[3] = true; //是自己的 playerid
            SendToAssignPlayer(OperationCode.Room, packet, playId);

            #region Console
            try
            {
                //Console
                _server.PrintLine("BaseRoomPlayer_Total += 1");
                _server.PrintLine("RoomPlayer_Total += 1");
            }
            catch (Exception e)
            {
                _server.PrintLine(" Room_Exit裡的 Console 錯誤  Error = " + e.Message);
            }
            #endregion



            return true;
        }
        private byte? GetAbleId()
        {
            for (byte i = 0; i < 255; i++)
            {
                if (!players.ContainsKey(i))
                    return i;
            }
            return null;
        }
        /// <summary>
        /// close this room
        /// </summary>
        public void Room_Disband()
        {
            //結束主線程
            timer.Stop();
            timer.Close();

            #region Console
            try
            {
                //Console
                _server.PrintLine("BaseRoomPlayer_Total -= 1");
                _server.PrintLine("RoomPlayer_Total -= 1");
            }
            catch (Exception e)
            {
                _server.PrintLine(" Room_Exit裡的 Console 錯誤  Error = " + e.Message);
            }
            #endregion

            //跟其他玩家說他被踢了
            foreach (KeyValuePair<byte, PlayarPeer> player in this.players)
            {
                player.Value.RoomWasKicked();
            }

            _server.Room_Remove(this.RoomIndexInApplication);
        }
        /// <summary>
        /// 換遊戲房間
        /// </summary>
        /// <param name="type"></param>
        public void Room_ChangeType(RoomTypes type)
        {
            //DisplayMessageBox(type.ToString());

            #region Console
            try
            {
                //Console
                _server.PrintLine("BaseRoomPlayer_Total -= 1");
                _server.PrintLine("RoomPlayer_Total -= 1");
            }
            catch (Exception e)
            {
                _server.PrintLine(" Room_Exit裡的 Console 錯誤  Error = " + e.Message);
            }
            #endregion

            _server.Room_Create(this, type);
        }
        /// <summary>
        /// 回傳玩家數量
        /// </summary>
        /// <returns></returns>
        public byte Room_GetPlayersCount()
        {
            return (byte)players.Count;
        }
        #endregion

        #region 遊戲功能-Message
        /// <summary>
        /// 傳送訊息給在房間中的所有人
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToAllClient(string message)
        {
            foreach (KeyValuePair<byte, PlayarPeer> peer in players)
            {
                peer.Value.SendServerLog(message);
            }
        }
        #endregion

        #region 斷線觸發
        /// <summary>
        /// 當玩家斷線時，呼叫
        /// </summary>
        /// <param name="playerId"></param>
        public virtual void DisConnect(byte playerId)
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {(byte)0,2 }, //switch code 對應到 Client 的 DisConnecter
                {(byte)1,playerId},
            };
            BroadcastPacket(packet);
        }
        #endregion

        private RoomTypes getRoomType()
        {
            RoomTypes result;
            //取得 class 的名字字串，再用此判斷
            #region Get Type
            Type type = this.GetType();
            string class_Name = type.Name;
            switch (class_Name)
            {
                case "Room":
                    result = RoomTypes.Base;
                    break;
                default:
                    result = RoomTypes.Base;
                    break;
            }

            #endregion
            return result;
        }

    }

    public enum OperationCode : byte
    {
        Member = 0,
        Room = 1,
        Gaming = 2,
        System = 3,
    }
}
