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
using PalaceWar;

public class Test : MonoBehaviour
{
    private Connect connect;
    private TestUIController uiController;
    private float ping = 0;

    public GameObject Cube;
    public List<GameObject> Cubes;
    

    // Use this for initialization
	void Start ()
	{
        DontDestroyOnLoad(gameObject);
        Application.runInBackground = true;
        connect = GetComponent<Connect>();
	    uiController = FindObjectOfType<TestUIController>();

	    connect._system.Connect += () =>
	    {
	        uiController.CanQueue();
        };

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
            if (o is Cubes)
            {
                Cubes cubes = (Cubes)o;
                Debug.Log(cubes.Cube.Length);
                return;
            }
	        if (o is TCPServer.Projects.Palace.Packet.GamingStart)
	        {
                Debug.Log("GameStart");
                //SceneManager.LoadSceneAsync("Map1");
                //SceneManager.LoadSceneAsync("Map1");
                //SceneManager.LoadSceneAsync("Test2");                
                hasReady = false;
                uiController.gameObject.SetActive(false);

            }

	        if (o is GamingTest)
	        {
                Debug.Log("From Server GamingTest");
	        }

	        if (o is PalaceWar.GamingStart)
	        {
	            PalaceWar.GamingStart start = (PalaceWar.GamingStart) o;
                Debug.Log(start._Team);
	            Debug.Log(start.CardsFight);
	            Debug.Log(start.CardsCommander);
            }
	    };

        Invoke("Connect",1);

        for (int i = 0; i < 25; i++)
        {
            GameObject cube = Instantiate(Cube);
            cube.name = "This Computer";
            Cubes.Add(cube);
        }
    }

    private void Connect()
    {
        if (!connect.IsConnect)
        {
            connect._system.ConnectToServer();
            Invoke("Connect", 10);
        }
    }

    private bool StartSend = false;
    // Update is called once per frame
	void Update () {


	    if (uiController != null)
	    {
	        if (connect.IsConnect)
	            uiController.QueueButton_SetText("開始排隊");
	        else
	            uiController.QueueButton_SetText("連線中");
	    }
        if (Input.GetKeyDown(KeyCode.W))
        {
            StartSend = !StartSend;
        }
        if (StartSend)
        {
            if (time > timeMax)
            {
                Cubes cubes = new PalaceWar.Cubes() { Cube = new PalaceWar.Cube[Cubes.Count] };
                for (int i = 0; i < Cubes.Count; i++)
                {
                    Cube cube = new Cube()
                    {
                        PosX = Cubes[i].transform.position.x,
                        PosY = Cubes[i].transform.position.y,
                        PosZ = Cubes[i].transform.position.z,
                        RotX = Cubes[i].transform.eulerAngles.x,
                        RotY = Cubes[i].transform.eulerAngles.y,
                        RotZ = Cubes[i].transform.eulerAngles.z
                    };
                    cubes.Cube[i] = cube;
                }
                

                connect._gaming.SendToServer(new Dictionary<byte, object>() {
                    {(byte)0,(byte)2 },            //自訂的 class
                    {(byte)1,Serializate.ToByteArray(cubes) },
                });
                time = 0;
            }
            time += Time.deltaTime;
        }

    }
    private float time = 0.0f,timeMax = 0.3f;
    private bool hasReady = true;

    public void Sending()
    {
      StartSend = !StartSend;        
    }

    public void Ready()
    {
        connect._gaming.SendToServer(new Dictionary<byte, object>()
        {
            {(byte)0,1 },
        });
    }

    public void Queue()
    {
        if(connect.IsConnect)
            connect._queue.JoinQueue(new PalaceTest() { Key = "palaceTest1" });
    }
}
