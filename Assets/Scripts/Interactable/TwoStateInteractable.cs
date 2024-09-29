using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TwoStateInteractable : Interactable
{

    public string InteractionTextA;
    public string InteractionTextB;

    [SerializeField]
    private UnityEvent m_interactionEventB;

    private bool m_isInitialState = true;

    protected override void OnInteractionUpdate()
    {
        if (m_interactionRunning)
        {
            m_interactionProgress += Time.deltaTime;
            if (m_interactionProgress >= m_interactionDuration)
            {
                m_interactionRunning = false;
                m_InteractDetector.InteractComplete();

                if (m_isInitialState)
                {
                    m_interactionEvent.Invoke();
                } else
                {
                    m_interactionEventB.Invoke();
                }

                m_isInitialState = !m_isInitialState;

                if (m_isInitialState)
                {
                    InteractionText = InteractionTextA;
                } else
                {
                    InteractionText = InteractionTextB;
                }
            }
        }
    }
}
