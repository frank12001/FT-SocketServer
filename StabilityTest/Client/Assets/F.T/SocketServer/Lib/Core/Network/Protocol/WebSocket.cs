using System;
using System.Net;
using System.Threading;
using UnityEngine;
using BestHTTP.WebSocket;

namespace FTServer
{
    public class WebSocketLis : INetwork
    {
        private WebSocket mWebSocket;
        private CancellationToken cancelToken;

        private bool needFireDisconnect = false;
        public WebSocketLis() : base(NetworkProtocol.WebSocket)
        { }

        public override void Connect(Uri uri)
        {
            SetFireDisconnectTrigger();
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
                Debug.Log("WebSocket isOpen= " + webSocket.IsOpen);               
                if (webSocket.IsOpen)
                    onCompleteConnect(null);           
            };
            mWebSocket.OnBinary += (webSocket, message) =>
            {
                fireCompleteReadFromServerStream(message);
            };
            mWebSocket.OnError += (WebSocket webSocket, Exception ex) =>
            {
                Debug.Log("OnError");
                webSocket.Close();
                CheckAndFireDisconnect();
                //Debug.LogError(ex.Message);
            };
            mWebSocket.OnClosed += (webSocket, code, message) =>
            {
                Debug.Log("OnClose");
                CheckAndFireDisconnect();
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
            CheckAndFireDisconnect();
        }

        private void SetFireDisconnectTrigger()
        {
            needFireDisconnect = true;
        }

        private void CheckAndFireDisconnect()
        {
            if (needFireDisconnect)
            {
                needFireDisconnect = false;
                fireCompleteDisconnect();
            }
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