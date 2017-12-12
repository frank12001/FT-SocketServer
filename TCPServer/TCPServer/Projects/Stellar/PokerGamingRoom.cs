using System;
using startOnline;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms.VisualStyles;
using TVEducation.ServerClientSwap;

namespace TCPServer.Projects.Stellar
{   
    public class PokerGamingRoom : Room
    {
        private const byte PlayerHoldCardNumber = 4;
        private const byte DesktopCardNumber = 4;

        private PlayAR.Common.Timer _logicTimer = new PlayAR.Common.Timer() { startTimer = false, nowTimer = 0.0f , max_Timer = 0.0f};
        private byte GameState = 0;
        private List<Card> TotalCard = new List<Card>();
        private List<Card> DesktopCard = new List<Card>();
        private List<Card> WaitChangeCard = new List<Card>();
        private List<byte> ChangeCardPlayersIndex = new List<byte>();

        private Dictionary<byte, PlayerGamingInfo> playerGamingInfo = new Dictionary<byte, PlayerGamingInfo>();

        private struct PlayerGamingInfo
        {
            public List<Card> OwnedCards;
            public int TotalMoney;
            public bool IsLose;
            public PlayerGamingInfo(List<Card> ownedCards)
            {
                OwnedCards = ownedCards;
                TotalMoney = 0;
                IsLose = false;
            }
        }
        
        /// <summary>
        /// 每次加注的金額
        /// </summary>
        private int PeerStake = 0;

        public List<PlayerInfo> PlayerInfos = null;

        public PokerGamingRoom(List<PlayerInfo> playerInfos, string customName, PokerPeer[] joinPlayers, string roomIndexInApplication, Form1 applicationPointer) : base(customName,joinPlayers,roomIndexInApplication,applicationPointer)
        {
            _server.printLine("In Poker Gaming Room");
            PlayerInfos = new List<PlayerInfo>(playerInfos);
            //foreach (PokerPeer peer in joinPlayers)
            //{
            //    peer._Queueing = false; //結束排隊
            //}
            PokerGamingRoomStart poker = new PokerGamingRoomStart(PlayerInfos.ToArray());
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {0,3},
                {1,TCPServer.Math.Serializate.ToByteArray(poker) },
            };
            BroadcastPacket(packet);

            TotalCard = GetAllCard();
            //5 秒後進入下個遊戲狀態
            _logicTimer.Set(true,0,5);
            GameState = 1;
        }

        public override void mainThread(object sender, ElapsedEventArgs e)
        {
            switch (GameState)
            {
                case 1:
                    _logicTimer.nowTimer += (timer_interal/1000);
                    if (_logicTimer.nowTimer >= _logicTimer.max_Timer)
                    {
                        //發牌 1. User , desktop 
                        //建立 playerGamingInfo
                        playerGamingInfo = new Dictionary<byte, PlayerGamingInfo>();
                        foreach (KeyValuePair<byte, PeerBase> @base in players)
                        {
                            List<Card> ownedCard = GetRandomCard(ref TotalCard, PlayerHoldCardNumber);                           
                            PlayerGamingInfo gamingInfo = new PlayerGamingInfo(ownedCard);
                            playerGamingInfo.Add(@base.Key, gamingInfo);
                        }
                        //建立 DesktopCard
                        DesktopCard = GetRandomCard(ref TotalCard, DesktopCardNumber);

                        //將 手牌&桌牌 ， Send 給 User
                        foreach (KeyValuePair<byte, PeerBase> @base in players)
                        {
                            GamingLicensing licensing = new GamingLicensing()
                            {
                                OwnedCards = playerGamingInfo[@base.Key].OwnedCards.ToArray(),
                                DestopCards = DesktopCard.ToArray(),
                                PlayerId = @base.Key
                            };
                            Dictionary<byte,object> packet = new Dictionary<byte, object>()
                            {
                                {(byte)0,3},
                                {(byte)1,Math.Serializate.ToByteArray(licensing)}
                            };
                            SendToAssignPlayer(packet,@base.Key);
                        }

                        //進入下個狀態，預計先等 5 秒
                        _logicTimer.Set(true, 0, 5);
                        GameState = 2;
                    }
                    break;
                    
                case 2: //等待傳送要換哪張牌
                    _logicTimer.nowTimer += (timer_interal / 1000);
                    if (_logicTimer.nowTimer >= _logicTimer.max_Timer)
                    {
                        GameState = 3;
                    }
                    break;
                case 3:

                    break;
                    
            }
        }


