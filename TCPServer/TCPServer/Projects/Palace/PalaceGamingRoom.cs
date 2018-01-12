using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using PalaceWar;
using startOnline;
using TCPServer.Projects.Stellar;
using TCPServer.Projects.Palace.Packet;

namespace TCPServer.Projects.Palace
{
    public class PalaceGamingRoom : Room
    {
        private bool SendGameStart = false;
        private float GamingTime = 0;
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
            #region 跟玩家說排到了，可以進遊戲場景了
            CanNextScene();
            #endregion
            GamingTime += timer_interal;
        }

        private void CanNextScene()
        {
            if (!SendGameStart)
            {
                SendGameStart = true;
                Dictionary<byte, object> packet = new Dictionary<byte, object>()
                {
                    {(byte)0,3 },
                    {(byte)1,Math.Serializate.ToByteArray(new LoadingNextScene(){ PlayersName = GetAllPlayersCustomName()}) },
                };
                BroadcastPacketAsync(packet);
            }
        }

        private List<byte> loadingSceneReady = new List<byte>();

        public override void GamingProcess(byte playerId, Dictionary<byte, object> packet100)
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>(packet100);
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
                            //BroadcastPacketAsync(packet);
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

                #region 1 Loading Scene Ready
                case 1: //Loading Scene Ready
                    loadingSceneReady.Add(playerId);
                    if (loadingSceneReady.Count.Equals(players.Count))
                        SendInitPacket();
                    break;
                #endregion

                #region 2 同步位置資料
                case 2: 
                   
                    Dictionary<byte,object> dic = new Dictionary<byte, object>(packet);
                    dic[0] = 3;
                    //_server.printLine("playerid = " + playerId);
                    //BroadcastPacket(packet, playerId);
                    //BroadcastPacket(dic, playerId);
                    BroadcastPacket(dic);
                    break;
                #endregion

                #region 3 GameOver
                case 3:
                    //GamingOver
                    packet[0] = 3;
                    object oo = Math.Serializate.ToObject((byte[]) packet[1]);
                    if (oo is GamingOver)
                    {
                        _server.printLine(" 遊戲結束 ");
                        BroadcastPacket(packet);
                        // 離開/解散房間
                        Room_Disband();
                    }

                    break;
                #endregion

                case 4: //怪物死亡
                    BroadcastPacket(packet);
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
                CardsCommander = new[] { "FC_1", "FC_1", "FC_1" },
                PlayersName = GetAllPlayersCustomName()
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

        /// <summary>
        /// 取得所有玩家的自定義名子
        /// </summary>
        /// <returns></returns>
        private string[] GetAllPlayersCustomName()
        {
            List<string> playersName = new List<string>();
            foreach (KeyValuePair<byte, PeerBase> @base in players)
            {
                PalacePeer peer = (PalacePeer)@base.Value;
                playersName.Add(peer.CustomName);
            }
            return playersName.ToArray();
        }
    }
}
