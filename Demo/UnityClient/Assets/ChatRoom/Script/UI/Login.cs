using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChatRoom
{
    public class Login : MonoBehaviour
    {
        public InputField Account;
        public InputField Password;
        public Button IwantLogin;
        public Action<string, string> _OnClick;

        public void OnClick()
        {
            _OnClick?.Invoke(Account.text, Password.text);
        }
    }
}