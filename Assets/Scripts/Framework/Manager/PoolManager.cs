using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // ����ظ��ڵ�
    Transform m_PoolParent;
    // ������ֵ�
    Dictionary<string, PoolBase> m_Pools = new Dictionary<string, PoolBase>();

    private void Awake()
    {
        // ��ʼ������ظ��ڵ�
        m_PoolParent = this.transform.parent.Find("Pool");
    }

    /// <summary>
    /// ���������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    private void CreatePool<T>(string poolName, float releaseTime) where T : PoolBase
    {
        // ��������ֵ��в����ڣ��ʹ���һ���ö���ظ��ڵ㣬������ع�����ȥ�����г�ʼ�������ͷ�ʱ�䣬������ؼ��뵽�ֵ���
        if(!m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            GameObject go = new GameObject(poolName);
            go.transform.SetParent(m_PoolParent);
            pool = go.AddComponent<T>();
            pool.Init(releaseTime);
            m_Pools.Add(poolName, pool);
        }
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    public void CreateGameObjectPool(string poolName, float releaseTime)
    {
        CreatePool<GameObjectPool>(poolName, releaseTime);
    }

    /// <summary>
    /// ������Դ�����
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    public void CreateAssetPool(string poolName, float releaseTime)
    {
        CreatePool<AssetPool>(poolName, releaseTime);
    }

    /// <summary>
    /// ȡ����ӿ�
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public UnityEngine.Object Spawn(string poolName, string assetName)
    {
        // �Ӷ�Ӧ�������ȡ������
        if(m_Pools.TryGetValue(poolName,out PoolBase pool)){
            return pool.Spawn(assetName);
        }
        return null;
    }

    /// <summary>
    /// ���ն���ӿ�
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="assetName"></param>
    /// <param name="asset"></param>
    public void UnSpawn(string poolName, string assetName, Object asset)
    {
        if(m_Pools.TryGetValue(poolName, out PoolBase pool)){
            pool.UnSpawn(assetName, asset);
        }
    }
}
