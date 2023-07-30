using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // ����UI
    Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();

    // ����UI����
    Dictionary<string, Transform> m_UIGroups = new Dictionary<string, Transform>();
    private Transform m_UIParent;

    private void Awake()
    {
        // �ҵ�UI�ĸ��ڵ�
        m_UIParent = this.transform.parent.Find("UI");
    }

    /// <summary>
    /// ���÷���
    /// </summary>
    /// <param name="group"></param>
    public void SetUIGroup(List<string> group)
    {
        for(int i=0; i < group.Count; i++)
        {
            // ���ݴ���ķ������б�ʵ������Ӧ�����ķ���ڵ㲢���뵽�ֵ���
            GameObject go = new GameObject("Group-" + group[i]);
            go.transform.SetParent(m_UIParent, false);
            m_UIGroups.Add(group[i], go.transform);
        }
    }

    /// <summary>
    /// ��ȡ����
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
    /// ��UI���Զ���UI����Lua��ִ��Lua�ű�
    /// </summary>
    /// <param name="uiName"></param>
    /// <param name="luaName"></param>
    public void OpenUI(string uiName, string group, string luaName)
    {
        GameObject ui = null;

        // ���UI�Ѿ������ڻ����У���ֱ�ӵ���Open����Init��
        if(m_UI.TryGetValue(uiName,out ui))
        {
            UILogic uILogic = ui.GetComponent<UILogic>();
            uILogic.Open();
            return;
        }

        // ����Ҫ��Initһ����Open��ģ����Awake��Start��Ч��
        Manager.Resource.LoadUI(uiName, (UnityEngine.Object obj) =>
        {
            // ʵ����UI��������ӵ�UI������
            ui = Instantiate(obj) as GameObject;
            m_UI.Add(uiName, ui);

            // λ�����õ���������
            Transform parent = GetUIGroup(group);
            ui.transform.SetParent(parent, false);

            // ����UILogic�е�Init��������ִ��Open
            UILogic uILogic = ui.AddComponent<UILogic>();
            uILogic.Init(luaName);
            uILogic.Open();
        });
    }
}
