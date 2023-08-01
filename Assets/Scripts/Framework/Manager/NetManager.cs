using System.Collections.Generic;
using UnityEngine;

public class NetManager : MonoBehaviour
{
    // ����ͻ���
    NetClient m_NetClient;
    // ���������Ϣ�Ķ���
    Queue<KeyValuePair<int, string>> m_MessageQueue = new Queue<KeyValuePair<int, string>>();
    // ���յ���Ϣ���͸�Lua�ķ���
    XLua.LuaFunction ReceiveMessage;

    public void Init()
    {
        m_NetClient = new NetClient();
        ReceiveMessage = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("ReceiveMessage");

    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="message"></param>
    public void SendMessage(int messageId, string message)
    {
        m_NetClient.SendMessage(messageId, message);
    }

    /// <summary>
    /// ���ӷ�����
    /// </summary>
    /// <param name="post"></param>
    /// <param name="port"></param>
    public void ConnectedServer(string post, int port)
    {
        m_NetClient.OnConnectServer(post, port);
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void OnNetConnected()
    {

    }

    /// <summary>
    /// ���������Ͽ�����
    /// </summary>
    public void OnDisConnected()
    {

    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="msgId"></param>
    /// <param name="message"></param>
    public void Receive(int msgId, string message)
    {
        // ������Ϣ������
        m_MessageQueue.Enqueue(new KeyValuePair<int, string>(msgId, message));
    }

    private void Update()
    {
        // �����������Ϣ��ת����Lua
        if (m_MessageQueue.Count > 0)
        {
            KeyValuePair<int, string> msg = m_MessageQueue.Dequeue();
            ReceiveMessage?.Call(msg.Key, msg.Value);
        }
    }
}
