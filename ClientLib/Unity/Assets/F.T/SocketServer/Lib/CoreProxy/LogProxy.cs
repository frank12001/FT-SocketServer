

public class LogProxy
{
    public static void WriteLine(object o)
    {
#if UNITY_5_3_OR_NEWER
        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        UnityEngine.Debug.Log(o);
#else
                    Console.WriteLine(o);
#endif
    }

    public static void WriteError(object o)
    {
#if UNITY_5_3_OR_NEWER
        UnityEngine.Debug.LogError(o);
#else
                    Console.WriteLine(o);
#endif
    }
}
