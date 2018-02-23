using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FTServer.Firebase
{
    public class DataBase
    {
        /// <summary>
        /// 將指定的資料拉出來。 記得設定 Firebase Safe Rule
        /// </summary>
        /// <param name="firebaseURL"> 該專案的 firebase URL EX : "https://pifairies-5e51c.firebaseio.com/" </param>
        /// <param name="jsonPath">要取得資料的路徑 EX : GlobalFairy</param>
        /// <returns>Request 字串回傳</returns>
        public static string Get(string firebaseURL,string jsonPath)
        {
            string url = string.Format("{0}{1}.json", firebaseURL,jsonPath);
            return FTServer.Http.Editor.Http.RequestGET(url);
        }

        /// <summary>
        /// 將指定的資料寫入指定位置。 記得設定 Firebase Safe Rule
        /// </summary>
        /// <param name="firebaseURL">該專案的 firebase URL EX : "https://pifairies-5e51c.firebaseio.com/"</param>
        /// <param name="jsonPath">要取得資料的路徑 EX : GlobalFairy</param>
        /// <param name="json">要設定進去的資料</param>
        public static void Set(string firebaseURL, string jsonPath, string json)
        {
            string url = string.Format("{0}{1}/{2}.json", firebaseURL, jsonPath,json);
            FTServer.Http.Editor.Http.ResquestPATCH(url, json);
        }
    }
}