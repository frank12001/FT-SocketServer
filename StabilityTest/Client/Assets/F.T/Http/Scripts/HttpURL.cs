using System.Collections.Generic;

namespace WebUtility
{
    public class HttpURL
    {
        public string OriginUrl { get { return mOriginUrl; } }
        private readonly string mOriginUrl;
        private readonly Dictionary<string,string> mParameters = new Dictionary<string, string>();

        public HttpURL(string url) { mOriginUrl = url; }

        public void Add(string key, string value)
        {
            mParameters.Add(key, value);
        }

        public void Remove(string key)
        {
            mParameters.Remove(key);
        }

        public void Clear()
        {
            mParameters.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> format: ?[Key1]=[Value1]&[Key2]=[Value2], for example: ?ID=125&Num=3 </returns>
        public string GetString()
        {
            var parameters = string.Empty;
            foreach(var pair in mParameters)
            {
                parameters += string.Format("{0}={1}&", pair.Key, pair.Value);
            }
           
            if(parameters.Length > 0) 
            {
                //如果有插入值的話, 移除最後一個 & 符號.
                parameters.Remove(parameters.Length - 1);

                //將問號插到開頭
                parameters = "?" + parameters;
            }

            return mOriginUrl + parameters;
        }
    }
}