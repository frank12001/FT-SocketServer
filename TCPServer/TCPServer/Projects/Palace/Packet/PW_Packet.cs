

using System;

namespace PalaceWar
{
    [Serializable]
    public abstract class Base
    {
        /// <summary>
        /// 遊戲在 Server 的進行時間
        /// </summary>
        public float GamingTime;
    }

    [Serializable]
    public enum Team : byte
    {
        Red, Blue
    }
    //角色陣營
    [Serializable]
    public enum Character_Camp
    {
        Character_Camp_0 = 0, Character_Camp_1 = 1, Character_Camp_2 = 2, Character_Camp_3 = 3, Character_Camp_4 = 4, Character_Camp_5 = 5, Character_Camp_6 = 6, Character_Camp_7 = 7, Character_Camp_8 = 8, Character_Camp_9 = 9
    }

    /// <summary>
    /// 遊戲時輸入的傳遞封包
    /// </summary>
    [Serializable]
    public class Loading
    {
        public bool Ready;
    }
    /// <summary>
    /// 遊戲時輸入的傳遞封包
    /// </summary>
    [Serializable]
    public class GamingStart : Base
    {
        public Team _Team;
        public string[] CardsFight, CardsCommander;
        /// <summary>
        /// 所有玩家的自定義名稱
        /// </summary>
        public string[] PlayersName;
        /// <summary>
        /// 是否跟 Bot 打
        /// </summary>
        public bool IsBot = false;
    }
    [Serializable]
    public class GamingOver
    {
        public bool Win;

    }
    [Serializable]
    public class Monster : Base
    {
        [Serializable]
        public enum Category { Giant }
        public byte Index;
        public uint HP;

        /// <summary>
        /// 計算怪物延遲修正後的演出時間
        /// </summary>
        /// <param name="clientGamingTime">Client 的遊戲進行時間</param>
        /// <returns>延遲修正後的演出時間</returns>
        public float GetDelayTime(float clientGamingTime)
        {
            //預設為 server time 之後的 2 秒
            uint bornDelay = 1;
            float bornTime = (GamingTime / 1000) + bornDelay;
            if (bornTime <= clientGamingTime)
                return 0;
            else
            {
                return bornTime - clientGamingTime;
            }
        }
    }

    [Serializable]
    public class Instantiate_FightingCharacter
    {
        public string FightingCharacterName;
        public Character_Camp FightingCharacter_Friendly_Camp;
        public Character_Camp FightingCharacter_Hostility_Camp;
        public float[] Point;
    }

    [Serializable]
    public class Update_Network_SyncData
    {
        public Character_PointData[] Character;
    }

    [Serializable]
    public class Character_PointData
    {
        public int index;
        //  public Character_Camp FightingCharacter_Friendly_Camp;
        public float[] Point;
        public float[] Rotate;
    }

    [Serializable]
    public struct Cube
    {
        public float PosX, PosY, PosZ, RotX, RotY, RotZ;
    }
    [Serializable]
    public struct Cubes
    {
        public Cube[] Cube;
    }

    /// <summary>
    /// 死亡回報
    /// </summary>
    [Serializable]
    public class Character_Death
    {
        public Character_Camp Character_Camp;
        public int Character_Index;
        public int Population_Quantity;
    }
}

