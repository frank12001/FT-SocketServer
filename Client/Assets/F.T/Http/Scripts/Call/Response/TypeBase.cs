using System;

namespace FTServer.Http
{
    public class TypeBase : Core 
    {
        //處理 Https 回傳的基礎類別
        public virtual void ResponseHandler(string[] responses, params object[] callback)
        {
            //範例
            //string s = JsonConvert.SerializeObject(object1);
            //class1 c = JsonConvert.DeserializeObject<class1>(string1);
            //Action<string> ok = (Action<string>)callback[1];
            //ok(responses[1]);
            //Action<FTServer.Http.Error> fail = (Action<FTServer.Http.Error>)callback[2];
            //fail(new FTServer.Http.Error());
        }

    }
}