using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using CustomConnectPhotonFish;
using System.Timers;
using TCPServer;
using TCPServer.Math;

namespace startOnline
{
    public class ExhibitionRoom : Room
    {
        /// <summary>
        /// 資料庫中公司的資訊，對應到 customer 資料表
        /// </summary>
        private List<object> companyInfo;
        /// <summary>
        /// 用來定義，伺服器狀態。 0 = default , 1 = 舞台演出中 
        /// </summary>
        private byte ServerState = 0;
        /// <summary>
        /// 用來限制 Fishing4DCubePos 傳輸量
        /// </summary>
        private byte Fishing4DCubePosPerFrame = 0;


        

        private const string ErrorMessage_1 = "正在舞台演出";

        
        
        

        #region 建構子
        public ExhibitionRoom(string customName, PeerBase ownerPeer, string roomIndexInApplication,Form1 applicationPointer) : base(customName,ownerPeer,roomIndexInApplication,applicationPointer)
        {           
            //向資料庫取得公司資訊
            companyInfo = DataBase.DataBase.Operator.GetCompanyInfo(this.RoomIndexInApplication);
            //取完後 Log 公司名
            _server.PrintLine("開房者為 : " + companyInfo[1].ToString());
        }
        /// <summary>
        /// 透過 room 建構另一個 room
        /// </summary>
        /// <param name="room"></param>
        public ExhibitionRoom(Room room, Form1 applicationPointer) : base(room,applicationPointer)
        {
        }
        #endregion
        #region 解構子
        ~ExhibitionRoom()
        {
            _server.PrintLine(companyInfo[1].ToString() + "的房間以清除");
        }
        #endregion
        #region 遊戲流程
        public override void mainThread(object sender, ElapsedEventArgs e)
        {
            base.mainThread(sender, e);
            //Log.Log.ToTxt("timer interal = " + (timer_interal * 0.001));
            //seamanController.Update(timer_interal * 0.001f); //以註解
                                
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
                #region case 3 取得公司資訊
                case 3: //switch code == 3 
                    try
                    {
                        Dictionary<byte, object> o = new Dictionary<byte, object>();
                        for (byte i = 0; i < this.companyInfo.Count; i++)
                        {
                            if (companyInfo[i] is System.DateTime) //如果是 DateTime 要特別做轉型。Photon 能傳送的型態不包含 DateTime
                            {
                                companyInfo[i] = ((System.DateTime)companyInfo[i]).Ticks;
                            }
                            o.Add(i, companyInfo[i]);
                        }
                        //回傳此房間公司資訊
                        Dictionary<byte, object> packet1 = new Dictionary<byte, object>()
                        {
                            { (byte)0,3 },
                            { (byte)2,o},
                        };
                        SendToAssignPlayer(packet1, playerId);
                    }
                    catch (Exception e)
                    {
                        //DisplayMessageBox(e.Message);
                        _server.PrintLine("ExhibitionRoom.cs Error = " + e.Message);
                    }

                    break;
                #endregion
                #region case 0 轉傳客製化封包 
                case 0: //轉傳客製化封包
                        //在 base.GamingProcess 是將封包無條件轉给 RoomOwner 
                        //在這裡先過濾，如條件不合直接 return ，不執行 base.GamingProcess .case 2
                        // 1. 嘗試解包
                    byte switchcode_2 = byte.Parse(packet[1].ToString());
                    switch (switchcode_2)
                    {
                        case 2:    //轉傳給 RoomOwner 
                            
                            
                            #region 用來限制 Fishing4DCubePos 這個封包的傳輸
                            object custom_class = Serializate.ToObject((byte[])packet[2]);
                            if (custom_class is Fishing4DCubePos)
                            {
                                Fishing4DCubePosPerFrame++;
                                if (Fishing4DCubePosPerFrame % 5 == 0)
                                    Fishing4DCubePosPerFrame = 0;
                                else
                                    return;
                            }
                            #endregion
                            break;
                    }
                    break;
                    #endregion 
            }
            //最後再進行 base.GamingProcess
            base.GamingProcess(playerId, packet); //switch code 已使用 0 , 1 , 2
        }
        #endregion

        private void DisplayMessageBox(string context)
        {
            new Thread(new ThreadStart(delegate
            {
                MessageBox.Show(context);
            })).Start();
        }
    }
}
