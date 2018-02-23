using PhotonServerConnect.Packet;
using System;
namespace CustomConnectPhotonFish
//20170314 PM03:45版 by Gary

{
    [Serializable]
    public class Fish4DCube : ServerBase
    {
        public byte PlayarID;
        public string FishID;
        public string FishType;
        public byte[] texture2d;
        public int width, height, textureFormat;

        public Fish4DCube(string fishid, string fishtype, byte userid, byte[] _texture, int _width, int _height, int _format)
        {
            this.FishType = fishtype;
            this.FishID = fishid;
            this.PlayarID = userid;
            this.texture2d = _texture;
            this.width = _width;
            this.height = _height;
            this.textureFormat = _format;
        }
    }

    [Serializable]
    public class Food4DCube : ServerBase
    {
        public int FoodCode;
        public Food4DCube(int _foodcode)
        {
            this.FoodCode = _foodcode;
        }
    }

    [Serializable]
    public class Seaman4DCube : ServerBase
    {
        public byte PlayarID;
        public byte SeamanID;
        public byte[] texture2d;
        public int width, height, textureFormat;
        public Seaman4DCube(byte seamanid, byte userid, byte[] _texture, int _width, int _height, int _format)
        {
            this.SeamanID = seamanid;
            this.PlayarID = userid;
            this.texture2d = _texture;
            this.width = _width;
            this.height = _height;
            this.textureFormat = _format;
        }
    }

    [Serializable]
    public class CallFishEvent4DCube : ServerBase
    {
        public int EffectID;
        public string FishID;
        public CallFishEvent4DCube(string _fishid, int _effectID)
        {
            this.EffectID = _effectID;
            this.FishID = _fishid;
        }
    }

    [Serializable]
    public class Fishing4DCube : ServerBase
    {
        public byte PlayarID;
        public Fishing4DCube(byte userid)
        {
            this.PlayarID = userid;
        }
    }

    [Serializable]
    public class Fishing4DCubePos : ServerBase
    {
        public byte PlayarID;
        public float Coordinate;
        public Fishing4DCubePos(byte userid, float _coordinate)
        {
            this.PlayarID = userid;
            this.Coordinate = _coordinate;
        }
    }

    [Serializable]
    public class FeedbackFishingFullSize : ServerBase
    {
        public byte PlayarID;
        public FeedbackFishingFullSize(byte userid)
        {
            this.PlayarID = userid;
        }
    }

    [Serializable]
    public class FeedbackFishingSuccess : ServerBase
    {
        public byte PlayarID;
        public int ArrayIndex;
        public FeedbackFishingSuccess(byte userid, int arrayIndex)
        {
            this.PlayarID = userid;
            this.ArrayIndex = arrayIndex;
        }
    }
}