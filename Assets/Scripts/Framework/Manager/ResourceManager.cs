using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UObject = UnityEngine.Object;


public class ResourceManager : MonoBehaviour
{
    // �ļ���Ϣ��
    internal class BundleInfo
    {
        public string AssetsName;
        public string BundleName;
        public List<string> Dependences;
    }

    internal class BundleData
    {
        public AssetBundle Bundle;
        // ���ü���
        public int Count;
        public BundleData(AssetBundle ab)
        {
            // ���캯��ʱ���ü�������Ϊ1
            Bundle = ab;
            Count = 1;
        }
    }

    // �洢����Bundle��Ϣ�ļ���
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();
    // �洢�����Ѽ��ص�Bundle��Դ
    private Dictionary<string, BundleData> m_AssetBundles = new Dictionary<string, BundleData>();

    /// <summary>
    /// �����汾�ļ�
    /// </summary>
    public void ParseVersionFile()
    {
        // �汾�ļ���·��
        string url = Path.Combine(PathUtil.BundleResourcePath, AppConst.FileListName);
        string[] data = File.ReadAllLines(url);

        // �����ļ���Ϣ
        for(int i=0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split('|');
            bundleInfo.AssetsName = info[0];
            bundleInfo.BundleName = info[1];
            // list���ԣ����������飬���ɶ�̬����
            bundleInfo.Dependences = new List<string>(info.Length - 2);
            for(int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependences.Add(info[j]);
            }
            m_BundleInfos.Add(bundleInfo.AssetsName, bundleInfo);
            //������¼Lua�ļ�
            if (info[0].IndexOf("LuaScripts") > 0)
                Manager.Lua.LuaNames.Add(info[0]);
        }
    }

    /// <summary>
    /// �첽������Դ
    /// </summary>
    /// <param name="assetName">��Դ��</param>
    /// <param name="action">��ɻص�</param>
    /// <returns></returns>
    IEnumerator LoadBundleAsync(string assetName, Action<UObject> action = null)
    {
        // �õ�Bundle���ƺ�·��
        string bundleName = m_BundleInfos[assetName].BundleName;
        string bundlePath = Path.Combine(PathUtil.BundleResourcePath, bundleName);
        // �õ�������Դ
        List<string> dependences = m_BundleInfos[assetName].Dependences;
        if(dependences != null && dependences.Count > 0)
        {
            // �ݹ������������Դ
            for(int i = 0; i<dependences.Count; i++)
            {
                yield return LoadBundleAsync(dependences[i]);
            }
        }

        // ����Bundle
        BundleData bundle = GetBundle(bundleName);
        if(bundle == null)
        {
            // ������д��ھʹӶ������ȡ��Bundle
            UObject obj = Manager.Pool.Spawn("AssetBundle", bundleName);
            if(obj != null)
            {
                AssetBundle ab = obj as AssetBundle;
                bundle = new BundleData(ab);
            }
            else
            {
                // �������û�оʹ���һ��
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
                yield return request;
                bundle = new BundleData(request.assetBundle);                
            }
            m_AssetBundles.Add(bundleName, bundle);
        }
            
        // ������Դ
        if (assetName.EndsWith(".unity"))//������Դ��ͨ���÷�ʽ����
        {
            action?.Invoke(null);
            yield break;
        }

        // ���ص���������Դ������Bundle��Դʱ��ֱ���˳�Э�̣�����Ҫ���ûص�
        if(action == null)
        {
            yield break;
        }

        AssetBundleRequest bundleRequest = bundle.Bundle.LoadAssetAsync(assetName);
        yield return bundleRequest;

        Debug.Log("this is LoadBundleAsync");
        // ���ûص�
        if (action != null && bundleRequest != null)
        {
            action.Invoke(bundleRequest.asset);
        }
    }

    /// <summary>
    /// ��ȡ�����е�Bundle
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
    /// ��ȥbundle�����������ü���
    /// </summary>
    /// <param name="assetName"></param>
    public void MinusBundleCount(string assetName)
    {
        string bundleName = m_BundleInfos[assetName].BundleName;
        MinusOneBundleCount(bundleName);// ��ȥBundle�����ü���
        // ������Դ
        List<string> dependences = m_BundleInfos[assetName].Dependences;
        if (dependences != null)
        {
            foreach(string dependence in dependences)
            {
                string name = m_BundleInfos[dependence].BundleName;
                MinusOneBundleCount(name);// ��ȥ���������ü���
            }
        }
    }

    /// <summary>
    /// ��ȥһ��Bundle�����ü���
    /// </summary>
    /// <param name="bundleName"></param>
    public void MinusOneBundleCount(string bundleName)
    {
        if(m_AssetBundles.TryGetValue(bundleName,out BundleData bundle))
        {
            // ���ü�������0�ͼ�1
            if (bundle.Count > 0)
            {
                bundle.Count--;
                Debug.Log("bundle���ü���: " + bundleName + " count: " + bundle.Count);
            }
            // ������ü���<=0�ͷ��������У���Bundle�������Ƴ�
            if (bundle.Count <= 0)
            {
                Debug.Log("����bundle�����: " + bundleName);
                Manager.Pool.UnSpawn("AssetBundle", bundleName, bundle.Bundle);
                m_AssetBundles.Remove(bundleName);
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// �༭��������Դ
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
    /// ������Դ�Ľӿ�
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

    // ���ظ�����Դ�Ľӿ�
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
    /// ж��Bundle
    /// </summary>
    /// <param name="name"></param>
    public void UnLoadBundle(UObject obj)
    {
        AssetBundle ab = obj as AssetBundle;
        ab.Unload(true);
    }
}
