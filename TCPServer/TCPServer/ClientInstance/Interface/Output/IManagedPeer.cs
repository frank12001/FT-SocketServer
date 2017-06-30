using TCPServer.ClientInstance.Packet;

namespace TCPServer.ClientInstance.Interface.Output
{
    interface IManagedPeer
    {
        void OnOperationRequest(OperationRequest operationRequest);
    }
}
