using startOnline;
using startOnline.DataBase;
using startOnline.playar.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.playar.Rooms.Operator
{
    public class Operator
    {
        public Dictionary<string, Room> Rooms;
        private Form1 server;
        public Operator(Form1 form1)
        {
            this.server = form1;
            this.Rooms = new Dictionary<string, Room>();
        }
        #region Room Function
        public virtual Room Room_Create(PlayarPeer peer, string serialId, RoomTypes roomType)
        {
            lock ("RoomOperator")
            {
                Room room;
                //DisplayMessageBox(serialId);
                //string qrcode = DataBase.Operator.GetQRCode(serialId);//GetQRCode(serialId);
                string qrcode = "Test";
                server.PrintLine(qrcode);
                string id = qrcode;
                if (Room_IsRoomExist(id) || qrcode == null) //檢查有沒有相同 id 的房間
                    return null;
                switch (roomType)
                {
                    case RoomTypes.Base:
                        room = new Room(serialId, peer, id, server);
                        // Console 
                        server.printLine("基本房 + 1");
                        break;
                    case RoomTypes.Exhibition:
                        room = new ExhibitionRoom(serialId, peer, id, server);
                        // Console 
                        server.printLine("擴增房 + 1");
                        break;
                    default: //默認創建 base 房 
                        room = new Room(serialId, peer, id, server);
                        // Console 
                        server.printLine("基本房 + 1");
                        break;
                }

                this.Rooms.Add(room.RoomIndexInApplication, room);
                // Console 
                server.printLine("房間總數 + 1");
                return room;
            }
        }
        /// <summary>
        /// 創建房間 (從既有的房間創建)
        /// </summary>
        /// <param name="source">既有的房間</param>
        /// <param name="roomType">新房的種類</param>
        /// <returns>新房間</returns>
        public virtual Room Room_Create(Room source, RoomTypes roomType)
        {
            lock ("RoomOperator")
            {
                if (Rooms.ContainsKey(source.RoomIndexInApplication))
                {
                    Room_Remove(source.RoomIndexInApplication);
                }
                Room room;
                switch (roomType)
                {
                    case RoomTypes.Base:
                        room = new Room(source, server);
                        // Console 
                        server.printLine("基本房 + 1");
                        break;
                    case RoomTypes.Exhibition:
                        room = new ExhibitionRoom(source, server);
                        // Console 
                        server.printLine("擴增房 + 1");
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
        /// <summary>
        /// join assign room 
        /// </summary>
        /// <param name="roomIndexInApplication">the room's guid</param>
        /// <param name="peer">Joiner's peer</param>
        /// <param name="playid">return playid if join sucess</param>
        /// <returns>if not sucess return null</returns>
        public virtual Room Room_Join(string roomIndexInApplication, PlayarPeer peer, out byte playid)
        {
            lock ("RoomOperator")
            {
                Room room;
                if (!this.Rooms.TryGetValue(roomIndexInApplication, out room)) //if the room not exist 
                {
                    foreach (KeyValuePair<string, Room> rooms in Rooms)
                    {
                        //DisplayMessageBox(rooms.Key.ToString() + " now guid = " + roomIndexInApplication.ToString());
                    }
                    playid = 0;
                    return null;
                }
                if (room.Room_Join(peer, out playid))
                    return room;
                else
                    return null;
            }
        }
        public virtual void Room_Remove(string index) //區隔這個 class 呼叫的移除功能
        {                                    //和給其他程式呼叫的移除功能
            //給自己的不 lock
            if (Room_IsRoomExist(index))
            {
                //Console
                Room room;
                Rooms.TryGetValue(index, out room);

                server.printLine("基本房 - 1");
                server.printLine("房間總數 - 1");

                Rooms.Remove(index);
            }
        }
        public virtual bool Room_IsRoomExist(string roomIndexInApplication)
        {
            return this.Rooms.ContainsKey(roomIndexInApplication);
        }
        /// <summary>
        /// 回傳房間總數
        /// </summary>
        /// <returns></returns>
        public virtual int GetRoomsCount()
        {
            return this.Rooms.Count;
        }

        #endregion
    }
}
