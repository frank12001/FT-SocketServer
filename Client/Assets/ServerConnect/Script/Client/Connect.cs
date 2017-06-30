using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

namespace Playar.PhotonServer
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

        GameNetWorkService gameService;
        
        void Awake()
        {
            gameService = new GameNetWorkService();
            _member = new Operator._Member(gameService);
            _room = new Operator._Room(gameService);
            _gaming = new Operator._Gaming(gameService);
            _system = new Operator._System(gameService);

            gameService.Address = ServerIP + ":" + "23000";
            //gameService.Address = ServerIP + ":" + "4530";
        }
        // Use this for initialization
        void Start()
        {

        }
        void OnApplicationQuit()
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
