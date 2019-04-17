using System.Collections.Generic;
using UnityEngine;
using FTServer;
using FTServer.Operator;

public class NewBehaviourScript : MonoBehaviour
{
    private Connect mConnect;
    private MyCallBackHandler MyCallBackHandler;
    // Use this for initialization
    void Start()
    {
        //create connection
        mConnect = new Connect("192.168.2.5"/*Server Ip*/, 30100/*port*/, NetworkProtocol.RUDP);
        //establish connection
        mConnect._system.ConnectToServer();
        mConnect._system.Connect += () => { Debug.Log("Connect to Server Success."); };
        //create logic object
        MyCallBackHandler = new MyCallBackHandler();
        //add this logic object to connection object
        mConnect.AddCallBackHandler(MyCallBackHandler.OperatorCode/*if server send packet which code is 20. this obj is going to handler it.*/, MyCallBackHandler);
    }
    // Update is called once per frame
    void Update()
    {
        //call every frame
        mConnect.Service();
        if (Input.anyKeyDown)
            MyCallBackHandler.Send("hellow world!!");
    }
}
public class StabilityTest : CallBackHandler
{
    public const int OperatorCode = 21;
    public StabilityTest()
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
        gameService.Deliver(MyCallBackHandler.OperatorCode, new Dictionary<byte, object>() { { 0, packet } });
    }
    public override void ServerCallBack(Dictionary<byte, object> server_packet)
    {
        //get something from server
        Debug.Log("Msg from server : " + server_packet[0].ToString());
    }
}
