using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour
{
    // Lua�������
    private LuaEnv m_LuaEnv = Manager.Lua.LuaEnv;
    // �ű������л���
    protected LuaTable m_ScriptEnv;

    // Lua��Ӧ������ί��
    private Action m_LuaInit;
    private Action m_LuaUpdate;
    private Action m_LuaOnDestroy;

    // Lua�ļ���
    public string luaName;

    private void Awake()
    {
        m_ScriptEnv = m_LuaEnv.NewTable();

        // Ϊÿ���ű�����һ�������Ļ�������һ���̶��Ϸ�ֹ�ű���ȫ�ֱ�����������ͻ
        LuaTable meta = m_LuaEnv.NewTable();
        meta.Set("__index", m_LuaEnv.Global);
        m_ScriptEnv.SetMetaTable(meta);
        meta.Dispose();

        // �ѱ��ű���Ϊһ��self����ע�뵽Lua�У�Luaֱ��ͨ��self��ʹ��
        m_ScriptEnv.Set("self", this);        
    }

    public virtual void Init(string luaName)
    {
        // ��Lua�ű��󶨵���ǰ�����C#�ű����л���m_ScriptEnv��
        m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName), luaName, m_ScriptEnv);

        // ��Lua����
        m_ScriptEnv.Get("OnInit", out m_LuaInit);
        m_ScriptEnv.Get("Update", out m_LuaUpdate);
        m_ScriptEnv.Get("OnDestroy", out m_LuaOnDestroy);

        // ִ��Init�Ļص�
        m_LuaInit?.Invoke();
    }


    // Update is called once per frame
    void Update()
    {
        m_LuaUpdate?.Invoke();
    }

    protected virtual void Clear()
    {
        // �ͷ�ί�кͻ���       
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
