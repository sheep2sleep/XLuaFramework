using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetPool : PoolBase
{
    /// <summary>
    /// 取出对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override UnityEngine.Object Spawn(string name)
    {
        return base.Spawn(name);
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public override void UnSpawn(string name, Object obj)
    {
        base.UnSpawn(name, obj);
    }

    /// <summary>
    /// 释放对象
    /// </summary>
    public override void Release()
    {
        base.Release();
        foreach(PoolObject item in m_Objects)
        {
            // 对于AssetBundle对象池中的所有对象，如果超过了释放时间，则卸载Bundle，移出对象池
            if (System.DateTime.Now.Ticks - item.LastUseTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.Log("AssetPool release time: " + System.DateTime.Now + "Unload ab: " + item.Name);
                Manager.Resource.UnLoadBundle(item.Object);// 卸载Bundle
                m_Objects.Remove(item);
                Release();//递归进行Release剩下的对象
                return;
            }
        }
    }
}
