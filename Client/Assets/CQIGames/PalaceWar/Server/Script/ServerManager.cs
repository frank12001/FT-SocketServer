using System.Collections;
using System.Collections.Generic;
using FTServer;
using PalaceWar.Server;
using TCPServer.Projects.Palace.Packet;
using UnityEngine;

public class ServerManager : MonoBehaviour
{

    public GameObject Connecter;
    private Connect connect;
    private TestUIController uiController;
    // Use this for initialization
    void Start ()
	{
	    Screen.orientation = ScreenOrientation.LandscapeLeft;
        connect = GameObject.FindObjectOfType<Connect>();

        if (connect != null)
	    {
	        Destroy(connect.gameObject);
	    }
	    connect = Instantiate(Connecter).GetComponent<Connect>();
        uiController = FindObjectOfType<TestUIController>();
    }

    public void Queue()
    {
        if (connect.IsConnect)
        {
            connect._queue.JoinQueue(new QueueInfo() { Key = "palaceTest1", CustomName = uiController.GetName() });
        }
    }
}
