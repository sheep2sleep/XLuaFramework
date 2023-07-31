using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameMode GameMode;

    private void Awake()
    {
        AppConst.GameMode = this.GameMode;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        // 用事件系统订阅事件
        Manager.Event.Subscribe(10000, OnLuaInit);
        // 测试Lua加载
        Manager.Resource.ParseVersionFile();
        Manager.Lua.Init();        
    }

    void OnLuaInit(object args)
    {
        Manager.Lua.StartLua("Main");

        // 在C#中调用Lua
        XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
        func.Call();

        Manager.Pool.CreateGameObjectPool("UI", 10);
        Manager.Pool.CreateGameObjectPool("Monster", 120);
        Manager.Pool.CreateGameObjectPool("Effect", 120);
        Manager.Pool.CreateAssetPool("AssetBundle", 10);
    }

    private void OnApplicationQuit()
    {
        // 退出游戏时用事件系统取消订阅
        Manager.Event.UnSubscribe(10000, OnLuaInit);
    }
}
