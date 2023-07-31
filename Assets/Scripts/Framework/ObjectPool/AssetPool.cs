using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetPool : PoolBase
{
    /// <summary>
    /// ȡ������
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override UnityEngine.Object Spawn(string name)
    {
        return base.Spawn(name);
    }

    /// <summary>
    /// ���ն���
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public override void UnSpawn(string name, Object obj)
    {
        base.UnSpawn(name, obj);
    }

    /// <summary>
    /// �ͷŶ���
    /// </summary>
    public override void Release()
    {
        base.Release();
        foreach(PoolObject item in m_Objects)
        {
            // ����AssetBundle������е����ж�������������ͷ�ʱ�䣬��ж��Bundle���Ƴ������
            if (System.DateTime.Now.Ticks - item.LastUseTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.Log("AssetPool release time: " + System.DateTime.Now + "Unload ab: " + item.Name);
                Manager.Resource.UnLoadBundle(item.Object);// ж��Bundle
                m_Objects.Remove(item);
                Release();//�ݹ����Releaseʣ�µĶ���
                return;
            }
        }
    }
}
