using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpServerMember
{
    public class Index : NancyModule
    {
        public Index()
        {
            Get("/", x =>
            {
                string pingResult = Redis.Instance.Ping();
                return "Hellow World! Redis Ping result = " + pingResult;
            });

            Get("/getaccount", x =>
            {
                string account = "";
                if (Request.Query.key.HasValue)
                {
                    string key = Request.Query.key;
                    account = Redis.Instance.Find(key);
                }
                return account;
            });

            Get("/setaccount", x =>
            {
                string result = "";
                if (Request.Query.key.HasValue && Request.Query.value.HasValue)
                {
                    string key = Request.Query.key;
                    string value = Request.Query.value;
                    Redis.Instance.Replace(key, value);
                    result = key;
                }
                return result;
            });
        }
    }
}
