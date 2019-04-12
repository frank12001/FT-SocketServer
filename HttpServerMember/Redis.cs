using System;
using System.Net;
using System.Collections.Generic;
using CSRedis;

namespace HttpServerMember
{
    public class Redis
    {
        private static Redis _Instance;
        public static Redis Instance { get
            {
                if (_Instance == null)
                    _Instance = new Redis();
                return _Instance;
            }
        }

        private RedisClient _MClient;
        private RedisClient MClient {
            get {
                CheckConnection();
                return _MClient;
            }
            set
            {
                _MClient = value;
            }
        }

        public Redis()
        {
            //MClient = new RedisClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6379));
            string ping = MClient.Ping();
            Console.WriteLine($@"Ping host. Response : {ping}");
        }

        private void CheckConnection()
        {
            if (_MClient == null)
            {
                _MClient = ConnectToRedis();
                return;
            }
            lock (_MClient)
            {
                if (!_MClient.IsConnected)
                    _MClient = ConnectToRedis();
            }
        }

        private RedisClient ConnectToRedis()
        {
            return new RedisClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6379));
        }

        public string Ping()
        {
            return MClient.Ping();
        }

        public void Replace(string key,string account)
        {
            lock (MClient)
            {
                MClient.Set(key, account);
            }
        }
        public void Delete(string key)
        {
            lock (MClient)
            {
                MClient.Del(key);
            }
        }

        public long TotalNum()
        {
            lock (MClient)
            {
                return MClient.Keys("*").LongLength;
            }
        }
        public string Find(string key)
        {
            bool b = false;
            string account = "";
            lock (MClient)
            {
                b = MClient.Exists(key);
                account = MClient.Get(key);
            }
            return account;
        }

        public List<string> FindAll()
        {
            List<string> result = new List<string>();
            try
            {
                string[] keys;
                string[] values;
                lock (MClient)
                {
                    keys = MClient.Keys("*");
                    values = MClient.MGet(keys);
                }
                foreach (string value in values)
                {
                    result.Add(value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return result;
        }
    }
}
