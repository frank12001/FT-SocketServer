# FT Socket-Server 0.0.2 indev
Socket Server for .NET Framework 4.5, Mono, .NET Core 2.0, .NET Standard 2.0 with Unity Client Libary.  
We recommend use docker or k8s deploy server.

## Build

### [NuGet](https://www.nuget.org/packages/FTServer/) [![NuGet](https://img.shields.io/nuget/v/FTServer.svg)](https://www.nuget.org/packages/FTServer/) 

### [Release builds](https://github.com/frank12001/Socket-Server/releases) [![GitHub (pre-)release](https://img.shields.io/github/release/frank12001/Socket-Server/all.svg)](https://github.com/frank12001/Socket-Server/releases)
## Features
* UDP, TCP, RUDP(LitNetLib), Websocket(Websocketsharp) support
* Ipv6 support
* Docker support
* Unity support
  * Android
  * iOS
  * WebGL (websocket)

## Usage samples
* Change PlayerSetting First!!!  File/Build Setting/PlayerSetting
  * PlayerSetting/OtherSettings/Configuration/ScriptRuntimeVersion -> .Net 4.x Equivalent  
  * PlayerSetting/OtherSettings/Configuration/ScriptingBackend     -> IL2CPP
### Client
```csharp
public class NewBehaviourScript : MonoBehaviour {
    private Connect mConnect;
    private MyCallBackHandler MyCallBackHandler;
    // Use this for initialization
    void Start () {
        //create connection
        mConnect = new Connect("104.199.194.170"/*Server Ip*/, 30100/*port*/, NetworkProtocol.RUDP);
        //establish connection
        mConnect._system.ConnectToServer();
        mConnect._system.Connect += ()=> { Debug.Log("Connect to Server Success."); };
        //create logic object
        MyCallBackHandler = new MyCallBackHandler();    
        //add this logic object to connection object
        mConnect.AddCallBackHandler(MyCallBackHandler.OperatorCode/*if server send packet which code is 20. this obj is going to handler it.*/, MyCallBackHandler);
    }
    // Update is called once per frame
    void Update () {
        //call every frame
        mConnect.Service();
        if (Input.anyKeyDown)
            MyCallBackHandler.Send("hellow world!!");
    }
}
public class MyCallBackHandler : CallBackHandler
{
    public const int OperatorCode = 20;
    public void Send(string packet)
    {
        //send packet to server
        gameService.Deliver(MyCallBackHandler.OperatorCode, new Dictionary<byte, object>(){ {0,packet }});
    }
    public override void ServerCallBack(Dictionary<byte, object> server_packet)
    {
        //get something from server
        Debug.Log("Msg from server : " + server_packet[0].ToString());     
    }
}

```
### Server
```csharp
    class Program : SocketServer
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.StartListen(30100, Protocol.RUDP);
            while (true){Thread.Sleep(500);}
        }
        public override ClientNode GetPeer(Core core, IPEndPoint iPEndPoint, SocketServer socketServer)
        {
            Peer player = new Peer(core, iPEndPoint, socketServer);
            return player;
        }
    }
    public class Peer : PeerBase
    {
        public Peer(ISender sender, IPEndPoint iPEndPoint, FTServer.SocketServer socketServer) : base(sender, iPEndPoint, socketServer, 10000)
        {}
        public override void OnOperationRequest(IPacket packet)
        {
            if (packet.OperationCode == 20)
            {
                Console.WriteLine("Client tell me : " + packet.Parameters[0].ToString());
                SendEvent(packet.OperationCode, new System.Collections.Generic.Dictionary<byte, object>()
                {
                    {0,"hello client!" }
                });
            }
        }
        public override void OnDisconnect()
        {
            Console.WriteLine("OnDisconnect");
        }
    }
```

## License ##

FTServer is provided under [The MIT License].
