using System;
namespace TVEducation.TVBoxSwap
{
    //20161110 PM17:53版

    //新增使用者角色事件
    [Serializable]
    public class ProduceQuestion
    {
        public string strQuestion;
        public string _OC;
        public int Variable_1, Variable_2;
        public ProduceQuestion(string question, string OC, byte var1, byte var2)
        {
            this.strQuestion = question;
            this._OC = OC;
            this.Variable_1 = var1;
            this.Variable_2 = var2;
        }
    }

    //遊戲回傳時間
    [Serializable]
    public class GameTime
    {
        public string Times;
        public GameTime(byte times)
        {
            this.Times = times.ToString();
        }
    }

    //遊戲回傳結果
    [Serializable]
    public class AnnouncedResults
    {
        public byte ID_1, ID_2;
        public bool isp1win, isp2win;
        public AnnouncedResults(byte player1_id, byte player2_id, bool isP1Winner, bool isP2Winner)
        {
            this.ID_1 = player1_id;
            this.ID_2 = player2_id;
            this.isp1win = isP1Winner;
            this.isp2win = isP2Winner;
        }
    }

    //播放呼叫動畫完成回傳伺服器
    [Serializable]
    public class AddClientOBJAniFinish
    {
        public AddClientOBJAniFinish()
        {

        }
    }

    //播放結論動畫完成回傳伺服器
    [Serializable]
    public class OBJFightingAniFinish
    {
        public OBJFightingAniFinish()
        {

        }
    }

}