using System;
using System.Net;
using FTServer.Network;
using FTServer.ClientInstance;
using FTServer.ClientInstance.Packet;

namespace SocketServerDemo1
{
    public class Peer : PeerBase
    {
        static int count=0;
        public Peer(ISender sender, IPEndPoint iPEndPoint,FTServer.SocketServer socketServer): base(sender, iPEndPoint, socketServer, 10000)
        {     
            count++;
            Console.WriteLine("Create Peer. Count = "+ count);
        }

        ~Peer()
        {
            count--;
            Console.WriteLine("Release Peer. Count = "+count);
        }

        public override void OnOperationRequest(IPacket packet)
        {
            Console.WriteLine("i get something from OnOperationRequest.");
            if (packet.OperationCode == 10)
            {
                SendEvent(10, packet.Parameters);
            }
        }
        public override void OnDisconnect()
        {
            Console.WriteLine("OnDisconnect");
        }
    }
}
