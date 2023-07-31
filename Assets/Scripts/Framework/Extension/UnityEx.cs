using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
public static class UnityEx
{
    /// <summary>
    /// 对按钮做点击事件监听的拓展
    /// </summary>
    /// <param name="button">对应的按钮</param>
    /// <param name="callback">要监听的回调</param>
    public static void OnClickSet(this Button button, object callback)
    {
        // 把回调转为Lua的Function
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        // 清空原有按钮的监听事件，再重新监听一个匿名委托，在委托的回调中直接执行Lua的方法
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(
            () =>
            {
                func?.Call();
            });
    }

    /// <summary>
    /// 对Slider做值改变事件监听的拓展
    /// </summary>
    /// <param name="slider">对应的Slider</param>
    /// <param name="callback">要监听的回调</param>
    public static void OnValueChangedSet(this Slider slider, object callback)
    {
        // 把回调转为Lua的Function
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        // 清空原有Slider的监听事件，再重新监听一个匿名委托，在委托的回调中直接执行Lua的方法
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(
            (float value) =>// 参数从C#这边获取到传递给Lua
            {
                func?.Call(value);
            });
    }
}
