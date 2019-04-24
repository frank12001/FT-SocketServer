using System.Collections.Generic;
using UnityEngine;
using FTServer;
using FTServer.Operator;
using System.Timers;
using System;
using FTServer.Example;
using FTServer.Math;
using GG;

public class NewBehaviourScript : MonoBehaviour
{
    private Connect mConnect;
    private MyCallBackHandler MyCallBackHandler;
    private StabilityTest StabilityTestHandler;
    private _System SystemHandler;
    // Use this for initialization
    void Start()
    {
        //create connection
        mConnect = new Connect("192.168.2.5"/*Server Ip*/, 30100/*port*/, NetworkProtocol.WebSocket);
        //establish connection
        //mConnect._system.ConnectToServer();
        //mConnect._system.Connect += () => { Debug.Log("Connect to Server Success."); };

        SystemHandler = new _System();
        mConnect.AddCallBackHandler(_System.OperatorCode, SystemHandler);

        //create logic object
        MyCallBackHandler = new MyCallBackHandler();
        //add this logic object to connection object
        mConnect.AddCallBackHandler(MyCallBackHandler.OperatorCode/*if server send packet which code is 20. this obj is going to handler it.*/, MyCallBackHandler);

        StabilityTestHandler = new StabilityTest();
        mConnect.AddCallBackHandler(StabilityTest.OperatorCode, StabilityTestHandler);

        SystemHandler.ConnectToServer();
    }
    // Update is called once per frame
    void Update()
    {
        //call every frame
        mConnect.Service();
        if (Input.anyKeyDown)
        {
            if (!SystemHandler.IsConnect)
                SystemHandler.ConnectToServer();
            MyCallBackHandler.Send("hellow world!!");
            //Serialize.ToObject( Serialize.ToByteArray(new Dictionary<byte, object>()
            //{
            //    {0,"123" },
            //}
            //));
            //Debug.Log(s);
        }
    }
    void OnApplicationQuit()
    {
        mConnect.Dispose();
    }
}
namespace GG
{
    [Serializable]
    public class G
    {
        public string s;
    }
}

public class StabilityTest : CallBackHandler
{
    public const int OperatorCode = 21;

    protected override void AfterAddService()
    {
        gameService.ConnectFromServer += GameService_ConnectFromServer;
        gameService.DisconnectFromServer += GameService_DisconnectFromServer;
    }

    private void GameService_ConnectFromServer()
    {
        Debug.Log(" StabilityTest : ConnectFromServer");
    }

    private void GameService_DisconnectFromServer()
    {
        Debug.Log(" StabilityTest : DisconnectFromServer");
    }

    public override void ServerCallBack(Dictionary<byte, object> server_packet)
    {
        //throw new System.NotImplementedException();
    }
}

public class MyCallBackHandler : CallBackHandler
{
    public const int OperatorCode = 20;
    public void Send(string packet)
    {
        //send packet to server
        gameService.Deliver(MyCallBackHandler.OperatorCode, new Dictionary<byte, object>() { { 0, Serialize.ToByteArray(new G() { s = packet }) } });
    }
    public override void ServerCallBack(Dictionary<byte, object> server_packet)
    {
        //get something from server
        //Debug.Log("Msg from server : " + server_packet[0].ToString());
        Debug.Log("Msg from server : " + ((G)Serialize.ToObject((byte[])server_packet[0])).s);
    }
}