using System.Diagnostics;

public class Log
{
    [Conditional("UNITY_EDITOR")]
    public static void error(string msg)
    {
        UnityEngine.Debug.LogError(msg);
    }

    [Conditional("UNITY_EDITOR")]
    public static void error(System.Exception ex)
    {
        UnityEngine.Debug.LogError(ex);
    }

    [Conditional("UNITY_EDITOR")]
    public static void info(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }

    [Conditional("UNITY_EDITOR")]
    public static void warning(string msg)
    {
        UnityEngine.Debug.LogWarning(msg);
    }
}
