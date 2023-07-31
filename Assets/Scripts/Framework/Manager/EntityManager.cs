using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    // ����ʵ��
    Dictionary<string, GameObject> m_Entities = new Dictionary<string, GameObject>();

    // ����ʵ�����
    Dictionary<string, Transform> m_Groups = new Dictionary<string, Transform>();
    private Transform m_EntityParent;

    private void Awake()
    {
        // �ҵ�Entity�ĸ��ڵ�
        m_EntityParent = this.transform.parent.Find("Entity");
    }

    /// <summary>
    /// ���÷���
    /// </summary>
    /// <param name="group"></param>
    public void SetEntityGroup(List<string> groups)
    {
        for (int i = 0; i < groups.Count; i++)
        {
            // ���ݴ���ķ������б�ʵ������Ӧ�����ķ���ڵ㲢���뵽�ֵ���
            GameObject go = new GameObject("Group-" + groups[i]);
            go.transform.SetParent(m_EntityParent, false);
            m_Groups.Add(groups[i], go.transform);
        }
    }

    /// <summary>
    /// ��ȡ����
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    Transform GetGroup(string group)
    {
        Transform groupTransform;
        if (!m_Groups.TryGetValue(group, out groupTransform))
            Debug.LogError("group is not exist");
        return groupTransform;
    }

    /// <summary>
    /// ��ʾEntity���Զ���Entity����Lua��ִ��Lua�ű�
    /// </summary>
    /// <param name="name"></param>
    /// <param name="luaName"></param>
    public void ShowEntity(string name, string group, string luaName)
    {
        GameObject entity = null;

        // ���Entity�Ѿ������ڻ����У���ֱ�ӵ���OnShow����Init��
        if (m_Entities.TryGetValue(name, out entity))
        {
            EntityLogic logic = entity.GetComponent<EntityLogic>();
            logic.OnShow();
            return;
        }

        // ����Ҫ��Initһ����OnShow��ģ����Awake��Start��Ч��
        Manager.Resource.LoadPrefab(name, (UnityEngine.Object obj) =>
        {
            // ʵ����Entity
            entity = Instantiate(obj) as GameObject;
            // λ�����õ��������²�������ӵ�Entity������
            Transform parent = GetGroup(group);
            entity.transform.SetParent(parent, false);
            m_Entities.Add(name, entity);

            // ����EntityLogic�е�Init��������ִ��OnShow
            EntityLogic logic = entity.AddComponent<EntityLogic>();
            logic.Init(luaName);
            logic.OnShow();
        });
    }
}
