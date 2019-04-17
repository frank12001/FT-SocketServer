using FTServer;
using FTServer.ClientInstance;
using FTServer.ClientInstance.Packet;
using FTServer.Network;
using System;
using System.Net;
using System.Threading;

namespace Server
{
    class Program : SocketServer
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.StartListen(30100, Protocol.RUDP);
            while (true) { Thread.Sleep(500); }
        }
        public override ClientNode GetPeer(Core core, IPEndPoint iPEndPoint, SocketServer socketServer)
        {
            Peer player = new Peer(core, iPEndPoint, socketServer);
            return player;
        }
    }
    public class Peer : PeerBase
    {
        public Peer(ISender sender, IPEndPoint iPEndPoint, FTServer.SocketServer socketServer) : base(sender, iPEndPoint, socketServer, 10000)
        { }
        public override void OnOperationRequest(IPacket packet)
        {
            if (packet.OperationCode == 20)
            {
                Console.WriteLine("Client tell me : " + packet.Parameters[0].ToString());
                SendEvent(packet.OperationCode, new System.Collections.Generic.Dictionary<byte, object>()
                {
                    {0,"hello client!" }
                });
            }
        }
        public override void OnDisconnect()
        {
            Console.WriteLine("OnDisconnect");
        }      
    }
}
