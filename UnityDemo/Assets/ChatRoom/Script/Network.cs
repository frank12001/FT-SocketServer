using FTServer;
using FTServer.Operator;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace ChatRoom
{
    public class Network : MonoBehaviour
    {
        FTServerConnecter connecter;
        public AccountCallBackHandler _AccountCallBack;
        public GroupCallBackHandler _GroupCallBackHandler;
        // Use this for initialization
        void Awake()
        {
            connecter = GetComponent<FTServerConnecter>();
            connecter.InitAndConnect(new IPEndPoint(IPAddress.Parse("104.199.194.170"), 30100), NetworkProtocol.RUDP, () => { Debug.Log("Connected!"); });

            _AccountCallBack = new AccountCallBackHandler(11);
            connecter.AddCallBackHandler(11, _AccountCallBack);

            _GroupCallBackHandler = new GroupCallBackHandler(12);
            connecter.AddCallBackHandler(12, _GroupCallBackHandler);

            _GroupCallBackHandler.BroadcastAction += o => { Debug.Log("receive broadcast msg : " + o); };
        }
    }

    public class AccountCallBackHandler : CallBackHandler
    {
        private const int HttpMaxLangth = 2000;
        private Queue<Action<string>> GetActions, SetActions;
        private byte OperatorCode;
        public AccountCallBackHandler(byte operatorCode)
        {
            this.OperatorCode = operatorCode;
            GetActions = new Queue<Action<string>>();
            SetActions = new Queue<Action<string>>();
        }

        public void Get(string key, Action<string> callback)
        {
            if (key.Length > HttpMaxLangth)
                throw new Exception("key 長度不能超過 2000 現在的長度是 : " + key.Length);
            gameService.Deliver(OperatorCode, new Dictionary<byte, object>()
            {
                {0,"Get"},
                {1,key}
            });
            GetActions.Enqueue(callback);
        }
        public void Set(string key, string value, Action<string> callback)
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
            SetActions.Enqueue(callback);
        }
        public override void ServerCallBack(Dictionary<byte, object> server_packet)
        {
            string code = server_packet[0].ToString();
            string response = server_packet[1].ToString();
            switch (code)
            {
                case "Get":
                    if (GetActions.Count > 0)
                    {
                        Action<string> callback = GetActions.Dequeue();
                        callback(response);
                    }
                    break;
                case "Set":
                    if (SetActions.Count > 0)
                    {
                        Action<string> callback = SetActions.Dequeue();
                        callback(response);
                    }
                    break;
                default:
                    Debug.Log("AccountCallBackHandler.ServerCallBack code u=is wrong.");
                    break;
            }
        }
    }
    public class GroupCallBackHandler : CallBackHandler
    {
        private Queue<Action<string[]>> GetListActions;
        public Action<string> BroadcastAction;
        private byte OperatorCode;
        public GroupCallBackHandler(byte operatorCode)
        {
            this.OperatorCode = operatorCode;
            GetListActions = new Queue<Action<string[]>>();
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
        public void GetList(Action<string[]> callback)
        {
            gameService.Deliver(OperatorCode, new Dictionary<byte, object>()
            {
                {0,"GetList"}
            });
            GetListActions.Enqueue(callback);
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
                    if (GetListActions.Count > 0)
                    {
                        object[] resGetList = (object[])server_packet[1];
                        List<string> s = new List<string>();
                        foreach (object o in resGetList)
                        {
                            s.Add(o.ToString());
                        }
                        Action<string[]> callback = GetListActions.Dequeue();
                        callback(s.ToArray());
                    }
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
}