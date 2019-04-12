using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChatRoom
{
    public class SelectChatroom : MonoBehaviour
    {
        public Dropdown Roomlist;
        public Button Join;
        public Action<string> _OnClick;

        public void OnClick()
        {
            if(Roomlist.options.Count > 0)
                _OnClick?.Invoke(Roomlist.options[Roomlist.value].text);
        }
    }
}