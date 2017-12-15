using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FTServer;
using FTServer.Operator;
using Stellar.Server;
using TCPServer.Projects.Stellar;

namespace Stellar.Poker
{
    public class PokerGameConnecter : MonoBehaviour
    {
        private Connect connect;

        /// <summary>
        /// 有無連到伺服器
        /// </summary>
        public bool IsConnect {
            get { return isConnect; }
        }
        private bool isConnect = false;

        /// <summary>
        /// 是否在 Poker 遊戲房中
        /// </summary>
        private bool InPokerGamingRoom = false;
        /// <summary>
        /// 發完牌後的暫存
        /// </summary>
        private GamingDeal PlayerGamingInfo;


        /// <summary>
        /// 加入排隊 (StartQueue) 的回傳，且當有任何玩家進入排隊時，都會觸發，回傳所有在排隊人的裝備、英雄 ..資訊
        /// </summary>
        public event Action<bool, PlayerInfo[]> QueueJoinIn;
        /// <summary>
        /// 排隊人數達標時觸發，進入遊戲。回傳所有玩家的 裝備、英雄 ..資訊
        /// </summary>
        public event Action<PokerGamingRoomStart> PokerGamingRoomStart;
        /// <summary>
        /// 發牌，觸發時機為，event PokerGamingRoomStart 觸發後的 5 秒。回傳該玩家的手牌，及桌面上的牌
        /// </summary>
        public event Action<GamingDeal> GamingDeal;
        /// <summary>
        /// 換卡要求的回傳 。 (有可能會換牌失敗)
        /// </summary>
        public event Action<ChangableCard> ChangableCard;
        /// <summary>
        /// 取得玩家丟出來，要換的牌。
        /// </summary>
        public event Action<SendAllChangableCard> AllChangableCard;
        /// <summary>
        /// GetChangAbleCard 的回傳。
        /// </summary>
        public event Action<GetTargetCard> GetChangAbleCardRes;
        /// <summary>
        /// 此玩家在換牌階段結束後，擁有的牌。
        /// </summary>
        public event Action<PlayerCardAfterChange> PlayerCardAfterChange;
        /// <summary>
        /// 下注狀態回傳
        /// </summary>
        public event Action<BettingState> CanBetting;
        /// <summary>
        /// 有無成功下注。 Brtting 的回傳
        /// </summary>
        public event Action<UseMoney> BettingRes;
        /// <summary>
        /// 遊戲結果
        /// </summary>
        public event Action<GameResult> GameResult;

        // Use this for initialization
        void Start()
        {
            HookUpCallBack();
        }


      

        /// <summary>
        /// 連線到伺服器 。連線到之後， PokerGameConnecter.IsConnect = true
        /// </summary>
        public void ConnectToServer()
        {
            connect._system.ConnectToServer();
        }
        /// <summary>
        /// 開始排隊。CallBack PokerGameConnecter.QueueJoinIn
        /// </summary>
        /// <param name="playerInfo">該玩家所需的資料</param>
        public void StartQueue(PlayerInfo playerInfo)
        {
            if (isConnect)
            {
                PlayerInfo info = playerInfo.Clone(Core.GetUserId());
                connect._queue.JoinQueue(info);
            }
        }

        /// <summary>
        /// 送出要交換的卡片。
        /// </summary>
        /// <param name="card">要換的卡片</param>
        /// <returns>如果還沒有手牌 return false</returns>
        public bool SendChangAbleCard(Card card)
        {
            /*
             * 跟 Server 說要換手上的第 x 張卡
             */
            bool result = false; //此功能有無執行成功
            if (PlayerGamingInfo!= null)
            {
                return result;
            }
            byte cardIndex = 255;
            //取出要換卡片在手上的 Index
            for (byte i = 0; i < PlayerGamingInfo.OwnedCards.Length; i++)
            {
                if (card.Value.Equals(card.Value))
                {
                    cardIndex = i;
                    break;
                }
            }
            if (cardIndex.Equals(255))
                return result;
            //SendToServer
            Dictionary<byte,object> packet = new Dictionary<byte, object>()
            {
                {(byte)0,2 },
                {(byte)1,cardIndex }
            };
            connect._gaming.SendToServer(packet);
            result = true;
            return result;
        }
        /// <summary>
        /// 送出要拿哪張卡片
        /// </summary>
        /// <param name="card"></param>
        public void GetChangAbleCard(Card card)
        {
            //ChangableCardTemp
            /*
             * 跟 Server 說要拿哪張卡
             */
            //SendToServer
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {(byte)0,2 },
                {(byte)1,card }
            };
            connect._gaming.SendToServer(packet);
        }

