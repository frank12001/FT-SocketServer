using startOnline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using startOnline.playar.Rooms;
using Stellar.Poker;
using TCPServer.playar.Rooms.Operator;
using PlayAR.Common;

namespace TCPServer.playar.Rooms
{
    public class QueueRoom : Room
    {

        private byte waitHowMuchPeople = 0;
        private bool hasChangeRoom = false;

        private int mPlayerInfoCount = 0;
        public List<PlayerInfo> PlayerInfos = null;

        private PlayAR.Common.Timer changeRoom = new PlayAR.Common.Timer(){ startTimer = false, nowTimer = 0.0f, max_Timer = 5.0f };

        public QueueRoom(string customName, PeerBase ownerPeer, string roomIndexInApplication, Form1 applicationPointer,
            byte waitHowMuchPeople) : base(customName, ownerPeer, roomIndexInApplication, applicationPointer)
        {
            this.waitHowMuchPeople = waitHowMuchPeople;
            this.PlayerInfos = new List<PlayerInfo>();
            this.mPlayerInfoCount = PlayerInfos.Count;
            _server.printLine("QueueRoom is create");
        }

        #region 遊戲流程

        public override void mainThread(object sender, ElapsedEventArgs e)
        {
            base.mainThread(sender, e);
            //Log.Log.ToTxt("timer interal = " + (timer_interal * 0.001));
            //seamanController.Update(timer_interal * 0.001f); //以註解

            //如 PlayerInfos 的數量有變動
            if (mPlayerInfoCount != PlayerInfos.Count)
            {
                mPlayerInfoCount = PlayerInfos.Count;
                var array = PlayerInfos.ToArray();
                Dictionary<byte, object> packet = new Dictionary<byte, object>()
                {
                    {0,3},
                    {1,TCPServer.Math.Serializate.ToByteArray(array) },
                };
                BroadcastPacket(packet);
            }
            //
            if (players.Count.Equals(waitHowMuchPeople) && !hasChangeRoom)
            {
                //轉換房間成，遊戲防
                if (_server.RoomOperator is Queue queue)
                {
                    queue.Room_QueueFinish(this, RoomTypes.PokerGamingRoom);
                    hasChangeRoom = true;
                }
            }
        }

        #endregion

        #region Client 封包接收

        /// <summary>
        /// Called when Peer access case 2 (接收回傳)
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="packet"></param>
        public override void GamingProcess(byte playerId, Dictionary<byte, object> packet)
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
                        case 0: //傳給所有人
                            BroadcastPacket(packet);
                            break;
                        case 1: //傳給除了自己之外的人
                            BroadcastPacket(packet, playerId);
                            break;
                        case 2: //轉傳給 RoomOwner
                            SendToAssignPlayer(packet, Ownerid);
                            break;
                    }
                    break;

                #endregion
            }
            //最後再進行 base.GamingProcess
            base.GamingProcess(playerId, packet); //switch code 已使用 0 , 1 , 2
        }

        #endregion

        //#region Room function

        //public override bool Room_Join(PeerBase peer, out byte playId)
        //{          
        //    bool isJoinRoom = base.Room_Join(peer, out playId);
        //    //檢查人數
        //    if (Room_GetPlayersCount().Equals(waitHowMuchPeople) && isJoinRoom)
        //    {
        //        //進入 Poker 遊戲房
        //        if (_server.RoomOperator is Queue queue)
        //        {
        //            queue.Room_QueueFinish(this, RoomTypes.PokerGamingRoom);
        //        }
        //    }
        //    return isJoinRoom;
        //}

        //#endregion
    }
}
