using UnityEngine;
using FTServer.Operator;
using System.Net;
using System;
using FTServer;

namespace ChatRoom
{
    public class FTServerConnecter : MonoBehaviour
    {
        /// <summary>
        /// 主要傳輸物件
        /// </summary>
        private Connect mConnect;
        public bool IsConnect = false;

        public void InitAndConnect(IPEndPoint ip, NetworkProtocol protocol,Action connect=null)
        {
            mConnect = new Connect(ip.Address.ToString(), ip.Port, protocol);
            mConnect._system.ConnectToServer();
            if (connect != null)
                mConnect._system.Connect += connect;
        }

        void OnDisable()
        {
            mConnect?.Dispose();
        }

        // Update is called once per frame
        protected void Update()
        {
            if (mConnect != null)
            {
                mConnect.Service();
                if (IsConnect != mConnect._system.IsConnect)
                    IsConnect = mConnect._system.IsConnect;
            }
        }

        /// <summary>
        /// 加入自定義的回傳處理者
        /// </summary>
        /// <param name="base"></param>
        public void AddCallBackHandler(byte operatorCode, CallBackHandler @base)
        {
            mConnect?.AddCallBackHandler(operatorCode, @base);
        }
        /// <summary>
        /// 清除所有自訂義的回傳處理者
        /// </summary>
        public void ClearCallBackHandler(byte code)
        {
            mConnect?.ClearCallBackHandler(code);
        }
    }
}
