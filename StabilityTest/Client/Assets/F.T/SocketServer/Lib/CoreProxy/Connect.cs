using System;
using System.Net;
using FTServer.Operator;

namespace FTServer
{
    public class Connect : IDisposable
    {
        private GameNetworkService gameService;

        public NetworkProtocol NetworkProtocol { get; private set; }

        public Connect(string ip,int port,NetworkProtocol protocol)
        {
            IPAddress address;
            if (!IPAddress.TryParse(ip, out address))
            {
                throw new InvalidCastException("IP 格式錯誤，請輸入正確的 IPV4 IP ex : 35.123.123.123");
            }
            string ServerIP = address.ToString() + ":" + port;

            NetworkProtocol = protocol;
            gameService = new GameNetworkService(NetworkProtocol);

            //websocket 的地址，對應到伺服器 websocketsharp 開出的地址
            if (NetworkProtocol == NetworkProtocol.WebSocket)
                gameService.Address = string.Format("ws://{0}/WebSocket", ServerIP);
            else
                gameService.Address = ServerIP;
        }

        /// <summary>
        /// 加入自定義的回傳處理者
        /// </summary>
        /// <param name="operatorCode">每個處理者的唯一編號，對應到 Server 的傳出編號</param>
        /// <param name="base"></param>
        public void AddCallBackHandler(byte operatorCode,CallBackHandler @base)
        {
            @base.AddService(gameService);
            gameService.AddCallBackHandler(operatorCode, @base);
        }
        /// <summary>
        /// 清除所有自訂義的 callback handler
        /// </summary>
        public void ClearCallBackHandler(byte code)
        {
            gameService.ClearCallBackHandler(code);
        }

        /// <summary>
        /// 釋放時，需呼叫。建議搭配 using(xxx) 使用
        /// </summary>
        public void Dispose()
        {
            gameService.DisConnect();
        }

        /// <summary>
        /// 需要放在主執行續，不斷直行的功能
        /// </summary>
        public void Service()
        {
            gameService.Service();
        }
    }
}