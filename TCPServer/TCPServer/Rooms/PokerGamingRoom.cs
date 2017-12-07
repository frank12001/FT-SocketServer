using startOnline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Stellar.Poker;

namespace TCPServer.playar.Rooms
{
    public class PokerGamingRoom : Room
    {
        public List<PlayerInfo> PlayerInfos = null;
        public PokerGamingRoom(QueueRoom queueRoom,Form1 form1) : base(queueRoom,form1)
        {
            _server.printLine("In Poker Gaming Room");
            PlayerInfos = new List<PlayerInfo>(queueRoom.PlayerInfos);
            PokerGamingRoomStart poker = new PokerGamingRoomStart(PlayerInfos.ToArray());
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {0,3},
                {1,TCPServer.Math.Serializate.ToByteArray(poker) },
            };
            BroadcastPacket(packet);
        }

        public override void mainThread(object sender, ElapsedEventArgs e)
        {
            base.mainThread(sender, e);
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
                    }
                    break;
                #endregion
            }
        }
    }
}
