using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Log
{
    /// <summary>
    /// 打印Info日志
    /// </summary>
    /// <param name="msg"></param>
    public static void Info(string msg)
    {
        if (!AppConst.OpenLog)
            return;
        Debug.Log(msg);
    }

    /// <summary>
    /// 打印Warning日志
    /// </summary>
    /// <param name="msg"></param>
    public static void Warning(string msg)
    {
        if (!AppConst.OpenLog)
            return;
        Debug.LogWarning(msg);
    }

    /// <summary>
    /// 打印Error日志
    /// </summary>
    /// <param name="msg"></param>
    public static void Error(string msg)
    {
        if (!AppConst.OpenLog)
            return;
        Debug.LogError(msg);
    }
}
