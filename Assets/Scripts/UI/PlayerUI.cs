using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    [SerializeField]
    private Image m_staminaBar;

    [SerializeField]
    private CanvasGroup m_interactionUIGroup;
    [SerializeField]
    private TextMeshProUGUI m_interactionText;

    private void Start()
    {
        EventBus.AddListener<Interactable>(EventTypes.InteractionTextPopup, OnNewIndicatorText);
        EventBus.AddListener<Interactable>(EventTypes.InteractionTextHide, OnIndicatorTextHide);
    }

    private void OnNewIndicatorText(Interactable ui)
    {
        if (ui == null)
            return;
        m_interactionText.text = ui.InteractionText;
        m_interactionUIGroup.alpha = 1;
    }

    private void OnIndicatorTextHide(Interactable ui)
    {
        if (ui == null || m_interactionText.text == ui.InteractionText)
        {
            m_interactionUIGroup.alpha = 0;
        }
    }

    private void Update()
    {
        // Stamina Bar Update
        if (Player.Instance.Stamina < 0)
        {
            m_staminaBar.fillAmount = 0;
        }
        else if (Player.Instance.Stamina > Player.Instance.StaminaMaxLimit)
        {
            m_staminaBar.fillAmount = 1;
        }
        else
        {
            m_staminaBar.fillAmount = Player.Instance.Stamina / Player.Instance.StaminaMaxLimit;
        }
    }
}
