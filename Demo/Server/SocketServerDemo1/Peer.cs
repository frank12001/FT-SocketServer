using System;
using System.Net;
using FTServer.Network;
using FTServer.ClientInstance;
using FTServer.ClientInstance.Packet;
using System.Collections.Generic;

namespace SocketServerDemo1
{
    public class Peer : PeerBase
    {
        static List<Peer> PeerList = new List<Peer>();
        static List<string> RoomTags = new List<string>();

        public string MRoomTag = "";

        public Peer(ISender sender, IPEndPoint iPEndPoint,FTServer.SocketServer socketServer): base(sender, iPEndPoint, socketServer, 10000)
        {
            PeerList.Add(this);
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

            if (packet.OperationCode == 12)
            {
                string code = packet.Parameters[0].ToString();
                if (code == "Join")
                {              
                    string roomtag = packet.Parameters[1].ToString();
                    if (!RoomTags.Contains(roomtag))
                        RoomTags.Add(roomtag);
                    MRoomTag = roomtag;
                    //Join 有可能換房，導致某房沒有人。須清清除
                }
                if (code == "Exit")
                {
                    string roomtag = this.MRoomTag;
                    this.MRoomTag = "";
                    if (!PeerList.Exists(peer => { return (peer.MRoomTag == roomtag); }))
                    {
                        if (RoomTags.Contains(roomtag))
                        {
                            RoomTags.Remove(roomtag);
                        }
                    }
                }
                if (code == "GetList")
                {
                    string[] result = RoomTags.ToArray();
                    SendEvent(packet.OperationCode,new Dictionary<byte, object>()
                    {
                        {0,code },
                        {1,result }
                    });
                }

                if (code == "Broadcast")
                {
                    if (MRoomTag != "")
                    {
                        List<Peer> roommates = PeerList.FindAll(peer => { return (peer.MRoomTag == this.MRoomTag); });
                        roommates.ForEach(peer => 
                        {
                            peer.SendEvent(packet.OperationCode, packet.Parameters);
                        });
                    }
                }
            }
        }
        public override void OnDisconnect()
        {
            PeerList.Remove(this);
            Console.WriteLine("OnDisconnect");
        }
    }
}
