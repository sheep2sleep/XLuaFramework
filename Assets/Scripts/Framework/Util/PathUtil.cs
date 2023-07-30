using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUtil
{
    // ��Ŀ¼
    public static readonly string AssetsPath = Application.dataPath;

    // ��Ҫ��Bundle��Ŀ¼
    public static readonly string BuildResourcesPath = AssetsPath + "/BuildResources/";

    // Bundle���Ŀ¼
    public static readonly string BundleOutPath = Application.streamingAssetsPath;
    
    // ֻ��Ŀ¼
    public static readonly string ReadPath = Application.streamingAssetsPath;

    // �ɶ�дĿ¼
    public static readonly string ReadWritePath = Application.persistentDataPath;

    // LuaĿ¼
    public static readonly string LuaPath = "Assets/BuildResources/LuaScripts";

    // Bundle��Դ·��
    public static string BundleResourcePath
    {
        get
        {// ����Ǹ���ģʽ�����־û�·����ΪBundle��Դ·��
            if (AppConst.GameMode == GameMode.UpdateMode)
                return ReadWritePath;
            return ReadPath;
        }
    }

    /// <summary>
    /// ��ȡUnity�����·��
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetUnityPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        return path.Substring(path.IndexOf("Assets"));
    }

    /// <summary>
    /// ��ȡ��׼·��
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetStandardPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        return path.Trim().Replace("\\", "/");
    }

    public static string GetLuaPath(string name)
    {
        return string.Format("Assets/BuildResources/LuaScripts/{0}.bytes", name);
    }

    public static string GetUIPath(string name)
    {
        return string.Format("Assets/BuildResources/UI/Prefabs/{0}.prefab", name);
    }

    public static string GetMusicPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Music/{0}", name);
    }

    public static string GetSoundPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Sound/{0}", name);
    }

    public static string GetEffectPath(string name)
    {
        return string.Format("Assets/BuildResources/Effect/Prefabs/{0}.prefab", name);
    }

    public static string GetSpritePath(string name)
    {
        return string.Format("Assets/BuildResources/Sprites/{0}", name);
    }

    public static string GetScenePath(string name)
    {
        return string.Format("Assets/BuildResources/Scenes/{0}.unity", name);
    }
}
