using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : PoolBase
{
    /// <summary>
    /// 取出对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override Object Spawn(string name)
    {
        Object obj = base.Spawn(name);
        if (obj == null)
            return null;
        // 取到了对象就将其设为激活，并返回
        GameObject go = obj as GameObject;
        go.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public override void UnSpawn(string name, Object obj)
    {
        // 将对象设置为不激活状态，移动到当前脚本结点下，放入对象池
        GameObject go = obj as GameObject;
        go.SetActive(false);
        go.transform.SetParent(this.transform, false);
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
            // 对于GameObject对象池中的所有对象，如果超过了释放时间，则销毁GameObject，移出对象池
            if(System.DateTime.Now.Ticks - item.LastUseTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.Log("GameObjectPool release time: " + System.DateTime.Now);
                Destroy(item.Object);// 销毁对象
                Manager.Resource.MinusBundleCount(item.Name);// 引用计数-1
                m_Objects.Remove(item);
                Release();//递归进行Release剩下的对象
                return;
            }
        }
    }
}
