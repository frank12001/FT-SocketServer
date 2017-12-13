using System;

namespace TCPServer.Projects.Stellar
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
        public string FirebaseUserId;

        public PlayerInfo(string playerName, int chipMultiple, string stellarId, string avatar1PartId, string avatar2PartId,
            string avatar3PartId, string firebaseuserId)
        {
            this.PlayerName = playerName;
            this.ChipMultiple = chipMultiple;
            this.StellarId = stellarId;
            this.StellarAvatar1PartId = avatar1PartId;
            this.StellarAvatar2PartId = avatar2PartId;
            this.StellarAvatar3PartId = avatar3PartId;
            this.PlayerIdInRoom = 0;
            FirebaseUserId = firebaseuserId;
        }

        public PlayerInfo Clone(string firebaseuserId)
        {
            PlayerInfo result = new PlayerInfo(this.PlayerName, this.ChipMultiple, this.StellarId, this.StellarAvatar1PartId,
                this.StellarAvatar2PartId, this.StellarAvatar3PartId, firebaseuserId);
            return result;
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
    [Serializable]
    public class GamingLicensing
    {
        public Card[] OwnedCards;
        public Card[] DestopCards;

        public byte PlayerId;

        public GamingLicensing Clone()
        {
            GamingLicensing result = new GamingLicensing();
            result.OwnedCards = new Card[OwnedCards.Length];
            result.DestopCards = new Card[DestopCards.Length];

            result.PlayerId = this.PlayerId;
            for (int i = 0; i < OwnedCards.Length; i++)
            {
                result.OwnedCards[i] = OwnedCards[i];
            }
            for (int i = 0; i < DestopCards.Length; i++)
            {
                result.DestopCards[i] = DestopCards[i];
            }
            return result;
        }
    }
    [Serializable]
    public class BettingState
    {
        public bool CanBetting;
        public byte BettingNumber;
        public BettingState(byte bettingNumber, bool canBetting)
        {
            this.BettingNumber = bettingNumber;
            this.CanBetting = canBetting;
        }
    }
    [Serializable]
    public class UseMoney
    {
        public bool Success;
        public UseMoney(bool success)
        {
            this.Success = success;
        }
    }
    [Serializable]
    public struct Card
    {
        [Serializable]
        public enum Category
        {   //4 黑桃 , 3 紅心 ,  2 方塊 , 1 梅花
            club = 1, diamond, heart, spade
        }
        public string Value;
        public Category MCategory;
        public byte Number;   // 1 ~ 13

        public Card(string value)
        {
            Value = value;
            char[] delimiterChars = { '-' };
            string[] words = value.Split(delimiterChars);
            MCategory = (Category)Enum.Parse(typeof(Category), words[0]);
            Number = byte.Parse(words[1]);
        }
    }
    [Serializable]
    public class ChangableCard
    {
        public byte CardIndex;
        public bool IsChange;
    }
    [Serializable]
    public class SendAllChangableCard
    {
        public Card[] cards;
    }
    [Serializable]
    public class GameResult
    {
        public Card[][] card;
        public byte WinnerId;
    }
}
