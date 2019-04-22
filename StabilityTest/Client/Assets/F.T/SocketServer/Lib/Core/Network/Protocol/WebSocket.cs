using System;
using System.Net;
using BestHTTP.WebSocket;
using UnityEngine;

namespace FTServer
{
    public class WebSocketLis : INetwork
    {
        private WebSocket mWebSocket;

        public WebSocketLis() : base(NetworkProtocol.WebSocket)
        { }

        public override void Connect(Uri uri)
        {
            IPAddress addr = IPAddress.Parse(uri.Host);
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (IPTool.IOSCheck(addr, out addr))
                {
                    string ipv6UriString = string.Format("ws://[{0}]:{1}/WebSocket", addr.ToString(), uri.Port);
                    uri = new Uri(ipv6UriString);
                }
            }

            mWebSocket = new WebSocket(uri);

            mWebSocket.StartPingThread = true;
            mWebSocket.OnOpen += webSocket =>
            {
                UnityEngine.Debug.Log("WebSocket isOpen= " + webSocket.IsOpen);
                if (webSocket.IsOpen)
                    onCompleteConnect(null);
            };
            mWebSocket.OnBinary += (webSocket, message) =>
            {
                fireCompleteReadFromServerStream(message);
                //UnityEngine.Debug.Log("Binary Message received from server. Length: " + message.Length);
            };
            mWebSocket.OnError += (ws, ex) =>
            {
                string errorMsg = string.Empty;
                fireCompleteDisconnect();
                mWebSocket.Close();
                UnityEngine.Debug.Log("An error occured: " + (ex != null ? ex.Message : "Unknown: " + errorMsg));
                System.Console.WriteLine("MYLOG = Error:" + ex.Message);
            };
            mWebSocket.OnClosed += (webSocket, code, message) =>
            {
                fireCompleteDisconnect();
                UnityEngine.Debug.Log("WebSocket Closed!");
                System.Console.WriteLine("MYLOG = Client Close");                
            };
            mWebSocket.Open();
        }

        public override void BeginSend(byte[] datagram, int bytes)
        {
            mWebSocket.Send(datagram);
            fireCompleteSend();
        }

        public override void DisConnect()
        {           
            mWebSocket.Close();
            fireCompleteDisconnect();
            Debug.Log("Close");
        }

        protected override void onCompleteConnect(IAsyncResult iar)
        {
            try
            {
                fireCompleteConnect();
            }
            catch (Exception exc)
            {
                fireCompleteDisconnect();
                Console.WriteLine(exc.Message);
            }
        }
    }
}