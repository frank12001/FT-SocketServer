using System;
using System.Collections.Generic;
using startOnline;
using startOnline.playar.Rooms;
using TCPServer.playar.Rooms;
using TCPServer.playar.Rooms.Operator;

namespace TCPServer.Projects.Stellar
{
    public class PokerQueueOperator : Operator
    {
        public const byte HowMuchPlayersJoinRoom = 6;
        public Dictionary<string, Dictionary<string, PokerQueuePlayerInfo>> _Queue;
        //public Dictionary<string, PokerGamingRoom> _PokerGamingRoom;
        public PokerQueueOperator(Form1 form1) : base(form1)
        {
            _Queue = new Dictionary<string, Dictionary<string, PokerQueuePlayerInfo>>();
        }
        public Room QueueJoin(PokerQueuePlayerInfo pokerQueuePlayerInfo)
        {
            lock ("QueueJoin")
            {
                Room room = null;
                //解出分類 Key
                var queryKey = pokerQueuePlayerInfo._PlayerInfo.ChipMultiple.ToString();
                Dictionary<string, PokerQueuePlayerInfo> queue = null;
                if (!_Queue.TryGetValue(queryKey, out queue)) //如果沒有這個分類的列隊
                {
                    //創一個新列隊，並將自己加入
                    queue = new Dictionary<string, PokerQueuePlayerInfo>() {
                    { pokerQueuePlayerInfo._PokerPeer._Guid.ToString(), pokerQueuePlayerInfo }
                };
                    _Queue.Add(queryKey, queue);
                }
                else
                {
                    //將自己加入列隊
                    queue.Add(pokerQueuePlayerInfo._PokerPeer._Guid.ToString(), pokerQueuePlayerInfo);
                }
                //檢查此列隊的人數，是否達到足夠開房
                if (queue.Count.Equals(HowMuchPlayersJoinRoom))
                {
                    //製作開房需要的參數，並開房
                    List<PlayerInfo> playerInfos = new List<PlayerInfo>();
                    List<PokerPeer> peerBase = new List<PokerPeer>();
                    foreach (KeyValuePair<string, PokerQueuePlayerInfo> player in queue)
                    {
                        playerInfos.Add(player.Value._PlayerInfo);
                        peerBase.Add(player.Value._PokerPeer);
                        player.Value._PokerPeer._Queueing = false;
                    }
                    room = new PokerGamingRoom(playerInfos, "PokerGamingRoom", peerBase.ToArray(), Guid.NewGuid().ToString(), server);
                    // Console 
                    server.printLine("撲克房 + 1");
                    //將此房間加入，房間列表
                    this.Rooms.Add(room.RoomIndexInApplication, room);
                    //將此列隊刪除
                    this._Queue.Remove(queryKey);
                }
                else
                {   //如果排隊人數不夠
                    List<PlayerInfo> playerInfos = new List<PlayerInfo>();
                    foreach (KeyValuePair<string, PokerQueuePlayerInfo> player in queue)
                    {
                        playerInfos.Add(player.Value._PlayerInfo);
                    }
                    foreach (KeyValuePair<string, PokerQueuePlayerInfo> playerInfo in queue)
                    {
                        playerInfo.Value.Send(playerInfos.ToArray());
                    }
                }

                return room;
            }
        }
        /// <summary>
        /// 排隊所需的各個玩家資訊
        /// </summary>
        public class PokerQueuePlayerInfo {
            public PokerPeer _PokerPeer;
            public PlayerInfo _PlayerInfo;
            public void Send(PlayerInfo[] playersInfo)
            {
                var packet = new Dictionary<byte, object>()
                {
                     {(byte) 0, 1},
                     {(byte) 1, true},
                     {(byte)2, Math.Serializate.ToByteArray(playersInfo) }
                };
                _PokerPeer.SendEvent(6, packet);
            }
        }

        /// <summary>
        /// 刪掉排隊列隊中，指定 Guid 的玩家 Peer
        /// </summary>
        /// <param name="peerGuid"></param>
        public void RemoveQueuingPeer(string peerGuid)
        {
            foreach (KeyValuePair<string, Dictionary<string, PokerQueuePlayerInfo>> queue in _Queue)
            {
                if (queue.Value.ContainsKey(peerGuid))
                {
                    queue.Value.Remove(peerGuid);
                } 
            }
        }

        //public override void Room_Remove(string index)
        //{
        //    //給自己的不 lock
        //    if (Room_IsRoomExist(index))
        //    {
        //        //Console
        //        PokerGamingRoom room;
        //        _PokerGamingRoom.TryGetValue(index, out room);

        //        server.printLine("基本房 - 1");
        //        server.printLine("房間總數 - 1");

        //        _PokerGamingRoom.Remove(index);
        //    }
        //}

        //public override bool Room_IsRoomExist(string roomIndexInApplication)
        //{
        //    return _PokerGamingRoom.ContainsKey(roomIndexInApplication);
        //}
    }
}

