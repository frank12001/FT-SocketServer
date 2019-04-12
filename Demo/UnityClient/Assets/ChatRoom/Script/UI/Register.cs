using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChatRoom
{
    public class Register : MonoBehaviour
    {
        public InputField Account;
        public InputField Password;
        public Button IwantRegister;
        public Action<string, string> _OnClick;

        public void OnClick()
        {
            _OnClick?.Invoke(Account.text, Password.text);
        }
    }
}