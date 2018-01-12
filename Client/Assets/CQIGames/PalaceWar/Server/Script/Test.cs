using System;
using System.Collections;
using System.Collections.Generic;
using FTServer;
using FTServer.Operator;
using PalaceWar.Server;
using TCPServer.Projects.Palace.Packet;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private Connect connect;
    private TestUIController uiController;
    private float ping = 0;
    private AsyncOperation sceneAsyncOperation;

    // Use this for initialization
    void Start ()
	{
        DontDestroyOnLoad(gameObject);
        Application.runInBackground = true;
        connect = GetComponent<Connect>();
	    uiController = FindObjectOfType<TestUIController>();

	    sceneAsyncOperation = SceneManager.LoadSceneAsync("Map1");
	    sceneAsyncOperation.allowSceneActivation = false;


        connect._system.Connect += () =>
	    {
	        uiController.CanQueue();
        };

        //connect._room.ReceiveCreateRoom += value =>
        //{
        //    Debug.Log(value.Length);
        //    if (value.Length.Equals(0))
        //    {
        //        connect._room.JoinRoom("Test");
        //       }
        //};

        connect._room.ReceiveJoinRoom += (b, bb) =>
	    {
	        Debug.Log(b +","+bb);
	    };

	    connect._queue.ReceiveExitQueue += b =>
	    {
	        Debug.Log("取消排隊 = "+b);
	    };

	    connect._queue.ReceiveJoinQueue += (b, o) =>
	    {
	        Debug.Log("加入排隊回傳 = "+b);
	    };


        connect._gaming.ReceiveServerPacket += o =>
	    {
	        if (o is LoadingNextScene)
	        {
	            LoadingNextScene packet = (LoadingNextScene) o;
                //Debug.Log(" 名子 : "+packet.PlayersName[0]+" , "+packet.PlayersName[1]);
                Debug.Log("LoadingNextScene");

                //過場演出時間
	            float ShowTime = 3;
                Invoke("NextScene",ShowTime);
                hasReady = false;

	        }
	    };
        Invoke("Connect",1);

	    //Invoke("ConpletePing", 1);
    }

    private void NextScene()
    {
        sceneAsyncOperation.allowSceneActivation = true;
    }

    private void Connect()
    {
        if (!connect.IsConnect)
        {
            connect._system.ConnectToServer();
            Invoke("Connect", 10);
        }
    }

    //private void ConpletePing()
    //{
    //    if(connect.IsConnect)
    //        connect._system.ComputePing(new object());
    //    Invoke("ConpletePing", 1);
    //}

    // Update is called once per frame
	void Update () {

     //   if (!connect._system.Ping.Equals(0))
	    //{
	    //    if (ping.Equals(0))
	    //        ping = connect._system.Ping;
	    //    else if (!ping.Equals(connect._system.Ping) && !ping.Equals(0))
	    //    {
     //           Debug.Log(string.Format("In ping = {0},connect ping = {1}", ping, connect._system.Ping));
	    //        ping = ((ping + connect._system.Ping) / 2);
	    //    }
	    //}

	    if (uiController != null)
	    {
	        if (connect.IsConnect)
	            uiController.QueueButton_SetText("進入戰場");
	        else
	            uiController.QueueButton_SetText("連線中");
	    }

        if (Input.anyKeyDown && !hasReady)
        {
            GetComponent<PalaceServerConnecter>().LoadingReady();

            hasReady = true;

        }

    }

    private bool hasReady = true;

    public void Ready()
    {
        connect._gaming.SendToServer(new Dictionary<byte, object>()
        {
            {(byte)0,1 },
        });
    }

    public void Queue()
    {
        if (connect.IsConnect)
        {
            connect._queue.JoinQueue(new QueueInfo() {Key = "palaceTest1",CustomName = uiController.GetName() });
        }
    }
}
