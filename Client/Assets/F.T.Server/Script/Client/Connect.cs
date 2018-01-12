using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using FTServer.Operator;

namespace FTServer
{
    public class Connect : MonoBehaviour
    {
        public string ServerIP = "192.168.0.121";
        /// <summary>
        /// 有沒有成功和伺服器建立連接
        /// </summary>
        public bool IsConnect = false;

        public Operator._Member _member;
        public Operator._Room _room;
        public Operator._Gaming _gaming;
        public Operator._System _system;
        public Operator._Queue _queue;

        GameNetWorkService gameService;
        
        void Awake()
        {
            gameService = new GameNetWorkService();
            _member = new Operator._Member(gameService);
            _room = new Operator._Room(gameService);
            _gaming = new Operator._Gaming(gameService);
            _system = new Operator._System(gameService);
            _queue = new Operator._Queue(gameService);

            gameService.Address = ServerIP + ":" + "23000";
            //gameService.Address = ServerIP + ":" + "4530";
        }
        void OnApplicationQuit()
        {
            gameService.DisConnect();
        }

        void OnDisable()
        {
            gameService.DisConnect();
        }

        // Update is called once per frame
        void Update()
        {
            gameService.Service();
            if(IsConnect != _system.IsConnect)
                IsConnect = _system.IsConnect;
        }   
    }
}
