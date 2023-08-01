using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Log
{
    /// <summary>
    /// ��ӡInfo��־
    /// </summary>
    /// <param name="msg"></param>
    public static void Info(string msg)
    {
        if (!AppConst.OpenLog)
            return;
        Debug.Log(msg);
    }

    /// <summary>
    /// ��ӡWarning��־
    /// </summary>
    /// <param name="msg"></param>
    public static void Warning(string msg)
    {
        if (!AppConst.OpenLog)
            return;
        Debug.LogWarning(msg);
    }

    /// <summary>
    /// ��ӡError��־
    /// </summary>
    /// <param name="msg"></param>
    public static void Error(string msg)
    {
        if (!AppConst.OpenLog)
            return;
        Debug.LogError(msg);
    }
}
