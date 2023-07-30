using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UObject = UnityEngine.Object;

public class HotUpdate : MonoBehaviour
{
    // ֻ��·���µ�FileList�ļ�����
    byte[] m_ReadPathFileListData;
    // �������ϵ�FileList�ļ�����
    byte[] m_ServerFileListData;

    internal class DownFileInfo
    {
        public string url;
        public string fileName;
        public DownloadHandler fileData;
    }

    /// <summary>
    /// ���ص����ļ�
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(DownFileInfo info, Action<DownFileInfo> Complete)
    {
        // ���ص����ļ�
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        yield return webRequest.SendWebRequest();
        // �ж��Ƿ����ش���
        if(webRequest.result == UnityWebRequest.Result.ProtocolError 
            || webRequest.result==UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("�����ļ�����" + info.url);
            yield break;
            // TODO:��������
        }
        // ���ûص������������ļ����
        info.fileData = webRequest.downloadHandler;
        Complete?.Invoke(info);
        // ��������ͷ�����
        webRequest.Dispose();
    }

    /// <summary>
    /// ���ض���ļ�
    /// </summary>
    /// <param name="infos"></param>
    /// <param name="Complete"></param>
    /// <param name="DownLoadAllComplete"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(List<DownFileInfo> infos, Action<DownFileInfo> Complete, Action DownLoadAllComplete)
    {
        foreach(DownFileInfo info in infos)
        {
            yield return DownLoadFile(info, Complete);
        }
        DownLoadAllComplete?.Invoke();
    }

    /// <summary>
    /// ��ȡ�ļ���Ϣ
    /// </summary>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileData, string path)
    {
        string content = fileData.Trim().Replace("\r", "");
        // ͨ�����з��õ�����ļ�
        string[] files = content.Split('\n');
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        // �������ļ������浽�����ļ���Ϣ�б���
        for(int i= 0; i<files.Length; i++)
        {
            // ��Ҫ��Ϊ�˻�ȡ�ļ������ļ�·��
            string[] info = files[i].Split('|');
            DownFileInfo fileInfo = new DownFileInfo();
            fileInfo.fileName = info[1];
            fileInfo.url = Path.Combine(path, info[1]);
            downFileInfos.Add(fileInfo);
        }
        return downFileInfos;
    }

    private void Start()
    {
        if (IsFirstInstall())
        {
            // �״ΰ�װ���ͷ���Դ
            ReleaseResources();
        }
        else
        {
            // ���״ΰ�װ��ֱ�ӽ��м�����
            CheckUpdate();
        }   
    }
    
    /// <summary>
    /// �ж��Ƿ��һ�ΰ�װ
    /// </summary>
    /// <returns></returns>
    private bool IsFirstInstall()
    {
        // �ж�ֻ��Ŀ¼�Ƿ���ڰ汾�ļ�
        bool isExistsReadPath = FileUtil.IsExists(Path.Combine(PathUtil.ReadPath, AppConst.FileListName));
        // �жϿɶ�дĿ¼�Ƿ���ڰ汾�ļ�
        bool isExistsReadWritePath = FileUtil.IsExists(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));
        // ǰ�ߴ��ں��߲�����ʱȷ��Ϊ��һ�ΰ�װ
        return isExistsReadPath && !isExistsReadWritePath;
    }

    /// <summary>
    /// �ͷ���Դ
    /// </summary>
    private void ReleaseResources()
    {
        // ��ȡ��FileList�ļ�
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        // ����FileList�ļ�
        StartCoroutine(DownLoadFile(info, OnDownLoadReadPathFileListComplete));
    }

    /// <summary>
    /// ������FileList�ļ�
    /// </summary>
    /// <param name="file"></param>
    private void OnDownLoadReadPathFileListComplete(DownFileInfo file)
    {
        // ����FileList�ļ����ֽ�����
        m_ReadPathFileListData = file.fileData.data;
        // ����FileList�ļ����õ��ļ��б�
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, PathUtil.ReadPath);
        // �����ļ��б��������ļ�
        StartCoroutine(DownLoadFile(fileInfos, OnReleaseFileComplete, OnReleaseAllFileComplete));
    }

    /// <summary>
    /// �����ļ��б��ļ��������
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        // ��FileList�ļ�д�뵽�ɶ�дĿ¼��
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        // ���������
        CheckUpdate();
    }

    /// <summary>
    /// �ļ��б��ڵ����ļ��������
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnReleaseFileComplete(DownFileInfo fileInfo)
    {
        Debug.Log("OnReleaseFileComplete��" + fileInfo.url);
        // ��ȡ��Ҫд����ļ�·��
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        // ִ���ļ�д��
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// ������
    /// </summary>
    private void CheckUpdate()
    {
        // ��ȡFileList����Դ�������ϵĵ�ַ
        string url = Path.Combine(AppConst.ResourcesUrl, AppConst.FileListName);
        // ������Դ�������ϵ�FileList�ļ�
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownLoadFile(info, OnDownLoadServerFileListComplete));
    }

    /// <summary>
    /// ��������FileList�������
    /// </summary>
    /// <param name="obj"></param>
    private void OnDownLoadServerFileListComplete(DownFileInfo file)
    {
        // ���������FileList�ļ����ֽ�����
        m_ServerFileListData = file.fileData.data;
        // ��ȡ��Դ���������ļ���Ϣ
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, AppConst.ResourcesUrl);
        // ����һ����Ҫ���ص��ļ�����
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();

        // ������Դ�������ϵ��ļ���Ϣ
        for(int i=0; i<fileInfos.Count; i++)
        {
            // �жϱ��ؿɶ�дĿ¼�Ƿ���ڸ÷������ļ���Ϣ
            string localFile = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            if (!FileUtil.IsExists(localFile))
            {
                // �������򽫸��ļ��ŵ���Ҫ���ص��ļ�������
                fileInfos[i].url = Path.Combine(AppConst.ResourcesUrl, fileInfos[i].fileName);
                downListFiles.Add(fileInfos[i]);
            }
        }

        // �������Ҫ���ص��ļ��������ļ�����
        if (downListFiles.Count > 0)
            StartCoroutine(DownLoadFile(downListFiles, OnUpdateFileComplete, OnUpdateAllFileComplete));
        else
            EnterGame();
    }

    /// <summary>
    /// �����ļ��������
    /// </summary>
    private void OnUpdateAllFileComplete()
    {
        // �ѷ�����FileList�ļ�д�뵽�ɶ�дĿ¼��
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ServerFileListData);
        // ������ɽ�����Ϸ
        EnterGame();
    }

    /// <summary>
    /// �����ļ��������
    /// </summary>
    /// <param name="obj"></param>
    private void OnUpdateFileComplete(DownFileInfo fileInfo)
    {
        Debug.Log("OnUpdateFileComplete��" + fileInfo.url);
        // ��ȡ��Ҫд����ļ�·��
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        // ִ���ļ�д��
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// ������Ϸ
    /// </summary>
    private void EnterGame()
    {
        Manager.Resource.ParseVersionFile();
        Manager.Resource.LoadUI("Login/LoginUI", OnComplete);
    }

    private void OnComplete(UObject obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}
