using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using PalaceWar;
using startOnline;
using TCPServer.Projects.Stellar;
using LoadingNextScene = TCPServer.Projects.Palace.Packet.LoadingNextScene;

namespace TCPServer.Projects.Palace
{
    public class PalaceGamingRoom : Room
    {
        #region 參數-遊戲中
        /// <summary>
        /// 已經 Loading Scene 好的人。用在剛開始，所有人都 Loading 完後才發送 GameStart
        /// </summary>
        private List<byte> loadingSceneReady = new List<byte>();
        /// <summary>
        /// 傳送過開始遊戲了嗎
        /// </summary>
        private bool SendGameStart = false;
        /// <summary>
        /// 從此房間被創建時，持續累加
        /// </summary>
        private float GamingTime = 0;


        #endregion 
        public PalaceGamingRoom(string customName, PalacePeer[] joinPlayers, string roomIndexInApplication, Form1 applicationPointer) : base(customName,joinPlayers,roomIndexInApplication,applicationPointer)
        {
            _server.printLine("In Palace Gaming Room");
        }

        ~PalaceGamingRoom()
        {
            _server.printLine("Release Palace Gaming Room");
        }

        public override void mainThread(object sender, ElapsedEventArgs e)
        {
            if (!SendGameStart)
            {
                Dictionary<byte, object> packet = new Dictionary<byte, object>()
                {
                    {(byte)0,3 },
                    {(byte)1,Math.Serializate.ToByteArray(new LoadingNextScene()) },
                };
                BroadcastPacket(packet);
                SendGameStart = true;
            }


            GamingTime += timer_interal;
        }



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
                        case 0:    //傳給所有人
                            BroadcastPacket(packet);
                            break;
                        case 1:    //傳給除了自己之外的人
                            BroadcastPacket(packet, playerId);
                            break;
                        case 2:    //轉傳給 RoomOwner
                            SendToAssignPlayer(packet, Ownerid);
                            break;
                        case 3: //Client Send To Server
                            //如果接到 Monster 包，將他解包加入 GamingTime 再傳給所有人
                            object o = Math.Serializate.ToObject((byte[])packet[2]);
                            if (o is Monster)
                            {
                                Monster monster = (Monster) o;
                                monster.GamingTime = this.GamingTime;
                                packet = new Dictionary<byte, object>()
                                {
                                    { (byte)0,3 }, //switch code 客製化封包                                   
                                    { (byte)1,Math.Serializate.ToByteArray(monster) }
                                };
                                BroadcastPacket(OperationCode.Gaming, packet);
                            }
                            break;
                            
                    }
                    break;
                #endregion

                case 1: //Loading Scene Ready
                    loadingSceneReady.Add(playerId);
                    if (loadingSceneReady.Count.Equals(players.Count))
                        SendInitPacket();
                    break;
                case 2: //同步怪物位置專用接口
                    packet[0] = 3;
                    //object oo = Math.Serializate.ToObject((byte[])packet[1]);
                    _server.printLine("playerid = " + playerId);
                    BroadcastPacket(packet, playerId);
                    break;
                
            }
        }

        /// <summary>
        /// 取得每個人的玩家資料並傳送
        /// </summary>
        private void SendInitPacket()
        {
            PalaceWar.GamingStart start = new PalaceWar.GamingStart()
            {
                CardsFight = new[] { "FS_A_1", "FS_A_1", "FS_A_1", "FS_A_1", "FS_A_1", "FS_A_1" },
                CardsCommander = new[] { "FC_1", "FC_1", "FC_1" }

            };

            foreach (KeyValuePair<byte, PeerBase> player in this.players)
            {
                start._Team = (PalaceWar.Team)(player.Key % 2);

                Dictionary<byte, object> packet = new Dictionary<byte, object>()
                {
                    {(byte) 0, 3},
                    {(byte) 1, Math.Serializate.ToByteArray(start)},
                };
                player.Value.SendEvent((byte)OperationCode.Gaming, packet);
            }
        }
    }
}
