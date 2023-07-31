using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : PoolBase
{
    /// <summary>
    /// ȡ������
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override Object Spawn(string name)
    {
        Object obj = base.Spawn(name);
        if (obj == null)
            return null;
        // ȡ���˶���ͽ�����Ϊ���������
        GameObject go = obj as GameObject;
        go.SetActive(true);
        return obj;
    }

    /// <summary>
    /// ���ն���
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public override void UnSpawn(string name, Object obj)
    {
        // ����������Ϊ������״̬���ƶ�����ǰ�ű�����£���������
        GameObject go = obj as GameObject;
        go.SetActive(false);
        go.transform.SetParent(this.transform, false);
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
            // ����GameObject������е����ж�������������ͷ�ʱ�䣬������GameObject���Ƴ������
            if(System.DateTime.Now.Ticks - item.LastUseTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.Log("GameObjectPool release time: " + System.DateTime.Now);
                Destroy(item.Object);// ���ٶ���
                Manager.Resource.MinusBundleCount(item.Name);// ���ü���-1
                m_Objects.Remove(item);
                Release();//�ݹ����Releaseʣ�µĶ���
                return;
            }
        }
    }
}
