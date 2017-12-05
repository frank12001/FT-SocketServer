using System;
using System.Collections.Generic;
using TCPServer.ClientInstance;
using TCPServer;
using System.Net.Sockets;
using TCPServer.ClientInstance.Packet;
using TCPServer.Math;

namespace startOnline
{
    public class PlayarPeer : ClientNode
    {
        private Form1 _server;

        /// <summary>
        /// in which room , now  . if null = not in room
        /// </summary>
        public Room room;
        byte playeridInRoom; //現在在room中的 id

        public PlayarPeer( Form1 app,TcpClient _tclient, byte[] _tx, byte[] _rx, string _str, IApplication applicationInterface) : base(_tclient,_tx,_rx, _str, applicationInterface)
        {
            this._server = app;
        }

        byte exeCode = 0; //當錯誤產生時，找到在哪執行時爆炸
        public override void OnOperationRequest(OperationRequest operationRequest)
        {
            try
            {
                switch (operationRequest.OperationCode)
                {
                    #region 0 : Member
                    case 0:
                        byte switchcode_0 = byte.Parse(operationRequest.Parameters[0].ToString());
                        exeCode = switchcode_0;
                        switch (switchcode_0)
                        {
                            #region 註冊新機器
                            case 1: //傳入公司名、地址、電話、有沒有product_tv、SN
                                    //用來註冊
                                string companyName = operationRequest.Parameters[1].ToString();
                                string phone = operationRequest.Parameters[2].ToString();
                                string address = operationRequest.Parameters[3].ToString();
                                bool product_tv = bool.Parse(operationRequest.Parameters[4].ToString());
                                string serialId = operationRequest.Parameters[5].ToString();
                                byte errorCode = 0;
                                string currect_qrocde = "";
                                bool result = DataBase.DataBase.Operator.Registered(out errorCode,out currect_qrocde, companyName, phone, address, product_tv, serialId);
                                Dictionary<byte, object> registerResponePakcet = new Dictionary<byte, object>()
                                {
                                    {(byte)0,1 }, //switch code
                                    {(byte)1,result  }, //register success or not
                                    {(byte)2,currect_qrocde },
                                    {(byte)3,errorCode },
                                };
                                SendEvent((byte)0, registerResponePakcet);
                                break;
                                #endregion
                            #region 查詢 ByCompanyName
                            case 2:
                                //傳入公司名稱
                                //回傳此公司的其他資訊
                                string companyNameCase2 = operationRequest.Parameters[1].ToString();
                                _server.PrintLine(companyNameCase2);
                                List<object> resultCase2 = DataBase.DataBase.Operator.Query(companyNameCase2);
                                if (resultCase2 == null || resultCase2.Count <= 1)
                                {
                                    Dictionary<byte, object> packetCase22 = new Dictionary<byte, object>() { { (byte)0, 2 } }; //switchCode;
                                    SendEvent((byte)0, packetCase22);
                                    break;
                                }
                                Dictionary<byte, object> packetCase2 = new Dictionary<byte, object>()
                                {
                                    //{(byte)0,2 }, //switchCode
                                    //{(byte)1,resultCase2[0]  },
                                    //{(byte)2,resultCase2[1]  },
                                    //{(byte)3,resultCase2[2]  },
                                    //{(byte)4,resultCase2[3]  },
                                    //{(byte)5,resultCase2[4]  },
                                    //{(byte)6,resultCase2[5]  },
                                    //{(byte)7,resultCase2[6]  },
                                    //{(byte)8,resultCase2[7]  },
                                    //{(byte)9,resultCase2[8]  },
                                };
                                packetCase2.Add(0, 2);
                                for (byte i = 1; i < resultCase2.Count; i++)
                                    packetCase2.Add(i, resultCase2[i - 1]);
                                SendEvent((byte)0, packetCase2);
                                break;
                            #endregion
                            #region 查詢 ByCompanyQRCode
                            case 3:
                                //傳入公司名稱
                                //回傳此公司的其他資訊
                                string qrcodeCase3 = operationRequest.Parameters[1].ToString();
                                _server.PrintLine(qrcodeCase3);
                                List<object> resultCase3 = DataBase.DataBase.Operator.GetCompanyInfo(qrcodeCase3);
                                if (resultCase3 == null || resultCase3.Count <= 1) //取得失敗
                                {
                                    Dictionary<byte, object> packetCase22 = new Dictionary<byte, object>() { { (byte)0, 2 } }; //switchCode;
                                    SendEvent((byte)0, packetCase22);
                                    break;
                                }
                                Dictionary<byte, object> packetCase3 = new Dictionary<byte, object>();
                                packetCase3.Add(0, 3);
                                for (byte i = 1; i <= resultCase3.Count; i++)
                                {
                                    //i = 0 放 switch Code 所以從，1 開始放
                                    if (resultCase3[i - 1] is DateTime)
                                        resultCase3[i - 1] = ((DateTime)resultCase3[i - 1]).Ticks;
                                    packetCase3.Add(i, resultCase3[i - 1]); 
                                }
                                SendEvent((byte)operationRequest.OperationCode, packetCase3);
                                break;
                                #endregion 
                        }
                        break;
                    #endregion
                    #region 1 : Room
                    case 1: //create room or add room
                            /* Receive packet
                             * 0 = switch code , create (0) or add (1)
                             * 1 = if(switchcode == 0) customName , if(switch == 1) TargetRoom.guid
                             */
                            /* Return packet
                             * 0 = this Command switch Code
                             * 1 = sucess or false
                             */
                        byte switchcode_1 = byte.Parse(operationRequest.Parameters[0].ToString());
                        exeCode = switchcode_1;
                        bool issucess = true;
                        Dictionary<byte, object> packet = new Dictionary<byte, object>
                    {
                       {0,switchcode_1},
                    };
                        if (room == null) //hasn't in room 
                        {
                            if (switchcode_1 == 0)  //create room
                            {
                                string customName = operationRequest.Parameters[1].ToString();
                                _server.PrintLine("嘗試創房 SN = " + customName);
                                //依照傳進來的 RoomType 創建房間
                                playar.Rooms.RoomTypes roomType = (playar.Rooms.RoomTypes)byte.Parse(operationRequest.Parameters[2].ToString());
                                _server.PrintLine("1");
                                this.room = _server.Room_Create(this, customName, roomType);
                                _server.PrintLine("2");
                                if (this.room == null) // false
                                    issucess = false;
                                else
                                    packet.Add(2, this.room.RoomIndexInApplication);
                                //DisplayMessageBox(this.room.RoomIndexInApplication.ToString("N"));
                            }
                            else if (switchcode_1 == 1) //join room
                            {
                                //DisplayMessageBox("some join room");
                                string targetRoom = Serializate.ToObject((byte[])operationRequest.Parameters[1]).ToString();
                                _server.PrintLine("嘗試加入房間 目標房間 = " + targetRoom);
                                this.room = _server.Room_Join(targetRoom, this, out this.playeridInRoom);

                                if (this.room == null) // false
                                    issucess = false;
                                if (issucess) //if join room success
                                {
                                    packet.Add((byte)2, (byte)this.room.Type);
                                    //DisplayMessageBox("join sucess guid = " + this.room.RoomIndexInApplication.ToString("N"));
                                    //DisplayMessageBox("now player = " + this.room.players.Count);
                                }
                                //JoinRoom 不從這邊回傳
                                //從 Room 裡回傳
                                break;
                            }
                        }
                        else //has in room 
                        {
                            if (switchcode_1 == 2) //Exit room
                            {
                                this.room.Room_Exit(this.playeridInRoom);
                                this.room = null;
                                issucess = true;
                            }                           // switch code 3 是踢出房間
                            else if (switchcode_1 == 4) //return players count 
                            {
                                byte playersCount = this.room.Room_GetPlayersCount();
                                packet.Add((byte)2, playersCount);
                            }
                            else
                            {
                                issucess = false;
                            }

                        }
                        //DisplayMessageBox(" packet  " + issucess);
                        packet.Add(1, issucess);
                        SendEvent((byte)OperationCode.Room, packet);
                        break;
                    #endregion
                    #region 2 : Gaming 
                    case 2: //傳進 room 的遊戲邏輯處理區，進行處理
                        exeCode = 0;
                        this.room.GamingProcess(this.playeridInRoom, operationRequest.Parameters);
                        break;
                    #endregion
                    #region 3 : System
                    case 3: //System
                            /*
                             * 系統功能
                             * 這裡的功能不一定要經過 Room
                             */
                        byte switchcode_3 = byte.Parse(operationRequest.Parameters[0].ToString());
                        exeCode = switchcode_3;
                        switch (switchcode_3)
                        {
                            case 0: //計算 Ping 
                                    //直接回傳 
                                SendEvent((byte)3, operationRequest.Parameters);
                                break;
                            case 2: //取得現在房間數量
                                Dictionary<byte, object> packet_system_2 = new Dictionary<byte, object>()
                            {
                                { (byte)0,(byte)2 }, //switch code
                                { (byte)1,(int)_server.GetRoomsCount()},
                            };
                                SendEvent((byte)3, packet_system_2);
                                break;
                            case 3: //取得Compamy info 並回傳
                                _server.printLine("1 - 1");
                                string qrcode = operationRequest.Parameters[1].ToString();
                                _server.printLine("qrcode");
                                List<object> company_list = DataBase.DataBase.Operator.GetCompanyInfo(qrcode);
                                _server.printLine("1 - 2");
                                _server.printLine("company_list count = "+ company_list.Count);
                                object[] company_array = new object[10];

                                if (company_list != null)
                                    company_array = company_list.ToArray();
                                else 
                                {    //就算沒有植有要強迫賦予
                                    for (byte j = 0; j < company_array.Length; j++)
                                    {
                                        company_array[j] = j;
                                    }
                                }
                                _server.printLine("1 - 3");

                                Dictionary<byte, object> packet_system_3 = new Dictionary<byte, object>()
                                {
                                    { (byte)0,(byte)3 }, //switch code
                                    { (byte)1,Serializate.ToByteArray(company_array)},
                                };
                                SendEvent((byte)3, packet_system_3);
                                _server.printLine("1 - 4");
                                break;
                            case 4: //不會有東西，可是也不要用。 case 4 是Server 傳送訊息給 Client 用的

                                break;
                        }
                        break;
                    #endregion
                    case 5:
                        _server.PrintLine(DateTime.Now + " - " + this.ToString() + ": " + operationRequest.ForTest);
                        break;
                    case 200:
                        _server.PrintLine("Case 200 On");
                        SendEvent(200, operationRequest.Parameters);
                        break;
                            
                }
            }
            catch (Exception e)
            {
                _server.PrintLine("peer OnOperationRequest error = " + e.Message);
                _server.PrintLine("OperationCode = " + operationRequest.OperationCode + "/ ExeCode = " + exeCode);
            }
        }

