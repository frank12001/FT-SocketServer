using System;
using System.Net;

namespace FTServer
{
    public abstract class INetwork
    {
        public NetworkProtocol NetworkProtocol { get; private set; }

        public event Action CompleteDisConnect, CompleteSend;
        public event Action<bool> CompleteConnect;
        public event Action<byte[]> CompleteReadFromServerStream;

        public INetwork(NetworkProtocol protocol)
        {
            NetworkProtocol = protocol;
        }

        #region Fire Event

        protected void fireCompleteDisconnect()
        {
            if (CompleteDisConnect != null)
                CompleteDisConnect();
        }

        protected void fireCompleteSend()
        {
            if (CompleteSend != null)
                CompleteSend();
        }

        protected void fireCompleteConnect(bool isConnect)
        {
            if (CompleteConnect != null)
                CompleteConnect(isConnect);
        }

        protected void fireCompleteReadFromServerStream(byte[] datagram)
        {
            if (CompleteReadFromServerStream != null)
                CompleteReadFromServerStream(datagram);
        }

        #endregion

        public abstract void BeginSend(byte[] datagram, int bytes);

        public abstract void DisConnect();

        public virtual void Connect(IPAddress addr, int port)
        {}

        public virtual void Connect(Uri uri)
        {}

        protected virtual void onCompleteConnect(IAsyncResult iar)
        {}

        protected virtual void onCompleteReadFromServerStream(IAsyncResult iar)
        {}

        public virtual void Service()
        { }
    }
}