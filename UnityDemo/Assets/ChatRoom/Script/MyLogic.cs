using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace ChatRoom
{
    public class MyLogic : MonoBehaviour
    {
        UIManager UIManager;
        Main _Network;
        MemberValue myMemberValue;
        // Use this for initialization
        void Start()
        {
            _Network = FindObjectOfType<Main>();
            _Network._GroupCallBackHandler.BroadcastAction = GetBroadcast;

            UIManager = FindObjectOfType<UIManager>();
            UIManager.Register.gameObject.SetActive(true);
            UIManager.Login.gameObject.SetActive(true);
            UIManager.SelectChatroom.gameObject.SetActive(false);
            UIManager.Chatroom.gameObject.SetActive(false);

            UIManager.Register._OnClick = Register;
            UIManager.Login._OnClick = Login;
            UIManager.SelectChatroom._OnClick = JoinGroup;
            UIManager.Chatroom._SendOnClick = Broadcast;
            UIManager.Chatroom._ExitOnClick = ExitGroup;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Register(string account, string password)
        {
            if (!CheckAccount(account,password))
            {
                return;
            }

            string key = ConvertToKey(account, password);
            _Network._AccountCallBack.Get(key, res => 
            {
                if (res.Length > 0)
                    Debug.Log("這組帳號密碼已使用過");
                else
                {
                    _Network._AccountCallBack.Set(key, JsonConvert.SerializeObject(new MemberValue(account)),res1 => 
                    {
                        if (res1.Length > 0)
                        {
                            Debug.Log("註冊成功");
                        }
                        else
                            Debug.Log("註冊失敗");
                    });
                }
            });
        }

        private void Login(string account, string password)
        {
            if (!CheckAccount(account,password))
            {
                return;
            }

            string key = ConvertToKey(account,password);
            _Network._AccountCallBack.Get(key, res =>
            {
                if (res.Length > 0)
                {
                    Debug.Log("Login 成功");
                    MemberValue memberValue = JsonConvert.DeserializeObject<MemberValue>(res);
                    UIManager.Register.gameObject.SetActive(false);
                    UIManager.Login.gameObject.SetActive(false);

                    UIManager.SelectChatroom.gameObject.SetActive(true);
                    _Network._GroupCallBackHandler.GetList(groups => 
                    {
                        UIManager.SelectChatroom.Roomlist.ClearOptions();
                        List<string> options = new List<string>(groups);
                        string myRoom = $"{memberValue.Name} 的房間";
                        if(!options.Contains(myRoom))
                            options.Add(myRoom);
                        UIManager.SelectChatroom.Roomlist.AddOptions(options);
                        myMemberValue = memberValue;
                    });
                }
                else
                {
                    Debug.Log("Login 失敗");
                }
            });
        }

        private void JoinGroup(string groupId)
        {
            _Network._GroupCallBackHandler.Join(groupId);
            UIManager.SelectChatroom.gameObject.SetActive(false);
            UIManager.Chatroom.Output.text = string.Empty;
            UIManager.Chatroom.gameObject.SetActive(true);
        }

        private void ExitGroup()
        {
            _Network._GroupCallBackHandler.Exit();
            UIManager.Chatroom.Output.text = string.Empty;
            UIManager.Chatroom.gameObject.SetActive(false);
            UIManager.SelectChatroom.gameObject.SetActive(true);
        }

        private void Broadcast(string msg)
        {
            msg = $"[{DateTime.Now.ToString()}] {myMemberValue.Name} : {msg}";
            _Network._GroupCallBackHandler.Broadcast(msg);
        }

        private void GetBroadcast(string msg)
        {
            UIManager.Chatroom.Output.text += msg + "\n";
        }

        private bool CheckAccount(string account, string password)
        {
            //在這裡自訂帳密的檢查
            bool result = true;
            if (account.Length < 8 || password.Length < 8)
            {
                Debug.Log("帳密都要超過 8 碼");
                result = false;
            }
            return result;
        }

        private string ConvertToKey(string account, string password)
        {
            MemberKey memberKey = new MemberKey() { Account = account, Password = password };
            return JsonConvert.SerializeObject(memberKey);
        }

        private struct MemberKey
        {
            public string Account;
            public string Password;
        }
        private struct MemberValue
        {
            public string Name;
            public int Lv;
            public int Diamond; //注意 : 這裡是由 client 端進行設定，如需傳送用真錢買的鑽石，需在伺服器增加功能
            public MemberValue(string name)
            {
                Name = name;
                Lv = 1;
                Diamond = 0;
            }
        }
    }
    
}
