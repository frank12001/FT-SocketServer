using System;

namespace Stellar.Poker
{
    /// <summary>
    /// 開始排隊時，傳入的資料
    /// </summary>
    [Serializable]
    public struct PlayerInfo
    {
        public string PlayerName, StellarId, StellarAvatar1PartId, StellarAvatar2PartId, StellarAvatar3PartId;
        public int ChipMultiple;
        public byte PlayerIdInRoom;

        public PlayerInfo(string playerName, int chipMultiple, string stellarId, string avatar1PartId, string avatar2PartId,
            string avatar3PartId)
        {
            this.PlayerName = playerName;
            this.ChipMultiple = chipMultiple;
            this.StellarId = stellarId;
            this.StellarAvatar1PartId = avatar1PartId;
            this.StellarAvatar2PartId = avatar2PartId;
            this.StellarAvatar3PartId = avatar3PartId;
            this.PlayerIdInRoom = 0;
        }
    }
    /// <summary>
    /// 開始排隊時，傳入的資料
    /// </summary>
    [Serializable]
    public class PokerGamingRoomStart
    {
        public PlayerInfo[] PlayerInfos;
        public PokerGamingRoomStart(PlayerInfo[] playerInfos)
        {
            this.PlayerInfos = playerInfos;
        }
    }
}