        public override void OnDisconnect()
        {
            //DisplayMessageBox(" peer is disconnect ");
            if (this.room != null) //if in room when disconnect
            {
                //DisplayMessageBox(" call room Exit ");
                //先呼叫自己斷線了
                this.room.DisConnect(this.playeridInRoom);
                //在呼叫離開此房間
                this.room.Room_Exit(this.playeridInRoom);
                //將此房間設為 null
                this.room = null;
            }
        }

        #region Room event 
        /// <summary>
        /// called when kicked by room
        /// </summary>
        public void RoomWasKicked()
        {
            if (this.room != null)
                this.room = null;
            //Send Message
            //要發新的包
            Dictionary<byte, object> packet = new Dictionary<byte, object>
            {
                {0,3},
                {1,true},
            };
            SendEvent(1, packet);
        }
        #endregion 
        #region myFunction
        public void SendEvent(byte eventCode, Dictionary<byte, object> packet)
        {
            EventData eventData = new EventData((byte)eventCode, packet);
            base.SendEvent(eventData);
        }
        /// <summary>
        /// 傳送伺服器訊息給此 Client。 Client 用 Connect.System.ReveiveServerLog 接
        /// </summary>
        public void SendServerLog(string message)
        {
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {(byte)0,4}, //swicth Code (固定)
                {(byte)1,message }, //Message
            };
            EventData eventData = new EventData((byte)3, packet); //event Code = 3,System
            SendEvent(eventData);
        }
        #endregion

    }
}
