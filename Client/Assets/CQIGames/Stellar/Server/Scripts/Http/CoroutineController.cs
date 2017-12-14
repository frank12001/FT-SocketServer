using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stellar.Server
{
    public class CoroutineController
    {
        private static FirebaseCoroutineController coroutiner = null;
        public FirebaseCoroutineController Get()
        {
            if (coroutiner == null)
            {
                //初始化協成控制器
                GameObject gameObject = new GameObject{  name = "_FirebaseCoroutineController_"};
                coroutiner = gameObject.AddComponent<FirebaseCoroutineController>();
            }
            return coroutiner;
        }
    }

    public class FirebaseCoroutineController : MonoBehaviour
    {
    }
}