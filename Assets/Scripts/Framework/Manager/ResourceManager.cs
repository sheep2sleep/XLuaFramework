using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UObject = UnityEngine.Object;


public class ResourceManager : MonoBehaviour
{
    // 文件信息类
    internal class BundleInfo
    {
        public string AssetsName;
        public string BundleName;
        public List<string> Dependences;
    }

    internal class BundleData
    {
        public AssetBundle Bundle;
        // 引用计数
        public int Count;
        public BundleData(AssetBundle ab)
        {
            // 构造函数时引用计数设置为1
            Bundle = ab;
            Count = 1;
        }
    }

    // 存储所有Bundle信息的集合
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();
    // 存储所有已加载的Bundle资源
    private Dictionary<string, BundleData> m_AssetBundles = new Dictionary<string, BundleData>();

    /// <summary>
    /// 解析版本文件
    /// </summary>
    public void ParseVersionFile()
    {
        // 版本文件的路径
        string url = Path.Combine(PathUtil.BundleResourcePath, AppConst.FileListName);
        string[] data = File.ReadAllLines(url);

        // 解析文件信息
        for(int i=0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split('|');
            bundleInfo.AssetsName = info[0];
            bundleInfo.BundleName = info[1];
            // list特性：本质是数组，但可动态扩容
            bundleInfo.Dependences = new List<string>(info.Length - 2);
            for(int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependences.Add(info[j]);
            }
            m_BundleInfos.Add(bundleInfo.AssetsName, bundleInfo);
            //单独记录Lua文件
            if (info[0].IndexOf("LuaScripts") > 0)
                Manager.Lua.LuaNames.Add(info[0]);
        }
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="assetName">资源名</param>
    /// <param name="action">完成回调</param>
    /// <returns></returns>
    IEnumerator LoadBundleAsync(string assetName, Action<UObject> action = null)
    {
        // 拿到Bundle名称和路径
        string bundleName = m_BundleInfos[assetName].BundleName;
        string bundlePath = Path.Combine(PathUtil.BundleResourcePath, bundleName);
        // 拿到依赖资源
        List<string> dependences = m_BundleInfos[assetName].Dependences;
        if(dependences != null && dependences.Count > 0)
        {
            // 递归加载依赖的资源
            for(int i = 0; i<dependences.Count; i++)
            {
                yield return LoadBundleAsync(dependences[i]);
            }
        }

        // 加载Bundle
        BundleData bundle = GetBundle(bundleName);
        if(bundle == null)
        {
            // 对象池中存在就从对象池中取出Bundle
            UObject obj = Manager.Pool.Spawn("AssetBundle", bundleName);
            if(obj != null)
            {
                AssetBundle ab = obj as AssetBundle;
                bundle = new BundleData(ab);
            }
            else
            {
                // 对象池中没有就创建一个
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
                yield return request;
                bundle = new BundleData(request.assetBundle);                
            }
            m_AssetBundles.Add(bundleName, bundle);
        }
            
        // 加载资源
        if (assetName.EndsWith(".unity"))//场景资源不通过该方式加载
        {
            action?.Invoke(null);
            yield break;
        }

        // 加载的是依赖资源而不是Bundle资源时，直接退出协程，不需要调用回调
        if(action == null)
        {
            yield break;
        }

        AssetBundleRequest bundleRequest = bundle.Bundle.LoadAssetAsync(assetName);
        yield return bundleRequest;

        Debug.Log("this is LoadBundleAsync");
        // 调用回调
        if (action != null && bundleRequest != null)
        {
            action.Invoke(bundleRequest.asset);
        }
    }

    /// <summary>
    /// 获取缓存中的Bundle
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    BundleData GetBundle(string name)
    {
        BundleData bundle = null;
        if(m_AssetBundles.TryGetValue(name,out bundle))
        {
            bundle.Count++;
            return bundle;
        }
        return null;
    }

    /// <summary>
    /// 减去bundle和依赖的引用计数
    /// </summary>
    /// <param name="assetName"></param>
    public void MinusBundleCount(string assetName)
    {
        string bundleName = m_BundleInfos[assetName].BundleName;
        MinusOneBundleCount(bundleName);// 减去Bundle的引用计数
        // 依赖资源
        List<string> dependences = m_BundleInfos[assetName].Dependences;
        if (dependences != null)
        {
            foreach(string dependence in dependences)
            {
                string name = m_BundleInfos[dependence].BundleName;
                MinusOneBundleCount(name);// 减去依赖的引用计数
            }
        }
    }

    /// <summary>
    /// 减去一个Bundle的引用计数
    /// </summary>
    /// <param name="bundleName"></param>
    public void MinusOneBundleCount(string bundleName)
    {
        if(m_AssetBundles.TryGetValue(bundleName,out BundleData bundle))
        {
            // 引用计数大于0就减1
            if (bundle.Count > 0)
            {
                bundle.Count--;
                Debug.Log("bundle引用计数: " + bundleName + " count: " + bundle.Count);
            }
            // 如果引用计数<=0就放入对象池中，从Bundle集合中移除
            if (bundle.Count <= 0)
            {
                Debug.Log("放入bundle对象池: " + bundleName);
                Manager.Pool.UnSpawn("AssetBundle", bundleName, bundle.Bundle);
                m_AssetBundles.Remove(bundleName);
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器加载资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    void EditorLoadAsset(string assetName, Action<UObject> action = null)
    {
        Debug.Log("this is EditorLoadAsset");

        UObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(UObject));
        if (obj == null)
            Debug.LogError("assets name is not exist:" + assetName);
        action?.Invoke(obj);
    }
#endif

    /// <summary>
    /// 加载资源的接口
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    private void LoadAsset(string assetName, Action<UObject> action)
    {
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadAsset(assetName, action);
        else
#endif
            StartCoroutine(LoadBundleAsync(assetName, action));
    }

    // 加载各类资源的接口
    public void LoadUI(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetUIPath(assetName), action);
    }

    public void LoadMusic(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetMusicPath(assetName), action);
    }

    public void LoadSound(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetSoundPath(assetName), action);
    }

    public void LoadEffect(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetEffectPath(assetName), action);
    }

    public void LoadScene(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetScenePath(assetName), action);
    }

    public void LoadLua(string assetName, Action<UObject> action = null)
    {
        LoadAsset(assetName, action);
    }
    
    public void LoadPrefab(string assetName, Action<UObject> action = null)
    {
        LoadAsset(assetName, action);
    }

    /// <summary>
    /// 卸载Bundle
    /// </summary>
    /// <param name="name"></param>
    public void UnLoadBundle(UObject obj)
    {
        AssetBundle ab = obj as AssetBundle;
        ab.Unload(true);
    }
}
