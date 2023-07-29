using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildTool : Editor
{
    [MenuItem("Tools/Build Windows Bundle")]
    static void BundleWindowsBuild()
    {
        Build(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Tools/Build Android Bundle")]
    static void BundleAndroidBuild()
    {
        Build(BuildTarget.Android);
    }
    
    [MenuItem("Tools/Build iOS Bundle")]
    static void BundleiOSBuild()
    {
        Build(BuildTarget.iOS);
    }

    static void Build(BuildTarget target)
    {
        // ������Bundle���б�
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();

        // ��¼����ļ�����������Ҫ������ļ�
        string[] files = Directory.GetFiles(PathUtil.BuildResourcesPath, "*", SearchOption.AllDirectories);
            // �ų�meta�ļ�
        for(int i=0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta"))
                continue;
            AssetBundleBuild assetBundle = new AssetBundleBuild();

            string fileName = PathUtil.GetStandardPath(files[i]);
            Debug.Log("file:" + fileName);

            string assetName = PathUtil.GetUnityPath(fileName);
            assetBundle.assetNames = new string[] { assetName };// ��Դ���Ŀ¼
            string bundleName = fileName.Replace(PathUtil.BuildResourcesPath, "").ToLower();
            assetBundle.assetBundleName = bundleName + ".ab";// bundleĿ¼����

            assetBundleBuilds.Add(assetBundle);
        }

        // ���������ļ��У����½�����ļ���
        if (Directory.Exists(PathUtil.BundleOutPath))
            Directory.Delete(PathUtil.BundleOutPath, true);
        Directory.CreateDirectory(PathUtil.BundleOutPath);

        // ����Bundle��
        BuildPipeline.BuildAssetBundles(PathUtil.BundleOutPath, assetBundleBuilds.ToArray(), 
            BuildAssetBundleOptions.None, target);
    }
}
