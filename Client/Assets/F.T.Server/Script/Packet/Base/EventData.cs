using System;
using System.Collections.Generic;


namespace TCPServer.ClientInstance.Packet
{
    [Serializable]
    public class EventData : IPacket
    {
        public EventData(byte eventCode, Dictionary<byte, object> parameters) : base(eventCode, parameters)
        {

        }
    }
}
