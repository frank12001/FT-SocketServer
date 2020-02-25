using FTServer.ClientInstance;

namespace FTServer.Network
{
    public class Instance
    {
        public readonly ClientNode _ClientNode;
        public Instance(ClientNode clientNode)
        {
            _ClientNode = clientNode;
        }
    }
}
