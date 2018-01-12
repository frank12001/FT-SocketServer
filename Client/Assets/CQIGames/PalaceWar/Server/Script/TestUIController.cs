using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PalaceWar.Server
{
    public class TestUIController : MonoBehaviour
    {
        public GameObject Canvans;

        private byte uiState = 0;

        private float timer = 0.0f;
        // Use this for initialization
        void Start()
        {
            if (Canvans == null)
            {
                Canvans = FindObjectOfType<Canvas>().gameObject;
            }

            Canvans.transform.Find("QueuingText").gameObject.SetActive(false);
            Canvans.transform.Find("QueueStart/Btn_Queue").gameObject.GetComponent<Button>().enabled = false;

            string customName = PlayerPrefs.GetString("CustomName", "輸入暱稱");
            Canvans.transform.Find("CustomName/Input").gameObject.GetComponent<InputField>().text = customName;
        }

        // Update is called once per frame
        void Update()
        {
            switch (uiState)
            {
                case 1:
                    Canvans.transform.Find("QueueStart/Btn_Queue").gameObject.GetComponent<Button>().enabled = true;
                    Text queueBtnText = Canvans.transform.Find("QueueStart/Btn_Queue/Text").gameObject.GetComponent<Text>();
                    queueBtnText.text = "加入戰場";
                    break;
                case 2:
                    Canvans.transform.Find("QueueStart").gameObject.SetActive(false);
                    timer = 0.0f;
                    InvokeRepeating("Timer",0,1);
                    break;
            }
            uiState = 0;
        }

        public void Timer()
        {
            timer += 1;
            int m = (int)(timer / 60);
            int s = (int)(timer % 60);

            string content = string.Format("{0} : {1}", FixString(m.ToString()), FixString(s.ToString()));
            Canvans.transform.Find("QueuingText/WaitingTime/Content").GetComponent<Text>().text = content;
        }

        private string FixString(string s)
        {
            string result = s.ToString();
            if (result.Length<2)
                result = "0" + s;
            return result;
        }

        public void CanQueue()
        {
            uiState = 1;
        }

        public void StartQueue()
        {
            uiState = 2;
        }

        public void QueueButton_Open()
        {
            GameObject queueBtn = Canvans.transform.Find("QueueStart").gameObject;
            queueBtn.SetActive(true);
        }
        public void QueueButton_Close()
        {
            GameObject queueBtn = Canvans.transform.Find("QueueStart").gameObject;
            queueBtn.SetActive(false);
        }

        public void QueuingText_Open()
        {
            GameObject queueBtn = Canvans.transform.Find("QueuingText").gameObject;
            queueBtn.SetActive(true);
        }

        public void QueueButton_SetText(string content)
        {
            Text queueBtnText = Canvans.transform.Find("QueueStart/Btn_Queue/Text").gameObject.GetComponent<Text>();
            if(!queueBtnText.text.Equals(content))
                queueBtnText.text = content;
        }

        public void CloseInput()
        {
            Canvans.transform.Find("CustomName/Input").gameObject.SetActive(false);
        }

        public void OpenInput()
        {
            Canvans.transform.Find("CustomName/Input").gameObject.SetActive(true);
        }

        public void CloseName()
        {
            Canvans.transform.Find("CustomName/Name").gameObject.SetActive(false);
        }

        public void OpenName()
        {
            Canvans.transform.Find("CustomName/Name").gameObject.SetActive(true);
        }

        public string GetName()
        {
            return Canvans.transform.Find("CustomName/Input").gameObject.GetComponent<InputField>().text;
        }

        public void SetInputToName()
        {
            Text name = Canvans.transform.Find("CustomName/Name").gameObject.GetComponent<Text>();
            InputField input = Canvans.transform.Find("CustomName/Input").gameObject.GetComponent<InputField>();

            name.text = input.text;

            PlayerPrefs.SetString("CustomName", name.text);
            PalaceServerConnecter.Instance.CustomName = name.text;
        }
    }
}