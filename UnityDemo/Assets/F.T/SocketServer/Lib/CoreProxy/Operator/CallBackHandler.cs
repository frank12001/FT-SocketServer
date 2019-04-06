using System.Collections.Generic;


namespace FTServer.Operator
{
    //基底 class
    public abstract class CallBackHandler : IServerCallBack
    {
        protected GameNetworkService gameService;

        public void AddService(GameNetworkService _gameService)
        {
            gameService = _gameService;
            if (gameService != null)
                AfterAddService();
        }
        /// <summary>
        /// 在確定有 gameService 物件後觸發
        /// </summary>
        protected virtual void AfterAddService()
        { }

        public abstract void ServerCallBack(Dictionary<byte, object> server_packet);
    }

    interface IServerCallBack
    {
        void ServerCallBack(Dictionary<byte, object> server_packet);
    }

  
}
