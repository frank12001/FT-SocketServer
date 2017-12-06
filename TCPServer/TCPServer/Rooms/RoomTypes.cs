
namespace startOnline.playar.Rooms
{
    public enum RoomTypes : byte
    {
        Base = 0,
        /// <summary>
        /// 展場用
        /// </summary>
        Exhibition, 
        /// <summary>
        /// 排隊房，排完自動轉入， PokerGamingRoom
        /// </summary>
        QueueRoom,
        /// <summary>
        /// Stellar Poker Gaming Room
        /// </summary>
        PokerGamingRoom,
    }
}
