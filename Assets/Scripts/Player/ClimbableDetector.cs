using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class ClimbableDetector : MonoBehaviour
{
    [SerializeField]
    private float m_climbableMaxHeight;
    [SerializeField]
    private float m_detectAngle = 20f;
    [SerializeField]
    private float m_detectRadius = 1f;
    [SerializeField]
    private PlayerMovement m_playerMovement;
    [SerializeField]
    private string m_climbableTag;


    private readonly RaycastHit[] m_hits = new RaycastHit[5];

    private void Update()
    {
        // The ray beam makes sure that the object player want to climb will not be taller than the limit
        int len = 0;
        List<GameObject> objOverHeight = new();
        len = Physics.RaycastNonAlloc(transform.position + m_climbableMaxHeight * Vector3.up, transform.forward, m_hits, m_detectRadius);
        for(int i = 0; i < len; i++)
        {
            if (m_hits[i].collider.CompareTag(m_climbableTag))
            {
                objOverHeight.Add(m_hits[i].collider.gameObject);
            }
        }
        // Left side detection
        len = Physics.RaycastNonAlloc(transform.position + m_climbableMaxHeight * Vector3.up, Quaternion.Euler(0, -m_detectAngle, 0) * transform.forward, m_hits, m_detectRadius);
        for (int i = 0; i < len; i++)
        {
            if (m_hits[i].collider.CompareTag(m_climbableTag))
            {
                objOverHeight.Add(m_hits[i].collider.gameObject);
            }
        }
        // Right side detection
        len = Physics.RaycastNonAlloc(transform.position + m_climbableMaxHeight * Vector3.up, Quaternion.Euler(0, m_detectAngle, 0) * transform.forward, m_hits, m_detectRadius);
        for (int i = 0; i < len; i++)
        {
            if (m_hits[i].collider.CompareTag(m_climbableTag))
            {
                objOverHeight.Add(m_hits[i].collider.gameObject);
            }
        }
        m_playerMovement.SetCurOverHeightList(objOverHeight);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(m_climbableTag))
        {
            m_playerMovement.AddNewClimbable(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(m_climbableTag))
        {
            m_playerMovement.RemoveClimbable(other.gameObject);
        }
    }
}
