using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    // 挂载场景脚本的对象名
    private string m_LogicName = "[SceneLogic]";

    private void Awake()
    {
        // 监听场景激活修改
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    /// <summary>
    /// 场景切换事件回调
    /// </summary>
    /// <param name="s1"></param>
    /// <param name="s2"></param>
    private void OnActiveSceneChanged(Scene s1, Scene s2)
    {
        if (!s1.isLoaded || !s2.isLoaded)
            return;

        SceneLogic logic1 = GetSceneLogic(s1);
        SceneLogic logic2 = GetSceneLogic(s2);

        // 隐藏前一个场景，激活后一个场景
        logic1?.OnInActive();
        logic2?.OnActive();
    }

    /// <summary>
    /// 激活场景的接口
    /// </summary>
    /// <param name="sceneName"></param>
    public void SetActive(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
    }

    /// <summary>
    /// 叠加加载场景的接口
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    public void LoadScene(string sceneName, string luaName)
    {
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            // 使用叠加模式加载场景
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Additive));
        });
    }

    /// <summary>
    /// 切换加载场景的接口
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    public void ChangeScene(string sceneName, string luaName)
    {
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            // 使用切换模式加载场景
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Single));
        });
    }

    /// <summary>
    /// 卸载场景的接口
    /// </summary>
    /// <param name="sceneName"></param>
    public void UnLoadSceneAsync(string sceneName)
    {
        StartCoroutine(UnLoadScene(sceneName));
    }

    /// <summary>
    /// 检测场景是否加载
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private bool IsLoadedScene(string sceneName)
    {
        // 通过名字获取到场景，通过isLoaded属性判断是否加载
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }

    /// <summary>
    /// 开始加载场景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    IEnumerator StartLoadScene(string sceneName, string luaName, LoadSceneMode mode)
    {
        // 如果场景已加载，则不重复加载
        if (IsLoadedScene(sceneName))
            yield break;

        // 异步加载场景
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, mode); 
        async.allowSceneActivation = true;// 场景加载完毕自动跳转，才能完整加载场景
        yield return async;

        Scene scene = SceneManager.GetSceneByName(sceneName);
        // 创建一个挂载场景脚本的节点，将其移动到对应场景中
        GameObject go = new GameObject(m_LogicName);
        SceneManager.MoveGameObjectToScene(go, scene);
        // 为父节点挂载场景逻辑脚本，执行初始化和首次加载方法
        SceneLogic logic = go.AddComponent<SceneLogic>();
        logic.SceneName = sceneName;
        logic.Init(luaName);
        logic.OnEnter();
    }

    /// <summary>
    /// 卸载场景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    IEnumerator UnLoadScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            Debug.LogError("scene is not loaded");
            yield break;
        }
        // 如果有logic对象，则调用logic中lua的卸载方法，随后异步卸载场景
        SceneLogic logic = GetSceneLogic(scene);
        logic?.OnQuit();
        AsyncOperation async = SceneManager.UnloadSceneAsync(scene);
        yield return async;
    }

    /// <summary>
    /// 查找场景中挂载的Logic
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private SceneLogic GetSceneLogic(Scene scene)
    {
        // 获取场景中所有的RootObject看有没有指定的场景父节点
        GameObject[] gameObjects = scene.GetRootGameObjects();
        foreach(GameObject go in gameObjects)
        {
            if(go.name.CompareTo(m_LogicName) == 0)
            {
                SceneLogic logic = go.GetComponent<SceneLogic>();
                return logic;
            }
        }
        return null;
    }


}
