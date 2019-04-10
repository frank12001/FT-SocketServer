using System.Net;
using UnityEngine;
using FTServer;
using FTServer.Example;

namespace ChatRoom
{
    public class Main : MonoBehaviour
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
}