        /// <summary>
        /// 下注，下注數量為此遊戲房的一注。 在 CanBetting 事件，回傳 true 之後才能正確下注
        /// </summary>
        public void Betting()
        {
            //SendToServer
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {(byte)0,1 },
                {(byte)1,Serializate.ToByteArray(new UseMoney(false)) }
            };
            connect._gaming.SendToServer(packet);
        }

        #region private Function

        /// <summary>
        /// 接 operatorCode = 3 (GamingRoom) , switchcode = 3
        /// </summary>
        /// <param name="packet">接進來的類別</param>
        private void ServerCallBack(object packet)
        {
            var pokerGamingRoomStart = packet as PokerGamingRoomStart;
            var gamingLicensing = packet as GamingDeal;
            var bettingState = packet as BettingState;
            var useMoney = packet as UseMoney;
            var changableCard = packet as ChangableCard;
            var sendAllChangableCard = packet as SendAllChangableCard;
            var getTargetCard = packet as GetTargetCard;
            var playerCardAfterChange = packet as PlayerCardAfterChange;
            var gameResult = packet as GameResult;

            if (pokerGamingRoomStart != null)
            {
                Debug.Log(pokerGamingRoomStart);
                InPokerGamingRoom = true;
                if (PokerGamingRoomStart != null)
                {
                    PokerGamingRoomStart(pokerGamingRoomStart);
                }
            }

            if (gamingLicensing != null)
            {
                Debug.Log(gamingLicensing);
                this.PlayerGamingInfo = gamingLicensing.Clone();
                if (GamingDeal != null)
                {
                    GamingDeal(gamingLicensing);
                }
            }

            if (bettingState != null)
            {
                Debug.Log(bettingState);
                Debug.Log("Betting State = " + bettingState.CanBetting);
                if (CanBetting != null)
                {
                    CanBetting(bettingState);
                }
            }

            if (useMoney != null)
            {
                Debug.Log(useMoney);
                if (BettingRes != null)
                {
                    BettingRes(useMoney);
                }
            }

            if (changableCard != null)
            {
                Debug.Log(changableCard);
                if (ChangableCard != null)
                    ChangableCard(changableCard);
            }

            if (sendAllChangableCard != null)
            {
                Debug.Log(sendAllChangableCard);
                if (AllChangableCard != null)
                    AllChangableCard(sendAllChangableCard);
            }

            if (getTargetCard != null)
            {
                Debug.Log(getTargetCard);
                if (GetChangAbleCardRes != null)
                    GetChangAbleCardRes(getTargetCard);
            }

            if (playerCardAfterChange != null)
            {
                Debug.Log(playerCardAfterChange);
                if (PlayerCardAfterChange != null)
                    PlayerCardAfterChange(playerCardAfterChange);
            }

            if (gameResult != null)
            {
                Debug.Log(gameResult);
                InPokerGamingRoom = false;
                if (GameResult != null)
                    GameResult(gameResult);
            }
        }

        #region 縮排功能 。  HookUpCallBack
        /// <summary>
        /// 掛勾回呼
        /// </summary>
        private void HookUpCallBack()
        {
            connect = GetComponent<Connect>();
            if (QueueJoinIn != null)
                connect._queue.ReceiveJoinQueue += QueueJoinIn;
            connect._queue.ReceiveJoinQueue += (success, b) => { Debug.Log("Join Queue Result = " + b); };
            connect._system.Connect += () => {
                Debug.Log("Connect");
                isConnect = true;
            };
            connect._system.Disconnect += () => {
                Debug.Log("DisConnect");
                isConnect = false;
            };
            connect._gaming.ReceiveServerPacket += ServerCallBack;
            connect._gaming.ReceiveRoomType += type =>
            {
                Debug.Log(type);
            };
        }
        #endregion 
        #endregion

        #region Test Function


        public void TestJoinQueueRoom()
        {
            PlayerInfo info = new PlayerInfo("YaYa",100,"0","1","2","3",Core.GetUserId());
            StartQueue(info);
        }

        /// <summary>
        /// 交換卡片 (測試)
        /// </summary>
        /// <returns>如果還沒有手牌 return false</returns>
        public void TestChangeCard()
        {
            //SendToServer
            byte cardIndex = 1;
            Dictionary<byte, object> packet = new Dictionary<byte, object>()
            {
                {(byte)0,2 },
                {(byte)1,cardIndex }
            };
            connect._gaming.SendToServer(packet);
        }
        #endregion

    }
}

