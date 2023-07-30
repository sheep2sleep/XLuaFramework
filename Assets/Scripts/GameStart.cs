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
        // ����Lua����
        Manager.Resource.ParseVersionFile();
        Manager.Lua.Init(() =>
        {
            Manager.Lua.StartLua("Main");

            // ��C#�е���Lua
            //XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
            //func.Call();
        });        
    }
}
