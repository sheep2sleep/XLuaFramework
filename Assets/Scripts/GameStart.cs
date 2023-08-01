using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameMode GameMode;
    public bool OpenLog;

    private void Awake()
    {
        AppConst.GameMode = this.GameMode;
        AppConst.OpenLog = this.OpenLog;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        // 用事件系统订阅事件
        Manager.Event.Subscribe((int)GameEvent.StartLua, StartLua);
        Manager.Event.Subscribe((int)GameEvent.GameInit, GameInit);

        if (AppConst.GameMode == GameMode.UpdateMode)
            this.gameObject.AddComponent<HotUpdate>();
        else
            //更新完成后进行初始化
            Manager.Event.Fire((int)GameEvent.GameInit);
    }

    /// <summary>
    /// 游戏初始化
    /// </summary>
    /// <param name="args"></param>
    private void GameInit(object args)
    {
        // 解析版本文件
        if (AppConst.GameMode != GameMode.EditorMode)
            Manager.Resource.ParseVersionFile();
        Manager.Lua.Init();
    }

    void StartLua(object args)
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
        Manager.Event.UnSubscribe((int)GameEvent.StartLua, StartLua);
        Manager.Event.UnSubscribe((int)GameEvent.GameInit, GameInit);
    }
}
