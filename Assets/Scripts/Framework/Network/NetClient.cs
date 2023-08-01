using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetClient
{
    // ����TCP��װ�õ�һ�����ͻ���ʹ�õ�Socket
    private TcpClient m_Client;
    // ���Դ�TcpClient�л�ȡ��������������
    private NetworkStream m_TcpStream;
    // �������ݵĴ�С
    private const int BufferSize = 1024 * 64;
    // �������ݵ����ݻ�����
    private byte[] m_Buffer = new byte[BufferSize];
    // ���ڽ������յ�������
    private MemoryStream m_MemStream;
    private BinaryReader m_BinaryReader;

    public NetClient()
    {
        m_MemStream = new MemoryStream();
        m_BinaryReader = new BinaryReader(m_MemStream);
    }

    /// <summary>
    /// ���ӷ�����
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
                m_Client = new TcpClient(AddressFamily.InterNetworkV6);// IPV6�Ļ���
            else
                m_Client = new TcpClient(AddressFamily.InterNetwork);// IPV4�Ļ���
            m_Client.SendTimeout = 1000;
            m_Client.ReceiveTimeout = 1000;
            m_Client.NoDelay = true;
            // ��ʼ����
            m_Client.BeginConnect(host, port, OnConnect, null);
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// ���ӵĻص�
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
        // ���ӳɹ����ȡ������
        m_TcpStream = m_Client.GetStream();
        // ��ʼ�������ݵ�������
        m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
    }

    /// <summary>
    /// ���յ����ݵĻص�
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnRead(IAsyncResult asyncResult)
    {
        try
        {
            if (m_Client == null || m_TcpStream == null)
                return;
            // �յ�����Ϣ����
            int length = m_TcpStream.EndRead(asyncResult);
            if (length < 1)
            {
                // û�н��ܵ����ݾͶϿ�����
                OnDisConnected();
                return;
            }
            // �����������������
            ReceiveData(length);
            lock (m_TcpStream)
            {
                // ���Buffer׼���´ν���
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
    /// ��������
    /// </summary>
    private void ReceiveData(int len)
    {
        // ������׷�ӵ�ĩβ
        m_MemStream.Seek(0, SeekOrigin.End);
        m_MemStream.Write(m_Buffer, 0, len);
        m_MemStream.Seek(0, SeekOrigin.Begin);
        // ʣ���ֽ���������Ϣid����Ϣ���ȵĳ���ʱ
        while (RemainingBytesLength() >= 8)
        {
            // ÿһ����Ϣ����Ϊ�����֣���Ϣid����Ϣ���ȡ���Ϣ��
            int msgId = m_BinaryReader.ReadInt32();
            int msgLen = m_BinaryReader.ReadInt32();
            // ʣ�೤�ȴ���msgLen˵����Ϣ��������
            if (RemainingBytesLength() >= msgLen)
            {
                // ��ȡ��������ϢתΪ�ַ�������Lua
                byte[] data = m_BinaryReader.ReadBytes(msgLen);
                string message = System.Text.Encoding.UTF8.GetString(data);

                // ת��Lua
                Manager.Net.Receive(msgId, message);
            }
            else
            {
                // ����������Ϣ�ͻ���ȥ��Ϣid����Ϣ���ȵ��ֽڣ���Ϊ��������һ��û���������
                m_MemStream.Position = m_MemStream.Position - 8;
                break;
            }
            // ʣ���ֽ�����д��stream��
            byte[] leftover = m_BinaryReader.ReadBytes(RemainingBytesLength());
            m_MemStream.SetLength(0);
            m_MemStream.Write(leftover, 0, leftover.Length);
        }
    }

    /// <summary>
    /// ��ȡ��Ϣʣ���ֽڳ���
    /// </summary>
    /// <returns></returns>
    private int RemainingBytesLength()
    {
        return (int)(m_MemStream.Length - m_MemStream.Position);
    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <param name="msgId"></param>
    /// <param name="message"></param>
    public void SendMessage(int msgId, string message)
    {
        using(MemoryStream ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter bw = new BinaryWriter(ms);
            // ��json����ת��Ϊ�ֽ�����
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            // Э��ID
            bw.Write(msgId);
            // ��Ϣ����
            bw.Write((int)data.Length);
            // ��Ϣ����
            bw.Write(data);
            bw.Flush();
            if(m_Client!= null && m_Client.Connected)
            {
                // TCP���������ӵľ͵��÷�����ʼд��
                byte[] sendData = ms.ToArray();
                m_TcpStream.BeginWrite(sendData, 0, sendData.Length, OnEndSend, null);
            }
            else
            {
                Debug.LogError("������δ����");
            }
        }
    }

    /// <summary>
    /// ������Ϣ�ɹ���Ļص�
    /// </summary>
    /// <param name="ar"></param>
    void OnEndSend(IAsyncResult ar)
    {
        try
        {
            // ��������
            m_TcpStream.EndWrite(ar);
        }
        catch(Exception ex)
        {
            OnDisConnected();
            Debug.LogError(ex.Message);
        }
    }

    /// <summary>
    /// �Ͽ�����
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
