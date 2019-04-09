using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FTServer.Example.ChatRoom
{
    public class ScrollView_SettoBottom : MonoBehaviour
    {
        ScrollRect rect;
        float heightTemp;
        // Use this for initialization
        void Start()
        {
            rect = GetComponent<ScrollRect>();
        }


        // Update is called once per frame
        void Update()
        {
            if (rect.content.rect.height != heightTemp)
            {
                rect.verticalNormalizedPosition = 0;
            }
            heightTemp = rect.content.rect.height;
        }
    }
}