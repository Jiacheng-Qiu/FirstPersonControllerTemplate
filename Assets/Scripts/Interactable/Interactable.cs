using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public string InteractionText;

    [SerializeField, Tooltip("Duration for player/npc to finish interaction")]
    protected float m_interactionDuration;
    [SerializeField]
    protected bool m_interactionEnabled = true;
    protected float m_interactionProgress;
    protected bool m_interactionRunning = false;

    protected PlayerInteractDetector m_InteractDetector;

    public bool InteractionEnabled { get { return m_interactionEnabled; } set {  m_interactionEnabled = value; } }

    [SerializeField]
    protected UnityEvent m_interactionEvent;

    public virtual void OnRaycastEnter()
    {
        EventBus.Broadcast<Interactable>(EventTypes.InteractionTextPopup, this);
    }

    public virtual void OnRaycastEnd()
    {
        EventBus.Broadcast<Interactable>(EventTypes.InteractionTextHide, this);
    }

    public virtual void OnInteractionStart(PlayerInteractDetector interactOrigin)
    {
        m_interactionRunning = true;
        m_InteractDetector = interactOrigin;
        m_interactionProgress = 0;
    }

    private void Update()
    {
        OnInteractionUpdate();
    }

    protected virtual void OnInteractionUpdate()
    {
        if (m_interactionRunning)
        {
            m_interactionProgress += Time.deltaTime;
            if (m_interactionProgress >= m_interactionDuration)
            {
                m_interactionRunning = false;
                m_InteractDetector.InteractComplete();
                m_interactionEvent.Invoke();
            }
        }
    }

    public virtual void DestroySelf()
    {
        StopAllCoroutines();
    }

}
