using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using TCPServer.ClientInstance.Packet;

namespace Playar.PhotonServer
{
    public class GameNetWorkService : NetWorkBase
    {
        #region 函數

        /// <summary>
        /// 是否連線到伺服器
        /// </summary>
        private bool isConnect = false;                  
        #endregion      
        //建構式
        public GameNetWorkService()
        {}
        #region OnEvent 
        public delegate void ReceiveDictionaryHandler(Dictionary<byte, object> packet);
        public event ReceiveDictionaryHandler MemberEvent , RoomEvent ,  GamingEvent, SystemEvent;
        /// <summary>
        /// 在主執行續執行的處理
        /// </summary>
        /// <param name="eventData"></param>
        protected override void OnEvent(EventData eventData)
        {
            switch (eventData.OperationCode)
            {
                case 0: //會員操作
                    MemberEvent(eventData.Parameters);
                    break;
                case 1: //房間操作
                    RoomEvent(eventData.Parameters);
                    break;
                case 2: //遊戲中
                    GamingEvent(eventData.Parameters);
                    break;
                case 3: //系統
                    SystemEvent(eventData.Parameters);
                    break;
                case 5:
                    Debug.Log("EventData ForTest = " + eventData.ForTest);
                    break;
            }
        }
        #endregion
        #region OnStatusChanged 
        public delegate void ConnectHandler();
        public event ConnectHandler ConnectFromServer,DisconnectFromServer;
        protected override void OnStatusChanged(StatusCode statusCode)
        {
            switch (statusCode)
            {
                case StatusCode.Connect:
                    this.isConnect = true;
                    ConnectFromServer();
                    break;
                case StatusCode.Disconnect:
                    this.isConnect = false;
                    DisconnectFromServer();
                    break;
                default:
                    break;
            }
        }



        #endregion
    }
}