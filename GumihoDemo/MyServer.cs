using System.Net;
using FTServer;
using FTServer.ClientInstance;
using FTServer.Network;

namespace GumihoDemo
{
    public class MyServer : SocketServer
    {
        public void Start(int port, Protocol protocol)
        {
            StartListen(port, protocol);
        }

        public override ClientNode GetPeer(Core core, IPEndPoint iPEndPoint, SocketServer application)
        {
            return new MyPeer(core, iPEndPoint, application);
        }
    }
}