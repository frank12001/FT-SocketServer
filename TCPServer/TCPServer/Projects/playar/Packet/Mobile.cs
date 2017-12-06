using System;
namespace TVEducation.ServerClientSwap
//20161115 PM5:35版
//進出Menu呼叫

{
    //以下Fighting1加入功能
    [Serializable]
    public class GetRoomPlayerQuantity
    {

    }

    [Serializable]
    public class AddObjectDataTransmission
    {
        public Category _Category;
        public byte ID_1, ID_2, PlayarID;
        public AddObjectDataTransmission(Category category, byte id_1, byte id_2)
        {
            this._Category = category;
            this.ID_1 = id_1;
            this.ID_2 = id_2;
        }
    }

    [Serializable]
    public class Fighting1_TopicDataGet : StageCode
    {
        public int[] ServerGiveData;
        public Fighting1_TopicDataGet(int[] i, byte id)
        {
            this.ServerGiveData = i;
            this.id = id;
        }
    }

    [Serializable]
    public class InOrOutMenu
    {
        public InOrOutMenu()
        {

        }
    }

    [Serializable]
    public class Fighting1_SetTopicAnswer
    {

        public int PlayerAnswer;
        public Fighting1_SetTopicAnswer(int Answerbyte)
        {
            this.PlayerAnswer = Answerbyte;
        }
    }

    [Serializable]
    public class Fighting1_GameOverGet : StageCode
    {
        public bool ServerGiveData;
        public Fighting1_GameOverGet(bool b, byte id)
        {
            this.ServerGiveData = b;
            this.id = id;
        }
    }

    [Serializable]
    public class StageCode
    {
        public byte id;
    }

    [Serializable]
    public enum Category : byte
    {
        Animal = 1,
        Vehide = 2,
        Garden = 3,
        Bird = 4,
        Fruit = 5,
        Dino = 6,
    }

    //以下KFBTV加入功能
    [Serializable]
    public class KFBTVPronunciation
    {
        public KFBTVPronunciationCategory _KFBTVPronunciationCategory;
        public byte ID_1;
        public KFBTVPronunciation(KFBTVPronunciationCategory KFBTVPronunciationcategory, byte id_1)
        {
            //KFBTVPronunciationcategory值 1=撥放名稱 2撥放導讀
            this._KFBTVPronunciationCategory = KFBTVPronunciationcategory;
            //_KFBTVPronunciationCategory為1時 1中文 2澳語 3英文 4西班牙 5俄羅斯
            //_KFBTVPronunciationCategory為2時 1中文 2英文
            this.ID_1 = id_1;
        }
    }

    [Serializable]
    public enum KFBTVPronunciationCategory : byte
    {
        Name = 1,
        Introduction = 2,
    }

    [Serializable]
    public class KFBTVPlayAction
    {
        public KFBTVPlayAction()
        {

        }
    }
}