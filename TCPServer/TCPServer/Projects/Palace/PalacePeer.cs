using System;
using System.Collections.Generic;
using System.Net.Sockets;
using startOnline;
using startOnline.playar.Rooms;
using TCPServer.Math;
using TCPServer.ClientInstance.Packet;
using TCPServer.Projects.Palace.Packet;
using TCPServer.Rooms.Operator;

namespace TCPServer.Projects.Palace
{
    public class PalacePeer : PeerBase
    {
        /// <summary>
        /// 該 Peer 的 Uid
        /// </summary>
        public Guid _Guid { get; private set; }
        public PalacePeer(Form1 app, TcpClient _tclient, byte[] _tx, byte[] _rx, string _str, IApplication applicationInterface) : base(app, _tclient, _tx, _rx, _str, applicationInterface)
        {
            _Guid = Guid.NewGuid();
        }

        ~PalacePeer()
        {
            _server.printLine("PalacePeer 解構子被呼叫");
        }

        public override void OnDisconnect()
        {
            _server.PrintLine("觸發 OnDisConnect ");
            base.OnDisconnect();

            //如果再排隊
            if (_Queueing)
            {
                if (_server.RoomOperator is PalaceQueueOperator queue3)
                {
                    queue3.ExitQueue(_Guid.ToString());
                    _Queueing = false;
                }
            }
            //如果在房間
            room?.Room_Exit(this.playeridInRoom);
        }

        private byte exeCode = 0;
        private PalaceQueueOperator _RoomOperator;
        private bool _Queueing;

        public override void OnOperationRequest(OperationRequest operationRequest)
        {
            try
            {
                switch (operationRequest.OperationCode)
                {
                    //需重構
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
                        switch (switchcode_1)
                        {
                            case 0:
                                if (room == null)
                                {
                                    string customName = operationRequest.Parameters[1].ToString();
                                    _server.PrintLine("嘗試創房 SN = " + customName);
                                    //依照傳進來的 RoomType 創建房間
                                    RoomTypes roomType = (RoomTypes) byte.Parse(operationRequest.Parameters[2].ToString());
                                    this.room = _server.RoomOperator.Room_Create(this, customName, roomType);
                                    if(this.room != null)
                                        packet.Add(2, this.room.RoomIndexInApplication);
                                    else
                                        issucess = false;
                                }
                                else
                                    issucess = false;
                                break;
                            case 1: //JoinRoom
                                //JoinRoom 不從這邊回傳
                                //從 Room 裡回傳
                                string targetRoom = Serializate.ToObject((byte[])operationRequest.Parameters[1]).ToString();
                                _server.PrintLine("嘗試加入房間 目標房間 = " + targetRoom);
                                this.room = _server.RoomOperator.Room_Join(targetRoom, this, out this.playeridInRoom);
                                break;
                            case 2:
                                if (room != null)
                                {
                                    this.room.Room_Exit(this.playeridInRoom);
                                    this.room = null;
                                }
                                break;
                            case 3: //switch code 3 是踢出房間
                                    //未實作
                                break;
                            case 4: //return players count 
                                byte playersCount = this.room.Room_GetPlayersCount();
                                packet.Add((byte)2, playersCount);
                                break;                                
                        }
                        if (!switchcode_1.Equals(1))  //JoinRoom 不從這邊回傳    //從 Room 裡回傳
                        {
                            packet.Add(1, issucess);
                            SendEvent((byte) OperationCode.Room, packet);
                        }
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
                                { (byte)1,(int)_server.RoomOperator.GetRoomsCount()},
                            };
                                SendEvent((byte)3, packet_system_2);
                                break;
                            case 4: //不會有東西，可是也不要用。 case 4 是Server 傳送訊息給 Client 用的

                                break;
                        }
                        break;
                    #endregion
                    case 5:
                        _server.PrintLine(DateTime.Now + " - " + this.ToString() + ": " + operationRequest.ForTest);
                        break;
                    #region 6 : Queue
                    case 6:
                        byte switchcode_6 = byte.Parse(operationRequest.Parameters[0].ToString());
                        switch (switchcode_6)
                        {
                            case 2://Join With info
                                if (_server.RoomOperator is PalaceQueueOperator queue)
                                {
                                    QueueInfo info =
                                        (QueueInfo)Serializate.ToObject((byte[])operationRequest.Parameters[1]);
                                    PalaceQueueInfo queueInfo = new PalaceQueueInfo() { Peer = this, Key = info.Key, Guid = _Guid.ToString() };
                                    this._RoomOperator = queue;
                                    this.room = this._RoomOperator.QueueJoin(queueInfo);
                                    _Queueing = true;

                                    packet = new Dictionary<byte, object>()
                                    {
                                        {(byte)0,2},
                                        {(byte)1,_Queueing},
                                        {(byte)2,Math.Serializate.ToByteArray(new object())}
                                    };
                                    SendEvent((byte)OperationCode.Queue, packet);
                                }
                                break;
                            case 3: //Exit Queue
                                bool success = false;
                                if (_server.RoomOperator is PalaceQueueOperator queue3)
                                {
                                    if (_Queueing)
                                    {
                                        success = queue3.ExitQueue(_Guid.ToString());
                                        this.room = null;
                                        _Queueing = false;
                                    }
                                }
                                packet = new Dictionary<byte, object>()
                                {
                                    {(byte)0,3},
                                    {(byte)1,success }
                                };
                                SendEvent((byte)OperationCode.Queue, packet);
                                break;
                        }
                        break;
                    #endregion

                    case 200: //不斷發送封包
                        //_server.PrintLine("Case 200 On");
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
    }
}
