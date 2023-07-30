using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 缓存UI
    Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();

    // 保存UI分组
    Dictionary<string, Transform> m_UIGroups = new Dictionary<string, Transform>();
    private Transform m_UIParent;

    private void Awake()
    {
        // 找到UI的父节点
        m_UIParent = this.transform.parent.Find("UI");
    }

    /// <summary>
    /// 设置分组
    /// </summary>
    /// <param name="group"></param>
    public void SetUIGroup(List<string> group)
    {
        for(int i=0; i < group.Count; i++)
        {
            // 根据传入的分组名列表实例化对应数量的分组节点并加入到字典中
            GameObject go = new GameObject("Group-" + group[i]);
            go.transform.SetParent(m_UIParent, false);
            m_UIGroups.Add(group[i], go.transform);
        }
    }

    /// <summary>
    /// 获取分组
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    Transform GetUIGroup(string group)
    {
        Transform groupTransform;
        if (!m_UIGroups.TryGetValue(group, out groupTransform))
            Debug.LogError("group is not exist");
        return groupTransform;
    }

    /// <summary>
    /// 打开UI，自动绑定UI名和Lua名执行Lua脚本
    /// </summary>
    /// <param name="uiName"></param>
    /// <param name="luaName"></param>
    public void OpenUI(string uiName, string group, string luaName)
    {
        GameObject ui = null;

        // 如果UI已经存在于缓存中，则直接调用Open不用Init了
        if(m_UI.TryGetValue(uiName,out ui))
        {
            UILogic uILogic = ui.GetComponent<UILogic>();
            uILogic.Open();
            return;
        }

        // 否则还要先Init一次再Open，模仿先Awake后Start的效果
        Manager.Resource.LoadUI(uiName, (UnityEngine.Object obj) =>
        {
            // 实例化UI并将其添加到UI缓存中
            ui = Instantiate(obj) as GameObject;
            m_UI.Add(uiName, ui);

            // 位置设置到分组结点下
            Transform parent = GetUIGroup(group);
            ui.transform.SetParent(parent, false);

            // 调用UILogic中的Init方法，并执行Open
            UILogic uILogic = ui.AddComponent<UILogic>();
            uILogic.Init(luaName);
            uILogic.Open();
        });
    }
}
