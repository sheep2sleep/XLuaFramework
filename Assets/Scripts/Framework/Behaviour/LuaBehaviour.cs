using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour
{
    // Lua的虚拟机
    private LuaEnv m_LuaEnv = Manager.Lua.LuaEnv;
    // 脚本的运行环境
    protected LuaTable m_ScriptEnv;

    // Lua对应方法的委托
    private Action m_LuaInit;
    private Action m_LuaUpdate;
    private Action m_LuaOnDestroy;

    // Lua文件名
    public string luaName;

    private void Awake()
    {
        m_ScriptEnv = m_LuaEnv.NewTable();

        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        LuaTable meta = m_LuaEnv.NewTable();
        meta.Set("__index", m_LuaEnv.Global);
        m_ScriptEnv.SetMetaTable(meta);
        meta.Dispose();

        // 把本脚本作为一个self变量注入到Lua中，Lua直接通过self来使用
        m_ScriptEnv.Set("self", this);        
    }

    public virtual void Init(string luaName)
    {
        // 把Lua脚本绑定到当前的这个C#脚本运行环境m_ScriptEnv中
        m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName), luaName, m_ScriptEnv);

        // 绑定Lua方法
        m_ScriptEnv.Get("OnInit", out m_LuaInit);
        m_ScriptEnv.Get("Update", out m_LuaUpdate);
        m_ScriptEnv.Get("OnDestroy", out m_LuaOnDestroy);

        // 执行Init的回调
        m_LuaInit?.Invoke();
    }


    // Update is called once per frame
    void Update()
    {
        m_LuaUpdate?.Invoke();
    }

    protected virtual void Clear()
    {
        // 释放委托和环境       
        m_LuaInit = null;
        m_LuaUpdate = null;
        m_LuaOnDestroy = null;
        m_ScriptEnv?.Dispose();
        m_ScriptEnv = null;
    }

    private void OnDestroy()
    {
        m_LuaOnDestroy?.Invoke();
        Clear();
    }

    private void OnApplicationQuit()
    {
        Clear();
    }
}
