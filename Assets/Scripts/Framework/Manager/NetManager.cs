using System.Collections.Generic;
using UnityEngine;

public class NetManager : MonoBehaviour
{
    // 网络客户端
    NetClient m_NetClient;
    // 定义接收消息的队列
    Queue<KeyValuePair<int, string>> m_MessageQueue = new Queue<KeyValuePair<int, string>>();
    // 接收的消息发送给Lua的方法
    XLua.LuaFunction ReceiveMessage;

    public void Init()
    {
        m_NetClient = new NetClient();
        ReceiveMessage = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("ReceiveMessage");

    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="message"></param>
    public void SendMessage(int messageId, string message)
    {
        m_NetClient.SendMessage(messageId, message);
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="post"></param>
    /// <param name="port"></param>
    public void ConnectedServer(string post, int port)
    {
        m_NetClient.OnConnectServer(post, port);
    }

    /// <summary>
    /// 网络连接
    /// </summary>
    public void OnNetConnected()
    {

    }

    /// <summary>
    /// 被服务器断开连接
    /// </summary>
    public void OnDisConnected()
    {

    }

    /// <summary>
    /// 接受数据
    /// </summary>
    /// <param name="msgId"></param>
    /// <param name="message"></param>
    public void Receive(int msgId, string message)
    {
        // 放入消息队列中
        m_MessageQueue.Enqueue(new KeyValuePair<int, string>(msgId, message));
    }

    private void Update()
    {
        // 如果队列有消息就转发给Lua
        if (m_MessageQueue.Count > 0)
        {
            KeyValuePair<int, string> msg = m_MessageQueue.Dequeue();
            ReceiveMessage?.Call(msg.Key, msg.Value);
        }
    }
}
