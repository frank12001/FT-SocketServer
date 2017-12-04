using startOnline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TCPServer.playar.Rooms
{
    public class QueueRoom : Room
    {

        private byte waitHowMuchPeople = 0;
        public QueueRoom(string customName, PlayarPeer ownerPeer, string roomIndexInApplication, Form1 applicationPointer, byte waitHowMuchPeople) : base(customName, ownerPeer, roomIndexInApplication, applicationPointer)
        {
            this.waitHowMuchPeople = waitHowMuchPeople;
        }

        #region 遊戲流程
        public override void mainThread(object sender, ElapsedEventArgs e)
        {
            base.mainThread(sender, e);
            //Log.Log.ToTxt("timer interal = " + (timer_interal * 0.001));
            //seamanController.Update(timer_interal * 0.001f); //以註解
            if (players.Count.Equals(waitHowMuchPeople))
            {
                //轉換房間成，遊戲防
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
            }
            //最後再進行 base.GamingProcess
            base.GamingProcess(playerId, packet); //switch code 已使用 0 , 1 , 2
        }
        #endregion
    }
}
