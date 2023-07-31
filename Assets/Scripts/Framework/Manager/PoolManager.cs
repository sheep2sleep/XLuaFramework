using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 对象池父节点
    Transform m_PoolParent;
    // 对象池字典
    Dictionary<string, PoolBase> m_Pools = new Dictionary<string, PoolBase>();

    private void Awake()
    {
        // 初始化对象池父节点
        m_PoolParent = this.transform.parent.Find("Pool");
    }

    /// <summary>
    /// 创建对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    private void CreatePool<T>(string poolName, float releaseTime) where T : PoolBase
    {
        // 若对象池字典中不存在，就创建一个该对象池父节点，将对象池挂载上去，进行初始化传入释放时间，将对象池加入到字典中
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
    /// 创建物体对象池
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    public void CreateGameObjectPool(string poolName, float releaseTime)
    {
        CreatePool<GameObjectPool>(poolName, releaseTime);
    }

    /// <summary>
    /// 创建资源对象池
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    public void CreateAssetPool(string poolName, float releaseTime)
    {
        CreatePool<AssetPool>(poolName, releaseTime);
    }

    /// <summary>
    /// 取对象接口
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public UnityEngine.Object Spawn(string poolName, string assetName)
    {
        // 从对应对象池中取出对象
        if(m_Pools.TryGetValue(poolName,out PoolBase pool)){
            return pool.Spawn(assetName);
        }
        return null;
    }

    /// <summary>
    /// 回收对象接口
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
