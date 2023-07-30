using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // ��Manager�ҵ����й�����
    private static ResourceManager _resource;
    public static ResourceManager Resource
    {
        get { return _resource; }
    }

    public void Awake()
    {
        _resource = this.gameObject.AddComponent<ResourceManager>();
    }
}
