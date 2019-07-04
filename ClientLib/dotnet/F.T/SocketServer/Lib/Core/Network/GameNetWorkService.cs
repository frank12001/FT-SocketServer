using System;
using System.Collections.Generic;
using FTServer.Operator;
using FTServer.ClientInstance.Packet;

namespace FTServer
{
    public class GameNetworkService : NetworkBase
    {
        /// <summary>
        /// 連線及斷線的 callback
        /// </summary>
        public event Action ConnectFromServer, DisconnectFromServer;
        /// <summary>
        /// CallBackhandler
        /// </summary>
        private Dictionary<byte, CallbackHandler> _CallBackhandler = new Dictionary<byte, CallbackHandler>();

        //建構式
        public GameNetworkService(NetworkProtocol protocol) : base(protocol)
        {}

        #region 收到封包時的處理。 (CallBackHandler 機制)
        /// <summary>
        /// 在主執行續執行的處理。收到封包時觸發
        /// </summary>
        /// <param name="eventData"></param>
        protected override void OnEvent(IPacket eventData)
        {
            CallbackHandler networkBase = null;
            if (_CallBackhandler.TryGetValue(eventData.OperationCode, out networkBase))
                networkBase.ServerCallback(eventData.Parameters);
        }

        public void AddCallBackHandler(byte operatorCode, CallbackHandler netWorkBase)
        {
            if (_CallBackhandler.ContainsKey(operatorCode))
            {
                throw new Exception("不能使用系統使用的 operatorCode，該 code 為 255");
            }
            if (_CallBackhandler.ContainsKey(operatorCode))
            {
                throw new Exception("以加入過相同的 operatorCode，請確保他們是唯一的");            
            }
            _CallBackhandler.Add(operatorCode, netWorkBase);
        }
        public void ClearCallBackHandler(byte code)
        {
            if (_CallBackhandler.ContainsKey(code))
                _CallBackhandler.Remove(code);
        }
        #endregion 

        protected override void OnStatusChanged(StatusCode statusCode)
        {
            switch (statusCode)
            {
                case StatusCode.Connect:
                    ConnectFromServer?.Invoke();
                    break;
                case StatusCode.Disconnect:
                    DisconnectFromServer?.Invoke();
                    break;
            }
        }
    }
}