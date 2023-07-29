using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        //�첽����
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(
            Application.streamingAssetsPath + "/ui/prefabs/testui.prefab.ab");
        yield return request;//�ȴ��������

        AssetBundleCreateRequest request1 = AssetBundle.LoadFromFileAsync(
            Application.streamingAssetsPath + "/ui/res/play.jpg.ab");
        yield return request1;

        AssetBundleCreateRequest request2 = AssetBundle.LoadFromFileAsync(
            Application.streamingAssetsPath + "/ui/res/btn_login.png.ab");
        yield return request2;

        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync(
            "Assets/BuildResources/UI/Prefabs/TestUI.prefab");
        yield return bundleRequest;

        //ʵ�������ص�Ԥ����
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
