using System;
using System.Collections.Generic;


namespace TCPServer.ClientInstance.Packet
{
    [Serializable]
    public class OperationRequest : IPacket
    {
        public OperationRequest(byte operationCode, Dictionary<byte, object> parameters) : base(operationCode, parameters)
        {

        }
    }
}
