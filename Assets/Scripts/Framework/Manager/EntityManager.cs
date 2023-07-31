using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    // 缓存实体
    Dictionary<string, GameObject> m_Entities = new Dictionary<string, GameObject>();

    // 保存实体分组
    Dictionary<string, Transform> m_Groups = new Dictionary<string, Transform>();
    private Transform m_EntityParent;

    private void Awake()
    {
        // 找到Entity的父节点
        m_EntityParent = this.transform.parent.Find("Entity");
    }

    /// <summary>
    /// 设置分组
    /// </summary>
    /// <param name="group"></param>
    public void SetEntityGroup(List<string> groups)
    {
        for (int i = 0; i < groups.Count; i++)
        {
            // 根据传入的分组名列表实例化对应数量的分组节点并加入到字典中
            GameObject go = new GameObject("Group-" + groups[i]);
            go.transform.SetParent(m_EntityParent, false);
            m_Groups.Add(groups[i], go.transform);
        }
    }

    /// <summary>
    /// 获取分组
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
    /// 显示Entity，自动绑定Entity名和Lua名执行Lua脚本
    /// </summary>
    /// <param name="name"></param>
    /// <param name="luaName"></param>
    public void ShowEntity(string name, string group, string luaName)
    {
        GameObject entity = null;

        // 如果Entity已经存在于缓存中，则直接调用OnShow不用Init了
        if (m_Entities.TryGetValue(name, out entity))
        {
            EntityLogic logic = entity.GetComponent<EntityLogic>();
            logic.OnShow();
            return;
        }

        // 否则还要先Init一次再OnShow，模仿先Awake后Start的效果
        Manager.Resource.LoadPrefab(name, (UnityEngine.Object obj) =>
        {
            // 实例化Entity
            entity = Instantiate(obj) as GameObject;
            // 位置设置到分组结点下并将其添加到Entity缓存中
            Transform parent = GetGroup(group);
            entity.transform.SetParent(parent, false);
            m_Entities.Add(name, entity);

            // 调用EntityLogic中的Init方法，并执行OnShow
            EntityLogic logic = entity.AddComponent<EntityLogic>();
            logic.Init(luaName);
            logic.OnShow();
        });
    }
}
