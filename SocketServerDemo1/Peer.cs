using System;
using System.Net;
using FTServer.Network;
using FTServer.ClientInstance;
using FTServer.ClientInstance.Packet;

namespace SocketServerDemo1
{
    public class Peer : PeerBase
    {
        public Peer(ISender sender, IPEndPoint iPEndPoint,FTServer.SocketServer socketServer): base(sender, iPEndPoint, socketServer, 10000)
        {     
        }

        ~Peer()
        {
        }

        public override void OnOperationRequest(IPacket packet)
        {
            if (packet.OperationCode == 11)
            {
                string code = packet.Parameters[0].ToString();
                string key = packet.Parameters[1].ToString();
                void res(string response)
                {
                    SendEvent(11, new System.Collections.Generic.Dictionary<byte, object>()
                        {
                            {0,code},
                            {1,response}
                        });
                }
                if (code == "Get")
                {
                    Member.GetAccount(key, res);
                }
                if (code == "Set")
                {
                    string value = packet.Parameters[2].ToString();
                    Member.SetAccount(key, value, res);
                }
            }
        }
        public override void OnDisconnect()
        {
            Console.WriteLine("OnDisconnect");
        }
    }
}
