using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBase : MonoBehaviour
{
    // 自动释放的时间/秒
    protected float m_ReleaseTime;

    // 上次释放资源的时间/毫微秒 1(秒)=10000000(毫微秒)
    protected long m_LastReleaseTime;

    // 真正的对象池
    protected List<PoolObject> m_Objects;

    public void Start()
    {
        // 初始化的时间作为上次释放资源的时间
        m_LastReleaseTime = System.DateTime.Now.Ticks;        
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    /// <param name="time"></param>
    public void Init(float time)
    {
        m_ReleaseTime = time;
        m_Objects = new List<PoolObject>();
    }

    /// <summary>
    /// 从对象池中取出对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual Object Spawn(string name)
    {
        foreach(PoolObject po in m_Objects)
        {
            if(po.Name == name)
            {
                m_Objects.Remove(po);
                return po.Object;
            }
        }
        return null;// 没有取到对象直接返回空，交给外界去创建或回收
    }

    /// <summary>
    /// 回收对象到对象池中
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public virtual void UnSpawn(string name, Object obj)
    {
        PoolObject po = new PoolObject(name, obj);
        m_Objects.Add(po);
    }

    /// <summary>
    /// 释放对象
    /// </summary>
    public virtual void Release()
    {
        // 父类中什么也不做
    }

    private void Update()
    {
        // 超过释放时间自动释放对象
        if (System.DateTime.Now.Ticks - m_LastReleaseTime >= m_ReleaseTime * 10000000)
        {
            m_LastReleaseTime = System.DateTime.Now.Ticks;
            Release();
        }
    }
}
