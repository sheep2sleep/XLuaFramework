using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    // ����lua���Է�װ�������Ϊһ��table������ֻ�趨��һ��������ί��
    public delegate void EventHandler(object args);
    // �����ֵ���������¼�
    Dictionary<int, EventHandler> m_Events = new Dictionary<int, EventHandler>();

    /// <summary>
    /// �����¼�
    /// </summary>
    /// <param name="id"></param>
    /// <param name="e"></param>
    public void Subscribe(int id, EventHandler e){
        if (m_Events.ContainsKey(id))// һ��id���Զ��Ķ���¼�
            m_Events[id] += e;// �ಥί��
        else
            m_Events.Add(id, e);// ����ί��
    }

    /// <summary>
    /// ȡ�������¼�
    /// </summary>
    /// <param name="id"></param>
    /// <param name="e"></param>
    public void UnSubscribe(int id, EventHandler e)
    {
        if (m_Events.ContainsKey(id))
        {
            // �Ƴ��¼���ֱ���Ƴ�key
            if (m_Events[id] != null)
                m_Events[id] -= e;
            if (m_Events[id] == null)
                m_Events.Remove(id);
        }
    }

    /// <summary>
    /// ִ���¼�
    /// </summary>
    /// <param name="id"></param>
    /// <param name="args"></param>
    public void Fire(int id, object args = null)
    {
        EventHandler handler;
        // ����id�Ƿ���ڣ����ھ�ִ��
        if(m_Events.TryGetValue(id,out handler))
        {
            handler(args);
        }
    }

}
