using System.Collections.Generic;

namespace WebUtility
{
    public class Packet
    {
        private Dictionary<string,string> Values;

        public Packet()
        {
            Values = new Dictionary<string, string>();
        }

        public void Add(string key, string value)
        {
            Values.Add(key, value);
        }

        public void Remove(string key)
        {
            Values.Remove(key);
        }

        public void Clear()
        {
            Values.Clear();
        }

        public string GetQueryString()
        {
            string queryString = "";
            foreach (KeyValuePair<string, string> item in Values)
            {
                queryString += string.Format("{0}={1}&", item.Key, item.Value);
            }
           
            if (!queryString.Length.Equals(0)) 
            {
                //如果有插入值的話
                //移除最後一個 & 符號
                int lastIndex = queryString.LastIndexOf("&");
                queryString.Remove(lastIndex);

                //將問號插到開頭
                queryString = "?" + queryString;
            }

            return queryString;
        }
    }
}