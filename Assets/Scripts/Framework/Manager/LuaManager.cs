using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class LuaManager : MonoBehaviour
{
    // ���е�Lua�ļ���
    public List<string> LuaNames = new List<string>();

    // ����Lua�ű�����
    private Dictionary<string, byte[]> m_LuaScripts;

    // Lua�����
    public LuaEnv LuaEnv;

    Action InitOK;

    public void Init(Action init)
    {
        // ���¼�
        InitOK += init;

        // ��ʼ��
        LuaEnv = new LuaEnv();
        LuaEnv.AddLoader(Loader);
        m_LuaScripts = new Dictionary<string, byte[]>();

        // ����ģʽ����Lua
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadLuaScript();
        else
#endif
            LoadLuaScript();
    }

    /// <summary>
    /// ִ��Lua�ű�
    /// </summary>
    /// <param name="name"></param>
    public void StartLua(string name)
    {
        // ʹ�ý̳��м���Lua�ļ���DoString����
        LuaEnv.DoString(string.Format("require '{0}'", name));
    }

    // �Զ���loader
    byte[] Loader(ref string name)
    {
        return GetLuaScript(name);
    }

    /// <summary>
    /// ��ȡLua�ű�
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private byte[] GetLuaScript(string name)
    {
        // ��lua��дrequire����ʹ��.��Ϊ·���������滻Ϊ/
        name = name.Replace(".", "/");
        string fileName = PathUtil.GetLuaPath(name);

        byte[] luaScript = null;
        // �ж��Ƿ񻺴��������Ѿ��������ֱ�Ӽ��أ����򱨴�
        if (!m_LuaScripts.TryGetValue(fileName, out luaScript))
            Debug.LogError("lua script is not exist: " + fileName);
        return luaScript;
    }

    /// <summary>
    /// ���lua�ļ�����
    /// </summary>
    /// <param name="assetsName"></param>
    /// <param name="luaScript"></param>
    public void AddLuaScript(string assetsName, byte[] luaScript)
    {
        // �ü�ֵ���ǵķ�ʽ��ֹ�ظ����
        m_LuaScripts[assetsName] = luaScript;
    }

    /// <summary>
    /// Bundleģʽ�¼���Lua�ķ���
    /// </summary>
    void LoadLuaScript()
    {
        foreach(string name in LuaNames)
        {
            // ����Դ���������첽���أ�����ֱ��д�ص�
            Manager.Resource.LoadLua(name, (UnityEngine.Object obj) =>
             {
                 // ������ת���ı���ȡ���ֽڼ��뻺����
                 AddLuaScript(name, (obj as TextAsset).bytes);
                 // ������Ϻ����Lua�б�
                 if(m_LuaScripts.Count >= LuaNames.Count)
                 {
                     InitOK?.Invoke();
                     LuaNames.Clear();
                     LuaNames = null;
                 }
             });
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// �༭��ģʽ�¼���Lua�ķ���
    /// </summary>
    void EditorLoadLuaScript()
    {
        // �༭��ģʽ��ֱ�ӴӶ�Ӧ��Lua·���м���
        string[] luaFiles = Directory.GetFiles(PathUtil.LuaPath, "*.bytes", SearchOption.AllDirectories);
        for(int i = 0; i < luaFiles.Length; i++)
        {
            string fileName = PathUtil.GetStandardPath(luaFiles[i]);
            byte[] file = File.ReadAllBytes(fileName);
            // ��ȡ�ļ���ӵ�Lua�ű�������
            AddLuaScript(PathUtil.GetUnityPath(fileName), file);
        }
    }
#endif


    private void Update()
    {
        // ����Lua�ڴ�
        if(LuaEnv != null)
        {
            LuaEnv.Tick();
        }
    }

    private void OnDestroy()
    {
        // �ű��ر�ʱ���������
        if (LuaEnv != null)
        {
            LuaEnv.Dispose();
            LuaEnv = null;
        }
    }
}
