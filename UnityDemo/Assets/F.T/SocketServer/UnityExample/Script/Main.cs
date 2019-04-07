using FTServer.Operator;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace FTServer.Example
{
    public class Main : MonoBehaviour
    {
        FTServerConnecter connecter;
        MyCallBackHandler myCallBack;
        AccountCallBackHandler accountCallBack;

        public string Key, Value;
        // Use this for initialization
        void Start()
        {
            connecter = GetComponent<FTServerConnecter>();
            connecter.InitAndConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30100),NetworkProtocol.RUDP,()=> { Debug.Log("Connected!"); });

            myCallBack = new MyCallBackHandler();
            connecter.AddCallBackHandler(10,myCallBack);

            accountCallBack = new AccountCallBackHandler();
            connecter.AddCallBackHandler(11, accountCallBack);
            accountCallBack.GetAction += res => { Debug.Log("Get Account Response: " + res); };
            accountCallBack.SetAction += res => { Debug.Log("Set Account Response: " + res); };
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                myCallBack.Send();
            }
            //if(connecter.IsConnect)
            //   myCallBack.Send();
            if (Input.GetKeyDown(KeyCode.R))
            {                
                accountCallBack.Set(Key, Value);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                accountCallBack.Get(Key);
            }
        }
    }

    public class MyCallBackHandler : CallBackHandler
    {
        public void Send()
        {
            string s = "";
            for (int i = 0; i < 700; i++)
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

    public class AccountCallBackHandler : CallBackHandler
    {
        private const int HttpMaxLangth = 2000;
        public Action<string> GetAction, SetAction;
        public void Get(string key)
        {
            if (key.Length > HttpMaxLangth)
                throw new Exception("key 長度不能超過 2000 現在的長度是 : " + key.Length);
            gameService.Deliver(11, new Dictionary<byte, object>()
            {
                {0,"Get"},
                {1,key}
            });
        }
        public void Set(string key, string value)
        {
            int totalLength = key.Length + value.Length;
            if (totalLength > HttpMaxLangth)
                throw new Exception("key + value 長度不能超過 2000. 現在的長度是 : " + totalLength);
            gameService.Deliver(11, new Dictionary<byte, object>()
            {
                {0,"Set"},
                {1,key},
                {2,value }
            });
        }
        public override void ServerCallBack(Dictionary<byte, object> server_packet)
        {
            string code = server_packet[0].ToString();
            string response = server_packet[1].ToString();
            switch (code)
            {
                case "Get":
                    GetAction?.Invoke(response);
                    break;
                case "Set":
                    SetAction?.Invoke(response);
                    break;
                default:
                    Debug.Log("AccountCallBackHandler.ServerCallBack code u=is wrong.");
                    break;
            }
        }
    }
}