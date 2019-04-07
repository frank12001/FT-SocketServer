using System;
using System.IO;
using System.Net;

namespace SocketServerDemo1
{
    public class Member
    {
        private const string ServerUrl = "http://localhost:60094/";
        private static void Get(string url, Action<string> @do)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";

            using (var webResponse = (HttpWebResponse)req.GetResponse())
            using (var stream = webResponse.GetResponseStream())
            using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
            {
                var html = reader.ReadToEnd();
                @do(html);
            }
        }

        public static void SetAccount(string key, string value, Action<string> @do)
        {            
            string url = $"{ServerUrl}setaccount?key={key}&value={value}";
            Get(url, @do);
        }
        public static void GetAccount(string key, Action<string> @do)
        {
            string url = $"{ServerUrl}getaccount?key={key}";
            Get(url, @do);
        }
    }
}
