using System;

namespace TCPServer.ClientInstance.Interface.Input
{
    interface ITCPClientCallBack
    {
        void onCompleteReadFromTCPClientStream(IAsyncResult iar);
        //void onCompleteWriteToClientStream(IAsyncResult iar);
    }
}
