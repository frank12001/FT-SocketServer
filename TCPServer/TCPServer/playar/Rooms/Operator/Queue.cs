using startOnline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.playar.Rooms.Operator
{
    public class Queue : Operator
    {
        public Dictionary<string, Room> queryRoom;
        public Queue(Form1 form1) : base(form1)
        {
            queryRoom = new Dictionary<string, Room>();
        }
        public Room QueueJoin(PlayarPeer peer, string serialId)
        {

        }
    }
}
