using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    // ���س����ű��Ķ�����
    private string m_LogicName = "[SceneLogic]";

    private void Awake()
    {
        // �������������޸�
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    /// <summary>
    /// �����л��¼��ص�
    /// </summary>
    /// <param name="s1"></param>
    /// <param name="s2"></param>
    private void OnActiveSceneChanged(Scene s1, Scene s2)
    {
        if (!s1.isLoaded || !s2.isLoaded)
            return;

        SceneLogic logic1 = GetSceneLogic(s1);
        SceneLogic logic2 = GetSceneLogic(s2);

        // ����ǰһ�������������һ������
        logic1?.OnInActive();
        logic2?.OnActive();
    }

    /// <summary>
    /// ������Ľӿ�
    /// </summary>
    /// <param name="sceneName"></param>
    public void SetActive(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
    }

    /// <summary>
    /// ���Ӽ��س����Ľӿ�
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    public void LoadScene(string sceneName, string luaName)
    {
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            // ʹ�õ���ģʽ���س���
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Additive));
        });
    }

    /// <summary>
    /// �л����س����Ľӿ�
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    public void ChangeScene(string sceneName, string luaName)
    {
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            // ʹ���л�ģʽ���س���
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Single));
        });
    }

    /// <summary>
    /// ж�س����Ľӿ�
    /// </summary>
    /// <param name="sceneName"></param>
    public void UnLoadSceneAsync(string sceneName)
    {
        StartCoroutine(UnLoadScene(sceneName));
    }

    /// <summary>
    /// ��ⳡ���Ƿ����
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private bool IsLoadedScene(string sceneName)
    {
        // ͨ�����ֻ�ȡ��������ͨ��isLoaded�����ж��Ƿ����
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }

    /// <summary>
    /// ��ʼ���س���
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    IEnumerator StartLoadScene(string sceneName, string luaName, LoadSceneMode mode)
    {
        // ��������Ѽ��أ����ظ�����
        if (IsLoadedScene(sceneName))
            yield break;

        // �첽���س���
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, mode); 
        async.allowSceneActivation = true;// ������������Զ���ת�������������س���
        yield return async;

        Scene scene = SceneManager.GetSceneByName(sceneName);
        // ����һ�����س����ű��Ľڵ㣬�����ƶ�����Ӧ������
        GameObject go = new GameObject(m_LogicName);
        SceneManager.MoveGameObjectToScene(go, scene);
        // Ϊ���ڵ���س����߼��ű���ִ�г�ʼ�����״μ��ط���
        SceneLogic logic = go.AddComponent<SceneLogic>();
        logic.SceneName = sceneName;
        logic.Init(luaName);
        logic.OnEnter();
    }

    /// <summary>
    /// ж�س���
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
        // �����logic���������logic��lua��ж�ط���������첽ж�س���
        SceneLogic logic = GetSceneLogic(scene);
        logic?.OnQuit();
        AsyncOperation async = SceneManager.UnloadSceneAsync(scene);
        yield return async;
    }

    /// <summary>
    /// ���ҳ����й��ص�Logic
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private SceneLogic GetSceneLogic(Scene scene)
    {
        // ��ȡ���������е�RootObject����û��ָ���ĳ������ڵ�
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
