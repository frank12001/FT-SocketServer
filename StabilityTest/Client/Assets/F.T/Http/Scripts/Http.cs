using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace WebUtility
{
    public static class HTTP
    {
        private static readonly Dictionary<UnityWebRequest, Action<string>> Requests = new Dictionary<UnityWebRequest, Action<string>>();

        public static void Get(HttpURL url, Action<string> callback, int timeoutInSeconds = 10, string token = "")
        {
            var request = UnityWebRequest.Get(url.GetString());
            request.timeout = timeoutInSeconds;
            if (!string.IsNullOrEmpty(token))
                request.SetRequestHeader(@"Authorization", token);
            Requests.Add(request, callback);
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            asyncOperation.completed += onResponse;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <param name="callback"> bool isError, string responseText. </param>
        /// <param name="timeoutInSeconds"></param>
        public static void Post(string url, string json, Action<string> callback, int timeoutInSeconds = 10, string token="")
        {

                // https://forum.unity.com/threads/posting-raw-json-into-unitywebrequest.397871/
                var request = new UnityWebRequest(url);
                request.method = UnityWebRequest.kHttpVerbPOST;
                request.chunkedTransfer = false;
                request.timeout = timeoutInSeconds;
            if (!string.IsNullOrEmpty(token))
                request.SetRequestHeader(@"Authorization", token);
            if (!String.IsNullOrEmpty(json))
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                    request.uploadHandler.contentType = "application/json";
                }
                request.downloadHandler = new DownloadHandlerBuffer();
                Requests.Add(request, callback);
                UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
                asyncOperation.completed += onResponse;
        }

        private static void onResponse(AsyncOperation obj)
        {
            var request = ((UnityWebRequestAsyncOperation)obj).webRequest;

            var responseText = request.downloadHandler.text;
            //if (request.isNetworkError)
            //    responseText = Response.ErrorMsg($"NetworkError. {request.error}, {request.downloadHandler.text}");
            //else if (request.isHttpError)
            //    responseText = Response.ErrorMsg($"HttpError. Code:{request.responseCode}, {request.error}, {request.downloadHandler.text}");

            Requests[request](responseText);

            Requests.Remove(request);
            request.Dispose();
        }
    }
}