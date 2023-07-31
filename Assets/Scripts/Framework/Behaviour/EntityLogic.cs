using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityLogic : LuaBehaviour
{
    Action m_LuaOnShow;
    Action m_LuaOnHide;

    public override void Init(string luaName)
    {
        base.Init(luaName);
        // ��lua����
        m_ScriptEnv.Get("OnShow", out m_LuaOnShow);
        m_ScriptEnv.Get("OnHide", out m_LuaOnHide);
    }

    // ����Lua��OnShow��OnHide����
    public void OnShow()
    {
        m_LuaOnShow?.Invoke();
    }

    public void OnHide()
    {
        m_LuaOnHide?.Invoke();
    }

    protected override void Clear()
    {
        base.Clear();       
        m_LuaOnShow = null;
        m_LuaOnHide = null;
    }
}
