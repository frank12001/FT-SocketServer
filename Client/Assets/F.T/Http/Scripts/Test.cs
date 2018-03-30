
using UnityEngine;
using FTServer.Http;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Input Start");
            Packet packet = new Packet();
            packet.Add("p1", "1");
            packet.Add("p2", "2");
            Http.Instance.Get("http://localhost:60216/ok",packet,v => {
                Debug.Log(v);
            });
        }

        Debug.Log("Update");
	}
}
