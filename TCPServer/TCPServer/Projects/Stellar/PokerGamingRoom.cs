using System;
using startOnline;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace TCPServer.Projects.Stellar
{   
    public class PokerGamingRoom : Room
    {
        private const byte PlayerHoldCardNumber = 4;
        private const byte DesktopCardNumber = 4;
        private const byte BettingCount = 3;

        private float TotalGamingTime = 0.0f;
        private PlayAR.Common.Timer _logicTimer = new PlayAR.Common.Timer() { startTimer = false, nowTimer = 0.0f , max_Timer = 0.0f};
        private byte GameState = 0;
        private List<Card> TotalCard = new List<Card>();
        private List<Card> DesktopCard = new List<Card>();
        private List<Card> WaitChangeCard = new List<Card>();
        private List<byte> ChangeCardPlayersIndex = new List<byte>();

        private string[] hasMoneyTemp = new string[4];

        private Dictionary<byte, PlayerGamingInfo> playerGamingInfo = new Dictionary<byte, PlayerGamingInfo>();

        private class PlayerGamingInfo
        {
            public List<Card> OwnedCards;
            public int TotalMoney;
            public int CostMoney;
            public bool IsLose;
            public PlayerGamingInfo(List<Card> ownedCards,bool isLose,int totalMoney,int costMoney)
            {
                OwnedCards = ownedCards;
                TotalMoney = totalMoney;
                CostMoney = costMoney;
                IsLose = isLose;
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

            List<string> userids = GetUserIds(PlayerInfos);

            PeerStake = PlayerInfos[0].ChipMultiple;
            GameStart(PeerStake* BettingCount, userids.ToArray());

        }
       
        ~PokerGamingRoom()
        {
            _server.PrintLine(" PokerGamingRoom 解構子被呼叫 ");
        }

        public override void mainThread(object sender, ElapsedEventArgs e)
        {
            TotalGamingTime += (timer_interal / 1000);
            switch (GameState)
            {
                #region 0 建構子 初始化 ing
                case 0: 
                    //這時還在初始化，完成後 GameState => 1
                    break;
                #endregion
                #region  1 確認玩家進房，發給玩家和桌面牌，並傳送
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
                            bool isLose = !bool.Parse(hasMoneyTemp[@base.Key]);
                            int totalMoney = (!isLose) ? PeerStake * (BettingCount-1) : 0;                            
                            PlayerGamingInfo gamingInfo = new PlayerGamingInfo(ownedCard, isLose, totalMoney, PeerStake);
                            playerGamingInfo.Add(@base.Key, gamingInfo);
                        }
                        //建立 DesktopCard
                        DesktopCard = GetRandomCard(ref TotalCard, DesktopCardNumber);

                        //將 手牌&桌牌 ， Send 給 User
                        foreach (KeyValuePair<byte, PeerBase> @base in players)
                        {
                            GamingDeal licensing = new GamingDeal()
                            {
                                OwnedCards = playerGamingInfo[@base.Key].OwnedCards.ToArray(),
                                DestopCards = DesktopCard.ToArray(),
                                PlayerId = @base.Key,
                                _Time = TotalGamingTime
                            };
                            Dictionary<byte,object> packet1 = new Dictionary<byte, object>()
                            {
                                {(byte)0,3},
                                {(byte)1,Math.Serializate.ToByteArray(licensing)}
                            };
                            SendToAssignPlayer(packet1,@base.Key);
                        }

                        //進入下個狀態，預計先等 5 秒
                        _logicTimer.Set(true, 0, 5);
                        GameState = 2;
                    }
                    break;
                #endregion
                #region 2 等待下注
                case 2: //等待下注
                    if (_logicTimer.nowTimer.Equals(0))
                    {
                        SendBettingState(new BettingState(2,true){ _Time = TotalGamingTime });
                    }
                    _logicTimer.nowTimer += (timer_interal / 1000);
                    if (_logicTimer.nowTimer >= _logicTimer.max_Timer)
                    {
                        SetIsBetting(2);
                        //檢查如果全部棄注 ? 
                        //傳送給所有人下注結束
                        SendBettingState(new BettingState(2, false) { _Time = TotalGamingTime });
                        _logicTimer.Set(true, 0, 5);
                        GameState = 3;
                    }
                    break;
                #endregion 
                #region 3 等待傳送要換哪張牌
                case 3: //等待傳送要換哪張牌
                    _logicTimer.nowTimer += (timer_interal / 1000);
                    if (_logicTimer.nowTimer >= _logicTimer.max_Timer)
                    {
                        GameState = 4;
                    }
                    break;
                #endregion
                #region 4 將換牌堆的排送給所有人，並等待 5 秒鐘 //WaitChangeCard
                case 4:
                    //將換牌堆的排送給所有人，並等待 5 秒鐘 //WaitChangeCard
                    SendAllChangableCard sendAllChangableCard = new SendAllChangableCard()
                    {
                        cards = WaitChangeCard.ToArray(),
                        _Time = TotalGamingTime
                    };
                    Dictionary<byte, object> packet3 = new Dictionary<byte, object>()
                    {
                           {(byte)0,3},
                           {(byte)1,Math.Serializate.ToByteArray(sendAllChangableCard)}
                    };
                    for (byte i = 0; i < ChangeCardPlayersIndex.Count; i++)
                    {
                        SendToAssignPlayer(packet3, ChangeCardPlayersIndex[i]);
                    }
                    //進入下個狀態，預計先等 5 秒
                    _logicTimer.Set(true, 0, 5);
                    GameState = 5;
                    break;
                #endregion
                #region 5 等待 誰要拿哪張牌
                case 5: //等待 誰要拿哪張牌
                    _logicTimer.nowTimer += (timer_interal / 1000);
                    if (_logicTimer.nowTimer >= _logicTimer.max_Timer)
                    {                        
                        //將拿完牌後的結果回傳
                        foreach (KeyValuePair<byte, PlayerGamingInfo> info in playerGamingInfo)
                        {
                            if (!info.Value.OwnedCards.Count.Equals(PlayerHoldCardNumber) && WaitChangeCard.Count > 0)
                            {   //如果拿牌堆還有牌，將他補給沒拿的人
                                Card card = WaitChangeCard[0];
                                WaitChangeCard.Remove(card);
                                info.Value.OwnedCards.Add(card);
                            }
                            PlayerCardAfterChange newCards = new PlayerCardAfterChange(){ Cards = info.Value.OwnedCards.ToArray(),_Time = TotalGamingTime};
                            Dictionary<byte, object> packet5 = new Dictionary<byte, object>()
                            {
                                {(byte)0,3},
                                {(byte)1,Math.Serializate.ToByteArray(newCards)}
                            };
                            SendToAssignPlayer(packet5, info.Key);
                        }

                        _logicTimer.Set(true, 0, 5);
                        GameState = 6;
                    }
                    break;
                #endregion
                #region 6 等待 第三次加注
                case 6: //第三次加注
                    if (_logicTimer.nowTimer.Equals(0))
                    {
                        SendBettingState(new BettingState(3, true){_Time = TotalGamingTime});
                    }
                    _logicTimer.nowTimer += (timer_interal / 1000);
                    if (_logicTimer.nowTimer >= _logicTimer.max_Timer)
                    {

                        SetIsBetting(3);
                        //傳送給所有人下注結束
                        SendBettingState(new BettingState(3, false){_Time = TotalGamingTime});

                        _logicTimer.Set(true, 0, 5);
                        GameState = 7;
                    }
                    break;
                #endregion
                #region 7 計算誰贏誰輸 ，並將結果和所有的牌傳到所有 Client
                case 7:
                    _logicTimer.nowTimer += (timer_interal / 1000);
                    if (_logicTimer.nowTimer >= _logicTimer.max_Timer)
                    {
                        //計算誰贏誰輸
                        //使用 playerGamingInfo[index].Value.OwnedCard
                        //和 DesktopCard 算出每位玩家的總分，並比較誰最贏
                        byte winnerIndex = WhoWin();

                        Card[][] card = new Card[playerGamingInfo.Count][];
                        for (byte i = 0; i < playerGamingInfo.Count; i++)
                        {
                            PlayerGamingInfo info = playerGamingInfo[i];
                            card[i] = info.OwnedCards.ToArray();
                        }
                        //將勝負結果和所有的卡傳到 Client
                        GameResult gameResult = new GameResult()
                        {
                            WinnerId = winnerIndex,
                            card = card,
                            _Time = TotalGamingTime
                        };

                        Dictionary<byte, object> packet4 = new Dictionary<byte, object>()
                        {
                           {(byte)0,3},
                           {(byte)1,Math.Serializate.ToByteArray(gameResult)}
                        };
                        for (byte i = 0; i < playerGamingInfo.Count; i++)
                        {
                            SendToAssignPlayer(packet4, i);
                        }

                        //將錢設回去
                        List<string> userids = GetUserIds(PlayerInfos);
                        int totalCost = 0;
                        foreach (KeyValuePair<byte, PlayerGamingInfo> info in playerGamingInfo)
                        {
                            totalCost += info.Value.CostMoney;
                        }
                        string result = "";
                        foreach (KeyValuePair<byte, PlayerGamingInfo> info in playerGamingInfo)
                        {
                            int money = 0;
                            if (winnerIndex.Equals(info.Key))
                                money = totalCost;
                            money += info.Value.TotalMoney;
                            result += userids[info.Key] + ">" + money + ",";
                        }
                        result = result.Substring(0, result.Length - 1);
                        _server.printLine("result string");
                        _server.printLine(result);
                        //將此字串傳到 firebase server
                        AddMoney(result);
                        _logicTimer.Set(true, 0, 5);
                        GameState = 8;
                    }
                    break;
                #endregion 
                case 8:
                    _logicTimer.nowTimer += (timer_interal / 1000);
                    if (_logicTimer.nowTimer >= _logicTimer.max_Timer)
                    {
                        //Exit Poker Room
                        _logicTimer.Set(true, 0, 5);
                        GameState = 9;
                    }
                    break;
                case 9: //
                    _logicTimer.nowTimer += (timer_interal / 1000);
                    if (_logicTimer.nowTimer >= _logicTimer.max_Timer)
                    {
                        //Exit Poker Room
                        Room_Disband();
                        GameState = 10;
                    }
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
                #region case 1 接收下注請求  GameState 2
                case 1: //接收下注請求
                    UseMoney useMoney = new UseMoney(false);
                    if (GameState.Equals(2) || GameState.Equals(6))
                    {
                        if (!playerGamingInfo[playerId].IsLose)
                        {
                            useMoney.Success = true;
                            playerGamingInfo[playerId].TotalMoney -= PeerStake;
                            playerGamingInfo[playerId].CostMoney += PeerStake;
                        }
                    }
                    Dictionary<byte, object> resPacket = new Dictionary<byte, object>()
                    {
                        {(byte)0,3 },
                        {(byte)1,Math.Serializate.ToByteArray(useMoney) },
                    };

                    SendToAssignPlayer(resPacket, playerId);
                    break;
                #endregion
                #region case 1 接收換牌請求  GameState 3
                case 2: //接收換牌請求
                    ChangableCard changableCard = new ChangableCard();
                    changableCard.IsChange = false;
                    if (GameState.Equals(3))
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
                    resPacket = new Dictionary<byte, object>()
                    {
                        {(byte)0,3 },
                        {(byte)1,Math.Serializate.ToByteArray(changableCard) },
                    };

                       SendToAssignPlayer(resPacket, playerId);
                    break;
                #endregion
                #region case 3 接收拿牌請求 GameState 5
                case 3:
                    GetTargetCard resCard = new GetTargetCard(){ Success = false};
                    Card targetCard = (Card)Math.Serializate.ToObject((byte[])packet[1]);
                    if (GameState.Equals(5))
                    {
                        //接收想拿哪張牌
                        if (WaitChangeCard.Contains((targetCard)) && playerGamingInfo[playerId].OwnedCards.Count < PlayerHoldCardNumber)
                        {
                            WaitChangeCard.Remove(targetCard);
                            playerGamingInfo[playerId].OwnedCards.Add(targetCard);
                        }
                    }
                    resCard.card = targetCard;
                    resPacket = new Dictionary<byte, object>()
                    {
                        {(byte)0,3 },
                        {(byte)1,Math.Serializate.ToByteArray(resCard) },
                    };

                    SendToAssignPlayer(resPacket, playerId);
                    break;
                    #endregion
            }
        }

        /// <summary>
        /// 傳送給所有人 可以 or 不行 下注
        /// </summary>
        /// <param name="startBetting"></param>
        private void SendBettingState(BettingState bettingState)
        {
            //傳送給所有人說，開始下注
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {(byte)0,3 }, //switch Code , Server Call Back
                {(byte)1,Math.Serializate.ToByteArray(bettingState) },
            };
            BroadcastPacket(packet);
        }

        /// <summary>
        /// 設定有無下注
        /// </summary>
        /// <param name="bettingNumber"></param>
        private void SetIsBetting(byte bettingNumber)
        {
            //檢查誰沒有下注
            foreach (KeyValuePair<byte, PlayerGamingInfo> info in playerGamingInfo)
            {
                if (!info.Value.IsLose)
                    info.Value.IsLose = (!info.Value.IsLose && !info.Value.CostMoney.Equals(PeerStake * bettingNumber));
            }
        }

        /// <summary>
        /// 遊戲開始的設定 (去 Firebase 進行資料確認)
        /// </summary>
        /// <param name="monryCount">確認鑽石數量</param>
        /// <param name="useridStrings">玩家們的 id</param>
        private async void GameStart(int monryCount, string[] useridStrings)
        {
            string result = await AccessFirebaseServerCheckMoneyAsync(monryCount, useridStrings);
            result = result.Substring(1, result.Length - 2);
            _server.printLine("result " + result);
            char[] delimiterChars = { ',' };
            string[] words = result.Split(delimiterChars);
            hasMoneyTemp = words;

            PokerGamingRoomStart poker = new PokerGamingRoomStart(PlayerInfos.ToArray());
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {0,3},
                {1,TCPServer.Math.Serializate.ToByteArray(poker) },
            };
            BroadcastPacket(packet);

            TotalCard = GetAllCard();
            //5 秒後進入下個遊戲狀態
            _logicTimer.Set(true, 0, 5);
            GameState = 1;
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

        /// <summary>
        /// 去 Firebase Server 確認，多位 user 的鑽石夠不夠玩
        /// </summary>
        /// <param name="moneyCount">確認的鑽石數量</param>
        /// <param name="useridStrings">玩家們的 id </param>
        /// <returns>回傳字串為 [true,true,false] ..傳入幾位玩家就回傳幾位玩家的鑽石夠不夠</returns>
        private async Task<string> AccessFirebaseServerCheckMoneyAsync(int moneyCount, string[] useridStrings)
        {
            string json = "";
            for (int i = 0; i < useridStrings.Length; i++)
            {
                json += "," + useridStrings[i];
            }
            json = json.Substring(1, json.Length - 1);
            //json = "[" + json + "]";
            _server.printLine("json " + json);

            HttpClient client = new HttpClient();
            string url = string.Format("https://us-central1-stellar-38931.cloudfunctions.net/PokerServer?parameter1=CheckUserMoney&parameter2={0}&parameter3={1}", moneyCount.ToString(), json);

            Task<string> getStringTask = client.GetStringAsync(url);

            string urlContents = "";
            try
            {
                urlContents = await getStringTask;
            }
            catch (Exception e)
            {
                json = "";
                for (int i = 0; i < useridStrings.Length; i++)
                {
                    json += "," + useridStrings[i];
                }
                json = json.Substring(1, json.Length - 1);
                json = "[" + json + "]";
            }
            _server.printLine("res " + urlContents);
            return urlContents;
        }

        private async void AddMoney(string parameter)
        {
            await AccessFirebaseServerAddMoneyAsync(parameter);
        }

        /// <summary>
        /// 去 Firebase Server 確認，多位 user 的鑽石夠不夠玩
        /// </summary>
        /// <param name="moneyCount">確認的鑽石數量</param>
        /// <param name="useridStrings">玩家們的 id </param>
        /// <returns>回傳字串為 [true,true,false] ..傳入幾位玩家就回傳幾位玩家的鑽石夠不夠</returns>
        private async Task<string> AccessFirebaseServerAddMoneyAsync(string parameter)
        {

            HttpClient client = new HttpClient();
            string url = string.Format("https://us-central1-stellar-38931.cloudfunctions.net/PokerServer?parameter1=AddMoney&parameter2={0}", parameter);
            _server.printLine("url " + url);
            try
            {
                Task<string> getStringTask = client.GetStringAsync(url);
            }
            catch (Exception e)
            {
                _server.printLine("Res Error " + e.Message);
                throw;
            }

            string urlContents = "";
            _server.printLine("res " + urlContents);
            return urlContents;
        }
        /// <summary>
        /// 誰贏這場遊戲
        /// </summary>
        /// <returns>贏家的 id in Room</returns>
        private byte WhoWin()
        {
            return 0;
        }

        /// <summary>
        /// 將該 List<PlayerInfo> 的 userids 拉出來做成 List ，目前使用測式版
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        private List<string> GetUserIds(List<PlayerInfo> infos)
        {
            //Release
            List<string> userids = new List<string>();
            for (byte i = 0; i < PlayerInfos.Count; i++)
            {
                userids.Add(PlayerInfos[i].FirebaseUserId);
            }
            //Test
            //List<string> userids = new List<string>(new string[] { "-L07-epS6L6ApPWZcojd", "-L07KMKucuGHBTXOWMMs" });
            return userids;
        }
    }
}
