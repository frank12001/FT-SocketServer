using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stellar.Server
{
    /// <summary>
    /// 錯誤處理
    /// </summary>
    public class ErrorHandler
    {

        private List<Error> _Errors;
        private string userid;
        private Action<Error> send;
        public ErrorHandler(ref string userid,Action<Error> send)
        {
            this.userid = userid;
            this.send = send;
            this._Errors = new List<Error>();
        }

        /// <summary>
        /// 將 Error 傳回 Server 並記錄
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public void Set(byte code, string msg)
        {
            Error error = new Error() { Code = code, Msg = msg, time = DateTime.Now, UserId = userid, Platform = Application.platform.ToString() };
            _Errors.Add(error); //存入
            this.send(error);  //發送
        }

    }

    public struct Error
    {
        public byte Code;
        public string Msg;
        public DateTime time;
        public string UserId;
        public string Platform;
        public Error(byte code)
        {
            Code = code;
            Msg = "";
            UserId = "";
            time = DateTime.Now;
            Platform = Application.platform.ToString();
        }
    }

}