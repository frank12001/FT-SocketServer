using System;
using System.Net;
using System.Threading;
using FTServer;
using FTServer.Network;
using FTServer.ClientInstance;

namespace SocketServerDemo1
{
    class Program : SocketServer
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.StartListen(30100, Protocol.RUDP);

            while (true)
            {
                Thread.Sleep(500);
            }
        }

        public override ClientNode GetPeer(Core core, IPEndPoint iPEndPoint, SocketServer socketServer)
        {
            Peer player = new Peer(core, iPEndPoint, socketServer);
            return player;
        }
    }
}
