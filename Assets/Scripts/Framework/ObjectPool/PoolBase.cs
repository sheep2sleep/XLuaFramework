using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBase : MonoBehaviour
{
    // �Զ��ͷŵ�ʱ��/��
    protected float m_ReleaseTime;

    // �ϴ��ͷ���Դ��ʱ��/��΢�� 1(��)=10000000(��΢��)
    protected long m_LastReleaseTime;

    // �����Ķ����
    protected List<PoolObject> m_Objects;

    public void Start()
    {
        // ��ʼ����ʱ����Ϊ�ϴ��ͷ���Դ��ʱ��
        m_LastReleaseTime = System.DateTime.Now.Ticks;        
    }

    /// <summary>
    /// ��ʼ�������
    /// </summary>
    /// <param name="time"></param>
    public void Init(float time)
    {
        m_ReleaseTime = time;
        m_Objects = new List<PoolObject>();
    }

    /// <summary>
    /// �Ӷ������ȡ������
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
        return null;// û��ȡ������ֱ�ӷ��ؿգ��������ȥ���������
    }

    /// <summary>
    /// ���ն��󵽶������
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public virtual void UnSpawn(string name, Object obj)
    {
        PoolObject po = new PoolObject(name, obj);
        m_Objects.Add(po);
    }

    /// <summary>
    /// �ͷŶ���
    /// </summary>
    public virtual void Release()
    {
        // ������ʲôҲ����
    }

    private void Update()
    {
        // �����ͷ�ʱ���Զ��ͷŶ���
        if (System.DateTime.Now.Ticks - m_LastReleaseTime >= m_ReleaseTime * 10000000)
        {
            m_LastReleaseTime = System.DateTime.Now.Ticks;
            Release();
        }
    }
}
