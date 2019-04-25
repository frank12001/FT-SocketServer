using FTServer.Operator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace FTServer.Example
{
    public class _System : CallbackHandler
    {
        public const byte OperatorCode = 255;
        private Timer timer = new Timer();
        private float time = 0.0f;
        private const float ConnectToServerLimit = 5;

        private bool isConnect = false;
        public bool IsConnect { get { return this.isConnect; } }
        //連線時觸發
        public event Action Connect;
        //斷線時觸發
        public event Action Disconnect;

        protected override void AfterAddService()
        {
            //多掛勾個 連線/斷線 觸發
            this.gameService.ConnectFromServer += this.connect;
            this.gameService.DisconnectFromServer += this.disConnect;

            setTimer(ref this.timer, 1000);
        }
        #region 傳送遊戲封包 (主動)
        /// <summary>
        /// 連線到伺服器
        /// </summary>
        public void ConnectToServer()
        {
            if (this.timer.Enabled)
            {
                if (time > TimeSpan.FromSeconds(ConnectToServerLimit).TotalMilliseconds)
                {
                    this.timer.Stop();
                    this.time = 0;
                }
                return; //如果正在計時，則跳出
            }
            this.timer.Start();
            if (!IsConnect)
            {
                Debug.Log("ConnectToServer");
                gameService.ConnectToServer();
            }
        }

        public void CloseConnect()
        {
            if (this.timer.Enabled)
            {
                this.timer.Stop();
                this.time = 0;
            }
            if (!IsConnect)
                return;

            gameService.DisConnect();
        }
        #endregion
        #region Receive Server CallBack (被動呼叫)
        public override void ServerCallback(Dictionary<byte, object> server_packet)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 當剛連線時呼叫
        /// </summary>
        private void connect()
        {
            isConnect = true;
            if (Connect != null)
                Connect();
        }
        /// <summary>
        /// 掛勾給 gameService 當斷線時呼叫
        /// </summary>
        private void disConnect()
        {
            isConnect = false;
            if (this.timer.Enabled)
            {
                this.timer.Stop();
                this.time = 0;
            }
            if (Disconnect != null)
                Disconnect();
        }
        #endregion
        #region 私有方法 縮排用
        /// <summary>
        /// 初始化計時器
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="interval"></param>
        private void setTimer(ref Timer timer, double interval)
        {
            // Create a timer with a two second interval.
            timer = new System.Timers.Timer(interval);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimer;
            timer.AutoReset = true;
        }
        /// <summary>
        /// 用來計算時間的功能
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            this.time += (float)timer.Interval;
        }
        #endregion
    }
}