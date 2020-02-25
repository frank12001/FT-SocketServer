using System.Net;
using FTServer;
using FTServer.ClientInstance;
using FTServer.Network;

namespace GumihoDemo
{
    public class MyPeer : PeerBase
    {
        public MyPeer(ISender sender, IPEndPoint iPEndPoint, SocketServer applicationInterface) : base(sender, iPEndPoint, applicationInterface)
        {
        }
    }
}