        public override void GamingProcess(byte playerId, Dictionary<byte, object> packet)
        {
            byte switchcode_1 = byte.Parse(packet[0].ToString()); //switch code
            switch (switchcode_1)
            {
                #region case 0 客製化封包 switch code 2 = 轉傳對象， 0 = 所有人 、 1 = 自己之外所有人、2 = RoomOwner(房主)
                case 0: //客製化封包
                    /* Receive packet
                     * 0 = switch code , 0 = 客製化封包
                     * 1 = 傳送給誰  0 = transform to every one  , 1 = transform to every . Exception someone
                     * 2 = 遊戲封包
                     */
                    byte switchcode_2 = byte.Parse(packet[1].ToString());
                    switch (switchcode_2)
                    {
                        case 0:    //傳給所有人
                            BroadcastPacket(packet);
                            break;
                        case 1:    //傳給除了自己之外的人
                            BroadcastPacket(packet, playerId);
                            break;
                        case 2:    //轉傳給 RoomOwner
                            SendToAssignPlayer(packet, Ownerid);
                            break;
                    }
                    break;
                #endregion
                case 1: //接收換牌請求
                    ChangableCard changableCard = new ChangableCard();
                    changableCard.IsChange = false;
                    if (GameState.Equals(2))
                    {
                        if (!ChangeCardPlayersIndex.Contains(playerId))
                        {
                            //牌的 Index
                            byte cardId = byte.Parse(packet[1].ToString());
                            Card changAbleCard = playerGamingInfo[playerId].OwnedCards[cardId];
                            WaitChangeCard.Add(changAbleCard);

                            playerGamingInfo[playerId].OwnedCards.RemoveAt(cardId);
                            ChangeCardPlayersIndex.Add(playerId);

                            //送出成功結果
                            changableCard.IsChange = true;
                        }
                    }
                    Dictionary<byte,object> resPacket = new Dictionary<byte, object>()
                    {
                        {(byte)0,3 },
                        {(byte)1,Math.Serializate.ToByteArray(changableCard) },
                    };

                    SendToAssignPlayer(resPacket, playerId);
                    break;
                        
            }
        }

        /// <summary>
        /// 取得所有的卡片
        /// </summary>
        /// <returns></returns>
        private List<Card> GetAllCard()
        {
            List<Card> result = new List<Card>(); ;
            for (byte i = 1; i <= 4; i++)
            {
                Card.Category category = (Card.Category)i;
                for (byte j = 1; j <= 13; j++)
                {
                    string cardString = string.Format("{0}-{1}", category.ToString(), j.ToString());
                    Card card = new Card(cardString);
                    result.Add(card);
                }
            }
            return result;
        }

        /// <summary>
        /// 從指定的牌堆中，隨機 "取出" 指定數量的卡
        /// </summary>
        /// <param name="cardPool">牌堆</param>
        /// <param name="randomNum">要取出卡的數量</param>
        /// <returns></returns>
        private List<Card> GetRandomCard(ref List<Card> cardPool, byte randomNum)
        {
            List<Card> result = new List<Card>();
            for (int i = 0; i < randomNum; i++)
            {
                int indexNext = new Random().Next(0, cardPool.Count);
                result.Add(cardPool[indexNext]);
                cardPool.RemoveAt(indexNext);
            }

            return result;
        }
    }
}
