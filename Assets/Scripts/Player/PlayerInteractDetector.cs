using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractDetector : MonoBehaviour
{
    [SerializeField]
    private bool m_interactEnabled = true;
    [SerializeField]
    private float m_interactRange;
    [SerializeField]
    private LayerMask m_interactionLayer;

    private GameObject m_interactableTarget;

    private void Start()
    {
        EventBus.AddListener(EventTypes.Interact, Interact);
    }

    private void Update()
    {
        // Always take the first item in ray range as potential interactable object
        if (m_interactEnabled)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, m_interactRange, m_interactionLayer))
            {
                if (hit.rigidbody.gameObject != m_interactableTarget)
                {
                    RenewTarget(hit.rigidbody.gameObject);
                    m_interactableTarget.GetComponent<Interactable>().OnRaycastEnter();
                }
            } else
            {
                RenewTarget(null);
            }
        }
    }

    private void RenewTarget(GameObject newTarget)
    {
        if (m_interactableTarget != null)
            m_interactableTarget.GetComponent<Interactable>().OnRaycastEnd();
        else
            EventBus.Broadcast<Interactable>(EventTypes.InteractionTextHide, null);
        m_interactableTarget = newTarget;
    }

    /// <summary>
    /// Interaction is only enabled under certain circumstances (no jump/climb/shooting)
    /// </summary>
    private void Interact()
    {
        if (m_interactEnabled && m_interactableTarget != null && Player.Instance.CanInteract())
        {
            m_interactEnabled = false;
            Player.Instance.AddMovementInputLimit();
            m_interactableTarget.GetComponent<Interactable>().OnInteractionStart(this);
        }
    }

    public void InteractComplete()
    {
        Player.Instance.RemoveMovementInputLimit();
        m_interactEnabled = true;
    }
}
