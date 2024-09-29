using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// Boolean flag which indicates whether this singleton may be reset
    /// Resetting will clear all data stored within the object and reset them
    /// to their default values.
    /// </summary>
    public bool CanBeReset = true;

    private static T m_Instance;
    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType(typeof(T)) as T;

                if (m_Instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    m_Instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);
                }
            }

            return m_Instance;
        }
    }

    /// <summary>
    /// If the existing singleton object can be reset, will reset the object
    /// Otherwise, does nothing.
    /// </summary>
    /// <returns>True upon successful reset; false otherwise</returns>
    public virtual bool Reset()
    {
        if (m_Instance == null) return false;
        if (!CanBeReset) return false;

        Destroy(m_Instance.gameObject);
        m_Instance = default(T);
        return true;
    }

    /// <summary>
    /// Returns whether there exists a valid instance for this monobehavior class
    /// </summary>
    public static bool IsValid()
    {
        return (m_Instance != null);
    }

    /// <summary>
    /// Ensures the uniqueness of the Monobehaviour singleton
    /// </summary>
    protected virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (m_Instance == null)
        {
            m_Instance = this as T;
        }
        else if (m_Instance != this as T)
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Resets singleton object upon object destruction
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (m_Instance == this)
        {
            m_Instance = default(T);
        }
    }
}
