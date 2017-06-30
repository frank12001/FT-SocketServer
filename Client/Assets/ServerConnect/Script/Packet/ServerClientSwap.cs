using System;
namespace TVEducation.ServerClientSwap
//20161104 PM4:50版
//進出Menu呼叫

{
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
        Dino = 0,
        Animal = 1,
        Vehide = 2,
        Garden = 3,
        Bird = 4,
        Fruit = 5,
    }
}