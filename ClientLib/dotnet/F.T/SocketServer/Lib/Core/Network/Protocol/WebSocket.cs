using System;
using System.Threading;

namespace FTServer
{
    public class WebSocketLis : INetwork
    {
        private CancellationToken cancelToken;

        private bool needFireDisconnect = false;
        public WebSocketLis() : base(NetworkProtocol.WebSocket)
        { }

        public override void Connect(Uri uri)
        {
            throw new NotImplementedException();
        }

        public override void BeginSend(byte[] datagram, int bytes)
        {
        }

        public override void DisConnect()
        {           
        }

        private void SetFireDisconnectTrigger()
        {
            needFireDisconnect = true;
        }

        private void CheckAndFireDisconnect()
        {
            if (needFireDisconnect)
            {
                needFireDisconnect = false;
                fireCompleteDisconnect();
            }
        }

        protected override void onCompleteConnect(IAsyncResult iar)
        {
            try
            {
                fireCompleteConnect();
            }
            catch (Exception exc)
            {
                fireCompleteDisconnect();
                Console.WriteLine(exc.Message);
            }
        }
    }
}