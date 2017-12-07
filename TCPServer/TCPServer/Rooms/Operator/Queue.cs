using startOnline;
using System;
using System.Collections.Generic;
using startOnline.playar.Rooms;


namespace TCPServer.playar.Rooms.Operator
{
    public class Queue : Operator
    {
        public Dictionary<string, QueueRoom> queryRoom;
        public Queue(Form1 form1) : base(form1)
        {
            queryRoom = new Dictionary<string, QueueRoom>();
        }
        public Room QueueJoin(PeerBase peer, string serialId,out byte playeridInRoom)
        {
            QueueRoom result = null;
            playeridInRoom = 0;
            if (queryRoom.Count.Equals(0))
            {
                string roomIndex = Guid.NewGuid().ToString();
                result = new QueueRoom(serialId,peer, roomIndex,server,2);
                queryRoom.Add(roomIndex, result);
            }
            else
            {
                foreach (KeyValuePair<string, QueueRoom> room in queryRoom)
                {
                    if (room.Value.Room_Join(peer, out playeridInRoom))
                    {
                        result = room.Value;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 創建房間 (從既有的房間創建)
        /// </summary>
        /// <param name="source">既有的房間</param>
        /// <param name="roomType">新房的種類</param>
        /// <returns>新房間</returns>
        public Room Room_QueueFinish(QueueRoom source, RoomTypes roomType)
        {
            lock ("RoomOperator")
            {
                if (queryRoom.ContainsKey(source.RoomIndexInApplication))
                {
                    queryRoom.Remove(source.RoomIndexInApplication);
                }
                Room room;
                switch (roomType)
                {
                    case RoomTypes.PokerGamingRoom:
                        room = new PokerGamingRoom(source,server);
                        // Console 
                        server.printLine("撲克房 + 1");
                        break;
                    default: //默認創建 base 房 
                        room = new Room(source, server);
                        // Console 
                        server.printLine("基本房 + 1");
                        break;
                }
                //DisplayMessageBox("現在房間列表 長度 = " + Rooms.Count);
                this.Rooms.Add(room.RoomIndexInApplication, room);
                // Console 
                server.printLine("房間總數 + 1");
                return room;
            }
        }
    }
}
