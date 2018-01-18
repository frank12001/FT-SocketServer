using System;
using System.Collections.Generic;
using startOnline;
using startOnline.playar.Rooms;
using TCPServer.Rooms.Operator;

namespace TCPServer.Projects.Palace
{
    public class PalaceQueueInfo : BaseQueueInfo
    {

    }

    public class PalaceQueueOperator : QueueOperator<PalaceQueueInfo>
    {
        private const byte HowMuchPlayersJoinRoom = 2;
        public PalaceQueueOperator(Form1 form1) : base(form1, HowMuchPlayersJoinRoom)
        {

        }

        public override Room QueueJoinSuccess(List<BaseQueueInfo> infos)
        {
            string queryKey = infos[0].Key;
            PalacePeer[] peers = new PalacePeer[infos.Count];
            for (byte i = 0; i < peers.Length; i++)
            {
                peers[i] = infos[i].Peer as PalacePeer;
                peers[i]._Queueing = false; //排隊成功進入
            }
            string roomIndexInApplication = Guid.NewGuid().ToString();
            PalaceGamingRoom room = new PalaceGamingRoom(Guid.NewGuid().ToString(), peers, roomIndexInApplication,server);
            //將此房間加入，房間列表
            this.Rooms.Add(room.RoomIndexInApplication, room);
            //將此列隊刪除
            this._Queue.Remove(queryKey);
            return room;
        }

        public override Room QueueJoinFail(List<BaseQueueInfo> infos)
        {

            return null;
        }

        public override Room Room_Create(PeerBase peer, string serialId, RoomTypes roomType)
        {
            lock ("RoomOperator")
            {
                Room room;

                string id = Guid.NewGuid().ToString();
                //string id = "Test";
                server.PrintLine(id);
                if (Room_IsRoomExist(id)) //檢查有沒有相同 id 的房間
                    return null;
                PalacePeer palacePeer = peer as PalacePeer;
                switch (roomType)
                {
                    case RoomTypes.PalaceGamingRoom:
                        room = new PalaceGamingRoom(serialId,new PalacePeer[]{palacePeer}, id, server);
                        server.printLine("PalaceGamingRoom + 1");
                        break;
                    default: //默認創建 base 房 
                        room = new Room(serialId, peer, id, server);
                        server.printLine("基本房 + 1");
                        break;
                }
                this.Rooms.Add(room.RoomIndexInApplication, room);
                server.printLine("房間總數 + 1");
                return room;
            }
        }
    }
}
