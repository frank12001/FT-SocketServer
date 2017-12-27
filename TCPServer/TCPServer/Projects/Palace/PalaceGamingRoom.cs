using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using startOnline;

namespace TCPServer.Projects.Palace
{
    public class PalaceGamingRoom : Room
    {
        public PalaceGamingRoom(string customName, PalacePeer[] joinPlayers, string roomIndexInApplication, Form1 applicationPointer) : base(customName,joinPlayers,roomIndexInApplication,applicationPointer)
        {
            _server.printLine("In Palace Gaming Room");
        }

        ~PalaceGamingRoom()
        {
            _server.printLine("Release Palace Gaming Room");
        }

        //private float t = 0;
        public override void mainThread(object sender, ElapsedEventArgs e)
        {
            //t += 30;
            //if (t > 50)
            //{
            //    //PalaceTest2[] yaya = new PalaceTest2[1];
            //    //for (int i = 0; i < yaya.Length; i++)
            //    //{
            //    //    yaya[i] = new PalaceTest2();
            //    //}
            //    Dictionary<byte, object> packet = new Dictionary<byte, object>()
            //    {
            //        {(byte) 0, 3}, //switch code
            //        {(byte) 1, TCPServer.Math.Serializate.ToByteArray(new PalaceTest2())},
            //    };
            //    BroadcastPacket(packet);
            //    t = 0;
            //}
        }

        public override void GamingProcess(byte playerId, Dictionary<byte, object> packet)
        {
            base.GamingProcess(playerId, packet);
        }
    }
}
