using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class LuaManager : MonoBehaviour
{
    // 所有的Lua文件名
    public List<string> LuaNames = new List<string>();

    // 缓存Lua脚本内容
    private Dictionary<string, byte[]> m_LuaScripts;

    // Lua虚拟机
    public LuaEnv LuaEnv;

    Action InitOK;

    public void Init(Action init)
    {
        // 绑定事件
        InitOK += init;

        // 初始化
        LuaEnv = new LuaEnv();
        LuaEnv.AddLoader(Loader);
        m_LuaScripts = new Dictionary<string, byte[]>();

        // 根据模式加载Lua
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadLuaScript();
        else
#endif
            LoadLuaScript();
    }

    /// <summary>
    /// 执行Lua脚本
    /// </summary>
    /// <param name="name"></param>
    public void StartLua(string name)
    {
        // 使用教程中加载Lua文件的DoString方法
        LuaEnv.DoString(string.Format("require '{0}'", name));
    }

    // 自定义loader
    byte[] Loader(ref string name)
    {
        return GetLuaScript(name);
    }

    /// <summary>
    /// 获取Lua脚本
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private byte[] GetLuaScript(string name)
    {
        // 在lua中写require经常使用.作为路径，将其替换为/
        name = name.Replace(".", "/");
        string fileName = PathUtil.GetLuaPath(name);

        byte[] luaScript = null;
        // 判断是否缓存过，如果已经缓存过则直接加载，否则报错
        if (!m_LuaScripts.TryGetValue(fileName, out luaScript))
            Debug.LogError("lua script is not exist: " + fileName);
        return luaScript;
    }

    /// <summary>
    /// 添加lua文件缓存
    /// </summary>
    /// <param name="assetsName"></param>
    /// <param name="luaScript"></param>
    public void AddLuaScript(string assetsName, byte[] luaScript)
    {
        // 用键值覆盖的方式防止重复添加
        m_LuaScripts[assetsName] = luaScript;
    }

    /// <summary>
    /// Bundle模式下加载Lua的方法
    /// </summary>
    void LoadLuaScript()
    {
        foreach(string name in LuaNames)
        {
            // 在资源管理器中异步加载，这里直接写回调
            Manager.Resource.LoadLua(name, (UnityEngine.Object obj) =>
             {
                 // 将对象转成文本再取出字节加入缓存中
                 AddLuaScript(name, (obj as TextAsset).bytes);
                 // 缓存完毕后清空Lua列表
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
    /// 编辑器模式下加载Lua的方法
    /// </summary>
    void EditorLoadLuaScript()
    {
        // 编辑器模式下直接从对应的Lua路径中加载
        string[] luaFiles = Directory.GetFiles(PathUtil.LuaPath, "*.bytes", SearchOption.AllDirectories);
        for(int i = 0; i < luaFiles.Length; i++)
        {
            string fileName = PathUtil.GetStandardPath(luaFiles[i]);
            byte[] file = File.ReadAllBytes(fileName);
            // 读取文件添加到Lua脚本缓存中
            AddLuaScript(PathUtil.GetUnityPath(fileName), file);
        }
    }
#endif


    private void Update()
    {
        // 回收Lua内存
        if(LuaEnv != null)
        {
            LuaEnv.Tick();
        }
    }

    private void OnDestroy()
    {
        // 脚本关闭时销毁虚拟机
        if (LuaEnv != null)
        {
            LuaEnv.Dispose();
            LuaEnv = null;
        }
    }
}
