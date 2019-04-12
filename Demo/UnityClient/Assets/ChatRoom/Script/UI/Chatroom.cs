using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChatRoom
{
    public class Chatroom : MonoBehaviour
    {
        public Text Output;
        public InputField Input;
        public Button Send;
        public Button Exit;
        public Action _ExitOnClick;
        public Action<string> _SendOnClick;

        public void SendOnClick()
        {
            _SendOnClick?.Invoke(Input.text);
        }

        public void ExitOnClick()
        {
            _ExitOnClick?.Invoke();
        }
    }
}