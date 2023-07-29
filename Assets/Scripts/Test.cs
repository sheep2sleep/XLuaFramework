using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        //异步加载
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(
            Application.streamingAssetsPath + "/ui/prefabs/testui.prefab.ab");
        yield return request;//等待加载完成

        AssetBundleCreateRequest request1 = AssetBundle.LoadFromFileAsync(
            Application.streamingAssetsPath + "/ui/res/play.jpg.ab");
        yield return request1;

        AssetBundleCreateRequest request2 = AssetBundle.LoadFromFileAsync(
            Application.streamingAssetsPath + "/ui/res/btn_login.png.ab");
        yield return request2;

        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync(
            "Assets/BuildResources/UI/Prefabs/TestUI.prefab");
        yield return bundleRequest;

        //实例化加载的预制体
        GameObject go = Instantiate(bundleRequest.asset) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
