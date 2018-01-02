using System.Collections;
using System.Collections.Generic;
using System.Timers;
using FTServer;
using FTServer.Operator;
using TCPServer.Projects.Palace.Packet;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.SceneManagement;

namespace PalaceWar.Server
{
    public class PalaceServerConnecter : MonoBehaviour
    {
        public static PalaceServerConnecter Instance {
            get { return instance; }
        }
        private static PalaceServerConnecter instance = null;

        public Connect _Connect {
            get { return connect; }
        }
        private Connect connect;

        /// <summary>
        /// 遊戲進行時間
        /// </summary>
        public float GamingTime {
            get { return this.gamingTime; }
            set { gamingTime = value; }
        }
        private float gamingTime=0;
        private bool StartGamingTime = false;          

       public float ping;

        void Awake()
        {
            if (PalaceServerConnecter.Instance==null)
            {
                instance = this;
            }
            connect = GetComponent<Connect>();
        }
        // Use this for initialization
        void Start()
        {
            
            connect._gaming.ReceiveServerPacket += o =>
            {
                if (o is PalaceWar.GamingStart)
                {
                    PalaceWar.GamingStart start = (PalaceWar.GamingStart) o;

                    this.gamingTime = ConvertGamingTimeWithDelay(start.GamingTime/1000, ping/1000);
                    Debug.Log("GamingTime = "+ GamingTime);
                    StartGamingTime = true;
                }

                if (o is Monster)
                {
                    //Debug.Log("In Monster 2");
                    //Monster m = (Monster) o;

                    //float monsterShowTime = m.GetDelayTime(PalaceServerConnecter.Instance.GamingTime);
                    //Debug.Log("Server GamingTime"+ m.GamingTime );
                }
            };

            //Invoke("ConpletePing", 1);
        }
        private void ConpletePing()
        {
            if (connect.IsConnect)
                connect._system.ComputePing(new object());
            Invoke("ConpletePing", 1);
        }

        private float ConvertGamingTimeWithDelay(float serverGamingTime, float ping)
        {
            float oneRun = ping / 2;
            return serverGamingTime + oneRun;
            //return serverGamingTime;
        }

        // Update is called once per frame
        void Update()
        {
            if (StartGamingTime)
                this.gamingTime += Time.deltaTime;
            if (!connect._system.Ping.Equals(0))
            {
                if (ping.Equals(0))
                    ping = connect._system.Ping;
                else if (!ping.Equals(connect._system.Ping) && !ping.Equals(0))
                {
                    ping = ((ping + connect._system.Ping) / 2);
                }
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                connect._gaming.SendToServer(new Monster(){ Index = 1});
            }
        }

        public void LoadingReady()
        {
            connect._gaming.SendToServer(new Dictionary<byte, object>()
            {
                {(byte) 0, 1},
            });
        }

    }
}