using System.Net;
using UnityEngine;

namespace FTServer.Example
{
    public class Main : MonoBehaviour
    {
        FTServerConnecter connecter;
        MyCallBackHandler myCallBack;
        // Use this for initialization
        void Start()
        {
            connecter = GetComponent<FTServerConnecter>();
            connecter.InitAndConnect(new IPEndPoint(IPAddress.Parse("35.229.223.110"), 30100),NetworkProtocol.RUDP,()=> { Debug.Log("Connected!"); });

            myCallBack = new MyCallBackHandler();
            connecter.AddCallBackHandler(10,myCallBack);

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                myCallBack.Send();
            }
            if(connecter.IsConnect)
               myCallBack.Send();

        }
    }
}