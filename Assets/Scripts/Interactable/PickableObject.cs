using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Bullet,
    Gun,
    Grenade,
    Melee,
    Healing,
    Tool
}

public class PickableObject : Interactable
{
    [SerializeField]
    private ItemType m_type;
    [SerializeField]
    private int m_itemID;
    [SerializeField]
    private int m_itemCount;
    [SerializeField]
    private int m_maxItemStackCount;
    [SerializeField]
    private Vector2Int m_itemSizeOnUI;


    public int GetItemCount()
    {
        return m_itemCount;
    }

    public void SetItemCount(int count)
    {
        m_itemCount = count;
    }

    public int GetItemMaxCount()
    {
        return m_maxItemStackCount;
    }

    public Vector2Int GetUISize()
    {
        return m_itemSizeOnUI;
    }

    public int GetItemID()
    {
        return m_itemID;
    }

    public ItemType GetItemType()
    {
        return m_type;
    }
}
