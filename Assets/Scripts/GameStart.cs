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
        // ���¼�ϵͳ�����¼�
        Manager.Event.Subscribe(10000, OnLuaInit);
        // ����Lua����
        Manager.Resource.ParseVersionFile();
        Manager.Lua.Init();        
    }

    void OnLuaInit(object args)
    {
        Manager.Lua.StartLua("Main");

        // ��C#�е���Lua
        XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
        func.Call();

        Manager.Pool.CreateGameObjectPool("UI", 10);
        Manager.Pool.CreateGameObjectPool("Monster", 120);
        Manager.Pool.CreateGameObjectPool("Effect", 120);
        Manager.Pool.CreateAssetPool("AssetBundle", 10);
    }

    private void OnApplicationQuit()
    {
        // �˳���Ϸʱ���¼�ϵͳȡ������
        Manager.Event.UnSubscribe(10000, OnLuaInit);
    }
}
