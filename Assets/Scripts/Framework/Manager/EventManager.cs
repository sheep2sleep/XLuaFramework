using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    // 由于lua可以封装多个参数为一个table，所以只需定义一个参数的委托
    public delegate void EventHandler(object args);
    // 定义字典管理所有事件
    Dictionary<int, EventHandler> m_Events = new Dictionary<int, EventHandler>();

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="e"></param>
    public void Subscribe(int id, EventHandler e){
        if (m_Events.ContainsKey(id))// 一个id可以订阅多个事件
            m_Events[id] += e;// 多播委托
        else
            m_Events.Add(id, e);// 单播委托
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="e"></param>
    public void UnSubscribe(int id, EventHandler e)
    {
        if (m_Events.ContainsKey(id))
        {
            // 移除事件或直接移除key
            if (m_Events[id] != null)
                m_Events[id] -= e;
            if (m_Events[id] == null)
                m_Events.Remove(id);
        }
    }

    /// <summary>
    /// 执行事件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="args"></param>
    public void Fire(int id, object args = null)
    {
        EventHandler handler;
        // 查找id是否存在，存在就执行
        if(m_Events.TryGetValue(id,out handler))
        {
            handler(args);
        }
    }

}
