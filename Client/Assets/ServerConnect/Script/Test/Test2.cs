using UnityEngine;
using System.Collections;
using Playar.PhotonServer;
using TVEducation.ServerClientSwap;
using TVEducation.TVBoxSwap;
using System;
using PhotonServerConnect.Packet;

public class Test2 : MonoBehaviour {

    Connect connect;
    public Texture2D texture;
    // Use this for initialization
    void Start () {
        connect = GameObject.FindObjectOfType<Connect>();

        connect._gaming.ReceiveCustomPacket += ServerCallBack;
        connect._system.ReceiveRoomsCount += GetRoomsCount;
        connect._queue.ReceiveJoinQueue += b => { Debug.Log("Join Queue Result = " + b); };
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.W))
        {
            connect._system.ConnectToServer();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            //connect._gaming.ChangeType(Playar.PhotonServer.Operator.RoomTypes.MathGame);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //connect._room.CreateRoom("yaya", Playar.PhotonServer.Operator.RoomTypes.Base);
            connect._queue.JoinQueue();
        }
        //mobile
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddObjectDataTransmission a = new AddObjectDataTransmission(Category.Dino, 0, 0);
            connect._gaming.SendToServer(a);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Fighting1_SetTopicAnswer f = new Fighting1_SetTopicAnswer(2);
            connect._gaming.SendToServer(f);
        }
        //tv
        if (Input.GetKeyDown(KeyCode.V))
        {
            AddClientOBJAniFinish a = new AddClientOBJAniFinish();
            connect._gaming.SendToServer(a);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            OBJFightingAniFinish a = new OBJFightingAniFinish();
            connect._gaming.SendToServer(a);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            BroadcastImage(this.texture);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            connect._system.GetRoomsCount();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            connect._room.JoinRoom("Test");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            connect._gaming.BroadcastCustomPacket_EveryOne(new TestPacket(5, false));
        }
    }

    public void ConnectToServer()
    {
        connect._system.ConnectToServer();
    }

    public void JoinQueueRoom()
    {
        connect._queue.JoinQueue();
    }

    public void GetRoomsCount(byte? b)
    {
        Debug.Log("房間總數為 = " + b);
    }
    public void BroadcastImage(Texture2D texture)
    {
        //texture.EncodeToJPG();
        //texture.Apply();
        //texture.Resize(128, 128);
        //texture.Apply();
        texture.Compress(false);
        texture.Apply();
        ImagePacket packet = new ImagePacket() { height = texture.height, width = texture.width, textureFormat = (int)texture.format, texture2d = texture.GetRawTextureData() };
        connect._gaming.BroadcastCustomPacket_EveryOne(packet);
    }
    public void SendEgg()
    {
        AddObjectDataTransmission a = new AddObjectDataTransmission(Category.Dino, 0, 0);
        connect._gaming.SendToServer(a);
    }
    public void SendAnswer()
    {
        Fighting1_SetTopicAnswer f = new Fighting1_SetTopicAnswer(2);
        connect._gaming.SendToServer(f);
    }
    public void ServerCallBack(object o)
    {
        if (o is AddObjectDataTransmission)
        {
            AddObjectDataTransmission a = o as AddObjectDataTransmission;
            Debug.Log(" 收到 AddObjectDataTransmission  種類等於 = " + a._Category.ToString() + " id_1 = " + a.ID_1 + " id_2 " + a.ID_2);
        }
        if (o is Fighting1_TopicDataGet)
        {
            Fighting1_TopicDataGet data = o as Fighting1_TopicDataGet;
            Debug.Log(" 收到 Fighting1_TopicDataGet 答案 1 = " + data.ServerGiveData[0] +
                 "答案 1 = " + data.ServerGiveData[1] +
                 "答案 2 = " + data.ServerGiveData[2] +" playerId = "+data.id);
        }
        if (o is GameTime)
        {
            GameTime gameTime = o as GameTime;
            Debug.Log(" 收到 GameTime 現在倒數為 = " + gameTime.Times);
        }
        if (o is ProduceQuestion)
        {
            ProduceQuestion produce = o as ProduceQuestion;
            Debug.Log("  收到 ProduceQuestion 題目 = " + produce.strQuestion);
        }
        if (o is Fighting1_GameOverGet)
        {
            Fighting1_GameOverGet f = o as Fighting1_GameOverGet;
            Debug.Log(" 收到 Fighting1_GameOverGet 我是 = " + f.ServerGiveData);
        }
        if (o is AnnouncedResults)
        {
            AnnouncedResults a = o as AnnouncedResults;
            Debug.Log(" 收到 AnnouncedResults player1 = " + a.isp1win +
                      " player2 = " + a.isp2win);
        }
        if (o is ImagePacket)
        {
            ImagePacket i = o as ImagePacket;
            Debug.Log(" 收到 ImagePacket 高是 = " + i.height);
        }
        if (o is TestPacket)
        {
            TestPacket i = o as TestPacket;
            Debug.Log(" 收到 TestPacket i = " + i.i + "  b = " + i.b);
        }
    }
}
