using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : CSingletonMono<ObjectPool>
{
    [SerializeField]
    private GameObject m_itemUIPrefab;

    private IObjectPool<GameObject> m_UIpool;

    private void Start()
    {
        m_UIpool = new ObjectPool<GameObject>(CreateItem, OnFetch, OnRelease);
    }

    GameObject CreateItem()
    {
        return Instantiate(m_itemUIPrefab as GameObject);
    }

    void OnFetch(GameObject obj)
    {
        obj.SetActive(true);    
    }

    void OnRelease(GameObject obj)
    {
        obj.transform.SetParent(null);
        obj.SetActive(false);
    }

    public IObjectPool<GameObject> GetUIPool()
    {
        return m_UIpool;
    }
}
