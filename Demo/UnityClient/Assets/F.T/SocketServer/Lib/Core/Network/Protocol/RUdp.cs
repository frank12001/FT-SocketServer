﻿using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace FTServer
{
    public class RUdp : INetwork
    {
        EventBasedNetListener listener;
        NetManager client;
        NetPeer peer;

        public RUdp() : base(NetworkProtocol.RUDP)
        {
            listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.UpdateTime = 15;
            client.Start();

            listener.PeerConnectedEvent += (NetPeer peer) =>
            {
                Debug.Log("Connect Success : " + peer.EndPoint);
                this.peer = peer;
                fireCompleteConnect(true);
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                byte[] packet = new byte[dataReader.AvailableBytes];
                dataReader.GetBytes(packet, dataReader.AvailableBytes);
                fireCompleteReadFromServerStream(packet);
                dataReader.Recycle();
            };

        }

        public override void Service()
        {
            client.PollEvents();
        }

        public override void Connect(IPAddress addr, int port)
        {
            client.Connect(new IPEndPoint(addr, port), "SomeConnectionKey");
        }

        public override void BeginSend(byte[] datagram, int bytes)
        {
            peer.Send(datagram, DeliveryMethod.ReliableOrdered);
            fireCompleteSend();
        }

        public override void DisConnect()
        {
            client.Stop();           
        }
    }
}