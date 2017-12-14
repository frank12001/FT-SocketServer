using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Stellar.Server
{
    public abstract class HttpProtocol: IDisposable
    {
        private const string _ServerIp = "https://us-central1-stellar-38931.cloudfunctions.net/"; //"https://us-central1-forblog-a63d0.cloudfunctions.net/";
        /// <summary>
        /// 使用此 MonoBehaivour 等待 Web Request 回傳
        /// </summary>
        protected MonoBehaviour coroutineController;

        /// <summary>
        /// 自動生成 CoroutineController
        /// </summary>
        protected HttpProtocol()
        {
            CreateCoroutineController();
        }

        public virtual void Dispose()
        {
            //if (coroutineController == null)
            //    return;
            //if (!Application.isPlaying)
            //    UnityEngine.Object.DestroyImmediate(coroutineController.gameObject);
            //else
            //    UnityEngine.Object.Destroy(coroutineController.gameObject);
        }
    #region 收、發 功能

    /// <summary>
    /// send web request
    /// </summary>
    /// <param name="url"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    protected void Send(string queryString, Action<UnityWebRequest, object[]> responce, params object[] callback)
        {
            CreateCoroutineController();
            callback = callback.Where(c => c != null).ToArray(); //清掉 null 的 value
            this.coroutineController.StartCoroutine(Request(queryString, responce,callback));
        }
        protected void Send(string queryStyring)
        {
            CreateCoroutineController();
            Debug.Log(queryStyring);
            this.coroutineController.StartCoroutine(Request(queryStyring));
        }
        protected void SendError(Error error)
        {
            CreateCoroutineController();
            string queryString = String.Format("Report?parameter1=Error&" +
                                                "parameter2={0}&parameter3={1}&parameter4={2}&parameter5={3}&parameter6={4}",
                                                error.Code,error.Msg, error.time.ToString("yyyy-MM-ddTHH:mm:ss"), error.UserId,error.Platform);
            this.coroutineController.StartCoroutine(Request(queryString));
           
        }
        private void CreateCoroutineController()
        {
            if (this.coroutineController == null)
            {
                coroutineController = new CoroutineController().Get();
                ////初始化協成控制器
                //GameObject gameObject = new GameObject
                //{
                //    name = "_FirebaseCoroutineController_"
                //};
                //this.coroutineController = gameObject.AddComponent<FirebaseCoroutineController>();
            }
        }
        /// <summary>
        /// 接收回傳模板
        /// </summary>
        /// <param name="url"></param>
        /// <param name="success"></param>
        /// <param name="fail"></param>
        /// <returns></returns>
        private IEnumerator Request(string queryString, Action<UnityWebRequest, object[]> responce, params object[] callback)
        {
            string url = String.Concat(_ServerIp, queryString);
            new Uri(url);
            //if (!uri.IsWellFormedOriginalString())
            //{ Debug.LogError(String.Format("Uri 字串格式不正確。錯誤的 uri 為 : {0} ",uri.ToString())); responce(null, callback); yield break; }

            var request = UnityWebRequest.Get(url);

            yield return request.Send();

            if (request.isNetworkError)
            {
                Debug.LogError(request.error);
                responce(request, callback);
                yield break;
            }
            responce(request, callback);
        }

        private IEnumerator Request(string queryString)
        {
            string url = String.Concat(_ServerIp, queryString);
            //Uri uri = new Uri(url);
            //if (!uri.IsWellFormedOriginalString())
            //{ Debug.LogError(String.Format("Uri 字串格式不正確。錯誤的 uri 為 : {0} ", uri.ToString())); yield break; }

            var request = UnityWebRequest.Get(url);

            yield return request.Send();

            if (request.isNetworkError)
            {
                Debug.LogError(request.error);
                yield break;
            }
        }
        #endregion
    }
}