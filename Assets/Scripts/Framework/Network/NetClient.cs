using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetClient
{
    // 基于TCP封装好的一个给客户端使用的Socket
    private TcpClient m_Client;
    // 可以从TcpClient中获取到的网络数据流
    private NetworkStream m_TcpStream;
    // 接收数据的大小
    private const int BufferSize = 1024 * 64;
    // 接收数据的数据缓存区
    private byte[] m_Buffer = new byte[BufferSize];
    // 用于解析接收到的数据
    private MemoryStream m_MemStream;
    private BinaryReader m_BinaryReader;

    public NetClient()
    {
        m_MemStream = new MemoryStream();
        m_BinaryReader = new BinaryReader(m_MemStream);
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public void OnConnectServer(string host, int port)
    {
        try
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if(addresses.Length == 0)
            {
                Debug.LogError("host invalid");
                return;
            }
            if (addresses[0].AddressFamily == AddressFamily.InterNetworkV6)
                m_Client = new TcpClient(AddressFamily.InterNetworkV6);// IPV6的环境
            else
                m_Client = new TcpClient(AddressFamily.InterNetwork);// IPV4的环境
            m_Client.SendTimeout = 1000;
            m_Client.ReceiveTimeout = 1000;
            m_Client.NoDelay = true;
            // 开始连接
            m_Client.BeginConnect(host, port, OnConnect, null);
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// 连接的回调
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnConnect(IAsyncResult asyncResult)
    {
        if(m_Client == null || !m_Client.Connected)
        {
            Debug.LogError("connect server error!!!");
            return;
        }
        Manager.Net.OnNetConnected();
        // 连接成功后获取网络流
        m_TcpStream = m_Client.GetStream();
        // 开始接收数据到缓存区
        m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
    }

    /// <summary>
    /// 接收到数据的回调
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnRead(IAsyncResult asyncResult)
    {
        try
        {
            if (m_Client == null || m_TcpStream == null)
                return;
            // 收到的消息长度
            int length = m_TcpStream.EndRead(asyncResult);
            if (length < 1)
            {
                // 没有接受到数据就断开连接
                OnDisConnected();
                return;
            }
            // 否则就正常解析数据
            ReceiveData(length);
            lock (m_TcpStream)
            {
                // 清空Buffer准备下次接收
                Array.Clear(m_Buffer, 0, m_Buffer.Length);
                m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            OnDisConnected();
        }
    }

    /// <summary>
    /// 解析数据
    /// </summary>
    private void ReceiveData(int len)
    {
        // 把数据追加到末尾
        m_MemStream.Seek(0, SeekOrigin.End);
        m_MemStream.Write(m_Buffer, 0, len);
        m_MemStream.Seek(0, SeekOrigin.Begin);
        // 剩余字节数超过消息id和消息长度的长度时
        while (RemainingBytesLength() >= 8)
        {
            // 每一条消息都分为三部分：消息id、消息长度、消息体
            int msgId = m_BinaryReader.ReadInt32();
            int msgLen = m_BinaryReader.ReadInt32();
            // 剩余长度大于msgLen说明消息是完整的
            if (RemainingBytesLength() >= msgLen)
            {
                // 读取完整的消息转为字符串传给Lua
                byte[] data = m_BinaryReader.ReadBytes(msgLen);
                string message = System.Text.Encoding.UTF8.GetString(data);

                // 转到Lua
                Manager.Net.Receive(msgId, message);
            }
            else
            {
                // 不是完整消息就还回去消息id和消息长度的字节，因为可能是上一次没读完的数据
                m_MemStream.Position = m_MemStream.Position - 8;
                break;
            }
            // 剩余字节重新写回stream中
            byte[] leftover = m_BinaryReader.ReadBytes(RemainingBytesLength());
            m_MemStream.SetLength(0);
            m_MemStream.Write(leftover, 0, leftover.Length);
        }
    }

    /// <summary>
    /// 获取消息剩余字节长度
    /// </summary>
    /// <returns></returns>
    private int RemainingBytesLength()
    {
        return (int)(m_MemStream.Length - m_MemStream.Position);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="msgId"></param>
    /// <param name="message"></param>
    public void SendMessage(int msgId, string message)
    {
        using(MemoryStream ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter bw = new BinaryWriter(ms);
            // 将json数据转换为字节数组
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            // 协议ID
            bw.Write(msgId);
            // 消息长度
            bw.Write((int)data.Length);
            // 消息内容
            bw.Write(data);
            bw.Flush();
            if(m_Client!= null && m_Client.Connected)
            {
                // TCP是正常连接的就调用方法开始写入
                byte[] sendData = ms.ToArray();
                m_TcpStream.BeginWrite(sendData, 0, sendData.Length, OnEndSend, null);
            }
            else
            {
                Debug.LogError("服务器未连接");
            }
        }
    }

    /// <summary>
    /// 发送消息成功后的回调
    /// </summary>
    /// <param name="ar"></param>
    void OnEndSend(IAsyncResult ar)
    {
        try
        {
            // 结束发送
            m_TcpStream.EndWrite(ar);
        }
        catch(Exception ex)
        {
            OnDisConnected();
            Debug.LogError(ex.Message);
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void OnDisConnected()
    {
        if(m_Client != null && m_Client.Connected)
        {
            m_Client.Close();
            m_Client = null;

            m_TcpStream.Close();
            m_TcpStream = null;
        }
        Manager.Net.OnDisConnected();
    }
}
