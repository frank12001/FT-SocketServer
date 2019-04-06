using FTServer;
using FTServer.ClientInstance.Packet;
using FTServer.Math;
using FTServer.Operator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FTServer.Example
{
    public class CustomCallBackHandler : MonoBehaviour
    {
        private Connect m_Connect;
        private MyCallBackHandler myCallBackHandler;

        // Use this for initialization
        void Start()
        {
            m_Connect = new Connect("127.0.0.1", 30000, NetworkProtocol.UDP);
            //m_Connect._system.ConnectToServer();
            myCallBackHandler = new MyCallBackHandler();
        }

        // Update is called once per frame
        void Update()
        {
            m_Connect.Service();
            if (Input.GetKeyDown(KeyCode.W))
            {
                m_Connect.AddCallBackHandler(10, myCallBackHandler);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                myCallBackHandler.Send();
            }
        }
    }

    public class MyCallBackHandler : CallBackHandler
    {
        public void Send()
        {
            string s = "";
            for (int i = 0; i < 1/*700*/; i++)
            {
                s += i;
            }
            gameService.Deliver(10, new Dictionary<byte, object>()
            {
                {0,s },
            });
        }
        public override void ServerCallBack(Dictionary<byte, object> server_packet)
        {
            Debug.Log("server_packet in");
        }
    }
}