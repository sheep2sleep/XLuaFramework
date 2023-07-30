using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UObject = UnityEngine.Object;

public class HotUpdate : MonoBehaviour
{
    // 只读路径下的FileList文件内容
    byte[] m_ReadPathFileListData;
    // 服务器上的FileList文件内容
    byte[] m_ServerFileListData;

    internal class DownFileInfo
    {
        public string url;
        public string fileName;
        public DownloadHandler fileData;
    }

    /// <summary>
    /// 下载单个文件
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(DownFileInfo info, Action<DownFileInfo> Complete)
    {
        // 下载单个文件
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        yield return webRequest.SendWebRequest();
        // 判断是否下载错误
        if(webRequest.result == UnityWebRequest.Result.ProtocolError 
            || webRequest.result==UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("下载文件出错：" + info.url);
            yield break;
            // TODO:重试下载
        }
        // 调用回调函数，传入文件句柄
        info.fileData = webRequest.downloadHandler;
        Complete?.Invoke(info);
        // 下载完毕释放请求
        webRequest.Dispose();
    }

    /// <summary>
    /// 下载多个文件
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
    /// 获取文件信息
    /// </summary>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileData, string path)
    {
        string content = fileData.Trim().Replace("\r", "");
        // 通过换行符拿到多个文件
        string[] files = content.Split('\n');
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        // 将各个文件都保存到下载文件信息列表中
        for(int i= 0; i<files.Length; i++)
        {
            // 主要是为了获取文件名和文件路径
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
            // 首次安装则释放资源
            ReleaseResources();
        }
        else
        {
            // 非首次安装则直接进行检查更新
            CheckUpdate();
        }   
    }
    
    /// <summary>
    /// 判断是否第一次安装
    /// </summary>
    /// <returns></returns>
    private bool IsFirstInstall()
    {
        // 判断只读目录是否存在版本文件
        bool isExistsReadPath = FileUtil.IsExists(Path.Combine(PathUtil.ReadPath, AppConst.FileListName));
        // 判断可读写目录是否存在版本文件
        bool isExistsReadWritePath = FileUtil.IsExists(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));
        // 前者存在后者不存在时确定为第一次安装
        return isExistsReadPath && !isExistsReadWritePath;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    private void ReleaseResources()
    {
        // 获取到FileList文件
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        // 下载FileList文件
        StartCoroutine(DownLoadFile(info, OnDownLoadReadPathFileListComplete));
    }

    /// <summary>
    /// 下载完FileList文件
    /// </summary>
    /// <param name="file"></param>
    private void OnDownLoadReadPathFileListComplete(DownFileInfo file)
    {
        // 保存FileList文件的字节数组
        m_ReadPathFileListData = file.fileData.data;
        // 解析FileList文件，拿到文件列表
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, PathUtil.ReadPath);
        // 下载文件列表中所有文件
        StartCoroutine(DownLoadFile(fileInfos, OnReleaseFileComplete, OnReleaseAllFileComplete));
    }

    /// <summary>
    /// 所有文件列表文件下载完成
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        // 把FileList文件写入到可读写目录中
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        // 进入检查更新
        CheckUpdate();
    }

    /// <summary>
    /// 文件列表内单个文件下载完成
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnReleaseFileComplete(DownFileInfo fileInfo)
    {
        Debug.Log("OnReleaseFileComplete：" + fileInfo.url);
        // 获取需要写入的文件路径
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        // 执行文件写入
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    private void CheckUpdate()
    {
        // 获取FileList在资源服务器上的地址
        string url = Path.Combine(AppConst.ResourcesUrl, AppConst.FileListName);
        // 下载资源服务器上的FileList文件
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownLoadFile(info, OnDownLoadServerFileListComplete));
    }

    /// <summary>
    /// 服务器上FileList下载完成
    /// </summary>
    /// <param name="obj"></param>
    private void OnDownLoadServerFileListComplete(DownFileInfo file)
    {
        // 保存服务器FileList文件的字节数组
        m_ServerFileListData = file.fileData.data;
        // 获取资源服务器的文件信息
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, AppConst.ResourcesUrl);
        // 定义一个需要下载的文件集合
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();

        // 遍历资源服务器上的文件信息
        for(int i=0; i<fileInfos.Count; i++)
        {
            // 判断本地可读写目录是否存在该服务器文件信息
            string localFile = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            if (!FileUtil.IsExists(localFile))
            {
                // 不存在则将该文件放到需要下载的文件集合中
                fileInfos[i].url = Path.Combine(AppConst.ResourcesUrl, fileInfos[i].fileName);
                downListFiles.Add(fileInfos[i]);
            }
        }

        // 如果有需要下载的文件，启动文件下载
        if (downListFiles.Count > 0)
            StartCoroutine(DownLoadFile(downListFiles, OnUpdateFileComplete, OnUpdateAllFileComplete));
        else
            EnterGame();
    }

    /// <summary>
    /// 所有文件更新完成
    /// </summary>
    private void OnUpdateAllFileComplete()
    {
        // 把服务器FileList文件写入到可读写目录中
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ServerFileListData);
        // 更新完成进入游戏
        EnterGame();
    }

    /// <summary>
    /// 单个文件更新完成
    /// </summary>
    /// <param name="obj"></param>
    private void OnUpdateFileComplete(DownFileInfo fileInfo)
    {
        Debug.Log("OnUpdateFileComplete：" + fileInfo.url);
        // 获取需要写入的文件路径
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        // 执行文件写入
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// 进入游戏
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
