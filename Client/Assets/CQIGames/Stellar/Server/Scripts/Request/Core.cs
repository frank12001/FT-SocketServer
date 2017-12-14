using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Networking;

namespace Stellar.Server
{
    public class Core : HttpProtocol
    {
        private static List<string> userIds = new List<string>();

        public static string GetUserId()
        {
            if (!userIds.Count.Equals(0))
            {
                return userIds[0];
            }
            else
            {
                return "-L07-epS6L6ApPWZcojd";
            }            
            //return "213413121";
        }

        //寫一個 UID 管理器
        public Core() : base()
        { }

        protected void AddUserid(string userid)
        {
            userIds.Add(userid);
        }

        protected string GetUserid(byte? index=null)
        {
            if (userIds.Count <= 0)
                return "";
            if (index != null)
                return userIds[(byte)index];
            else
                return userIds[userIds.Count-1];
        }

        protected void ClearUserid()
        {
            userIds.Clear();
        }
        #region 需擴充功能
        /// <summary>
        /// 在這層作解包
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private string[] _Unpacking(string response)
        {
            string[] result = null;
            //---解密實作
            //---
            string[] delimiterChars = { "<p>" };
            result = response.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            return result;
        }
        #endregion
        ///// <summary>
        ///// send web request
        ///// </summary>
        //protected void Send(string queryString,TypeBase typeBase,params object[] callback)
        //{
        //    List<object> list = new List<object>(callback);
        //    list.Insert(0, typeBase); callback = list.ToArray();
        //    base.Send(queryString, HttpResponse,callback);
        //    Debug.Log(String.Format("url : {0}",queryString));
        //}
        #region 統一回傳處理

        //private void HttpResponse(UnityWebRequest webRequest, params object[] callback)
        //{
        //    TypeBase reqType = (TypeBase)callback[0];
        //    var html = webRequest.downloadHandler.text;
        //    string[] unpacket_response = _Unpacking(html);

        //    string log = "Response : ";
        //    for (byte i = 0; i < unpacket_response.Length; i++)
        //        log += String.Format("[ {0} : {1} ] , ", i, unpacket_response[i]);
        //    Debug.Log(log);
        //    HandlerResponse(reqType, unpacket_response, callback);
        //}
        //protected virtual void HandlerResponse(TypeBase reqType, string[] responses, params object[] callback)
        //{
        //    using (reqType)
        //    {
        //        reqType.ResponseHandler(responses, callback);
        //    }
        //}
        #endregion
    }
}