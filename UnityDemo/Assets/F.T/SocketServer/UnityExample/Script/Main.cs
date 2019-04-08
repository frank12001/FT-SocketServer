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
        GroupCallBackHandler groupCallBackHandler;

        public string Key, Value;
        public string RoomKey;
        public string BroadString;
        // Use this for initialization
        void Start()
        {
            connecter = GetComponent<FTServerConnecter>();
            connecter.InitAndConnect(new IPEndPoint(IPAddress.Parse("104.199.194.170"), 30100),NetworkProtocol.RUDP,()=> { Debug.Log("Connected!"); });

            myCallBack = new MyCallBackHandler();
            connecter.AddCallBackHandler(10,myCallBack);
       
            accountCallBack = new AccountCallBackHandler(11);
            connecter.AddCallBackHandler(11, accountCallBack);
            accountCallBack.GetAction += res => { Debug.Log("Get Account Response: " + res); };
            accountCallBack.SetAction += res => { Debug.Log("Set Account Response: " + res); };

            groupCallBackHandler = new GroupCallBackHandler(12);
            connecter.AddCallBackHandler(12, groupCallBackHandler);
            groupCallBackHandler.GetListAction += o => {
                string msg = "";
                foreach (string s in o)
                {
                    msg += s + " ,";
                }
                Debug.Log("receive roomlist msg : "+msg);
            };
            groupCallBackHandler.BroadcastAction += o =>{ Debug.Log("receive broadcast msg : "+o); };
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
            if (Input.GetKeyDown(KeyCode.Y))
            {
                groupCallBackHandler.Join(RoomKey);
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                groupCallBackHandler.Broadcast(BroadString);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                groupCallBackHandler.GetList();
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                groupCallBackHandler.Exit();
            }
        }
    }

    public class GroupCallBackHandler : CallBackHandler
    {
        public Action<string[]> GetListAction;
        public Action<object> BroadcastAction;
        private byte OperatorCode;
        public GroupCallBackHandler(byte operatorCode)
        {
            this.OperatorCode = operatorCode;
        }

        public void Join(string key)
        {
            gameService.Deliver(OperatorCode, new Dictionary<byte, object>()
            {
                {0,"Join"},
                {1,key}
            });
        }
        public void Exit()
        {
            gameService.Deliver(OperatorCode, new Dictionary<byte, object>()
            {
                {0,"Exit"}
            });
        }
        public void GetList()
        {
            gameService.Deliver(OperatorCode, new Dictionary<byte, object>()
            {
                {0,"GetList"}
            });
        }
        public void Broadcast(string msg)
        {
            gameService.Deliver(OperatorCode, new Dictionary<byte, object>()
            {
                {0,"Broadcast"},
                {1,msg }
            });
        }


        public override void ServerCallBack(Dictionary<byte, object> server_packet)
        {
            string code = server_packet[0].ToString();
            switch (code)
            {
                case "GetList":
                    object[] resGetList = (object[])server_packet[1];
                    List<string> s = new List<string>();
                    foreach (object o in resGetList)
                    {
                        s.Add(o.ToString());
                    }
                    GetListAction?.Invoke(s.ToArray());
                    break;
                case "Broadcast":
                    string resBroadcast = server_packet[1].ToString();
                    BroadcastAction?.Invoke(resBroadcast);
                    break;
                default:
                    Debug.Log("GroupCallBackHandler.ServerCallBack code u=is wrong.");
                    break;
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
        private byte OperatorCode;
        public AccountCallBackHandler(byte operatorCode)
        {
            this.OperatorCode = operatorCode;
        }

        public void Get(string key)
        {
            if (key.Length > HttpMaxLangth)
                throw new Exception("key 長度不能超過 2000 現在的長度是 : " + key.Length);
            gameService.Deliver(OperatorCode, new Dictionary<byte, object>()
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
            gameService.Deliver(OperatorCode, new Dictionary<byte, object>()
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