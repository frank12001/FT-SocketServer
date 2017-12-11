using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FTServer;
using FTServer.Operator;

namespace Stellar.Poker
{
    public class PokerGameConnecter : MonoBehaviour
    {
        private Connect connect;

        public bool IsConnect {
            get { return isConnect; }
        }
        private bool isConnect = false;

        public event Action<bool, PlayerInfo[]> QueueJoinIn;
        public event Action<PokerGamingRoomStart> PokerGamingRoomStart;

        // Use this for initialization
        void Start()
        {
            connect = GameObject.FindObjectOfType<Connect>();
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
                    if (PokerGamingRoomStart != null)
                    {
                        PokerGamingRoomStart(pokerGamingRoomStart);
                    }                    
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

        #region Test Function


        public void JoinQueueRoom()
        {
            PlayerInfo info = new PlayerInfo("YaYa",100,"0","1","2","3");
            StartQueue(info);
        }
        #endregion

    }
}

