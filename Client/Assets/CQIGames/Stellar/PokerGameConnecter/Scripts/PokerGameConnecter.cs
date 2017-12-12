using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FTServer;
using FTServer.Operator;
using TCPServer.Projects.Stellar;

namespace Stellar.Poker
{
    public class PokerGameConnecter : MonoBehaviour
    {
        private Connect connect;

        public bool IsConnect {
            get { return isConnect; }
        }
        private bool isConnect = false;

        private bool InPokerGamingRoom = false;
        private GamingLicensing PlayerGamingInfo;
        public event Action<bool, PlayerInfo[]> QueueJoinIn;
        public event Action<PokerGamingRoomStart> PokerGamingRoomStart;
        public event Action<GamingLicensing> GamingLicensing;
        /// <summary>
        /// 換卡要求的回傳 。 (有可能會失敗)
        /// </summary>
        public event Action<ChangableCard> ChangableCard;



        // Use this for initialization
        void Start()
        {
            connect = GetComponent<Connect>();
            if(QueueJoinIn != null)
                connect._queue.ReceiveJoinQueue += QueueJoinIn;
            connect._queue.ReceiveJoinQueue += (success,b) => { Debug.Log("Join Queue Result = " + b); };
            connect._system.Connect += () => { Debug.Log("Connect");
                isConnect = true;
            };
            connect._system.Disconnect += () => { Debug.Log("DisConnect");
                isConnect = false;
            };
            connect._gaming.ReceiveServerPacket += packet =>
            {
                var pokerGamingRoomStart = packet as PokerGamingRoomStart;
                if (pokerGamingRoomStart != null)
                {
                    Debug.Log(pokerGamingRoomStart);
                    InPokerGamingRoom = true;
                    if (PokerGamingRoomStart != null)
                    {
                        PokerGamingRoomStart(pokerGamingRoomStart);
                    }                    
                }

                var gamingLicensing = packet as GamingLicensing;
                if (gamingLicensing != null)
                {
                    Debug.Log(gamingLicensing);
                    this.PlayerGamingInfo = gamingLicensing.Clone();
                    if (GamingLicensing != null)
                    {
                        GamingLicensing(gamingLicensing);
                    }
                }

                var changableCard = packet as ChangableCard;
                if (changableCard != null)
                {
                    Debug.Log(changableCard);
                }

                //SendAllChangableCard
                var sendAllChangableCard = packet as SendAllChangableCard;
                if (sendAllChangableCard != null)
                {
                    Debug.Log(sendAllChangableCard);
                }

                //GameResult
                var gameResult = packet as GameResult;
                if (gameResult != null)
                {
                    Debug.Log(gameResult);
                }
            };
            connect._gaming.ReceiveRoomType += type =>
            {
                Debug.Log(type);
            };
        }

        /// <summary>
        /// 連線到伺服器 。 PokerGameConnecter.IsConnect
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
                connect._queue.JoinQueue(playerInfo);
            }
        }

        /// <summary>
        /// 交換卡片
        /// </summary>
        /// <param name="card">要換的卡片</param>
        /// <returns>如果還沒有手牌 return false</returns>
        public bool ChangeCard(Card card)
        {
            bool result = false; //此功能有無執行成功
            if (PlayerGamingInfo!= null)
            {
                return result;
            }
            byte cardIndex = 255;
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
                {(byte)0,1 },
                {(byte)2,cardIndex }
            };
            connect._gaming.SendToServer(packet);
            result = true;
            return result;
        }

        #region Test Function


        public void JoinQueueRoom()
        {
            PlayerInfo info = new PlayerInfo("YaYa",100,"0","1","2","3");
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
                {(byte)0,1 },
                {(byte)1,cardIndex }
            };
            connect._gaming.SendToServer(packet);
        }
        #endregion

    }
}

