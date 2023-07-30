using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // 从Manager找到所有管理器
    private static ResourceManager _resource;
    public static ResourceManager Resource
    {
        get { return _resource; }
    }

    private static LuaManager _lua;
    public static LuaManager Lua
    {
        get { return _lua; }
    }

    public void Awake()
    {
        _resource = this.gameObject.AddComponent<ResourceManager>();
        _lua = this.gameObject.AddComponent<LuaManager>();
    }
}
