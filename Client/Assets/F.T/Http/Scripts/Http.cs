using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace FTServer.Http
{
    public class Http
    {
        public static Http Instance { get { return (instance != null) ? instance : instance = new Http(); } }
        private static Http instance;
        private Http()
        {
            Process = new Dictionary<UnityWebRequest, Action<string>>();
        }

        private Dictionary<UnityWebRequest, Action<string>> Process;
        /// <summary>
        /// Http Get
        /// </summary>
        /// <param name="url"> url 路徑 ex : https://localhost:80/file or https://www.google.com.tw/ </param>
        /// <param name="packet"> 要傳送的資料 </param>
        /// <param name="callBack"></param>
        public void Get(string url, Packet packet,Action<string> callBack)
        {
            url = string.Concat(url, packet.GetQueryString());

            UnityWebRequest request = UnityWebRequest.Get(url);
            Process.Add(request, callBack); // callback 加入列隊
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            asyncOperation.completed += CallBackHandler;
        }

        private void CallBackHandler(UnityEngine.AsyncOperation obj)
        {
            UnityWebRequest webRequest = ((UnityWebRequestAsyncOperation)obj).webRequest;
            Process[webRequest](webRequest.downloadHandler.text);
            Process.Remove(webRequest);
            webRequest.Dispose();
        }
    }
}