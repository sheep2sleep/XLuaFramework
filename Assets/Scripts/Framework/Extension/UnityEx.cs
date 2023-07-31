using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
public static class UnityEx
{
    /// <summary>
    /// �԰�ť������¼���������չ
    /// </summary>
    /// <param name="button">��Ӧ�İ�ť</param>
    /// <param name="callback">Ҫ�����Ļص�</param>
    public static void OnClickSet(this Button button, object callback)
    {
        // �ѻص�תΪLua��Function
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        // ���ԭ�а�ť�ļ����¼��������¼���һ������ί�У���ί�еĻص���ֱ��ִ��Lua�ķ���
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(
            () =>
            {
                func?.Call();
            });
    }

    /// <summary>
    /// ��Slider��ֵ�ı��¼���������չ
    /// </summary>
    /// <param name="slider">��Ӧ��Slider</param>
    /// <param name="callback">Ҫ�����Ļص�</param>
    public static void OnValueChangedSet(this Slider slider, object callback)
    {
        // �ѻص�תΪLua��Function
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        // ���ԭ��Slider�ļ����¼��������¼���һ������ί�У���ί�еĻص���ֱ��ִ��Lua�ķ���
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(
            (float value) =>// ������C#��߻�ȡ�����ݸ�Lua
            {
                func?.Call(value);
            });
    }
}
