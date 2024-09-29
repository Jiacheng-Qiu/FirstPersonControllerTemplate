using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    private float m_UIFadeTime;
    [SerializeField]
    private CanvasGroup m_mainMenuGroup;
    [SerializeField]
    private CanvasGroup m_optionsGroup;

    public void RunGame()
    {
        GameManager.Instance.SwitchScene("PlayGround", true);
        StartCoroutine(UIFade(m_mainMenuGroup, false));
    }

    private IEnumerator UIFade(CanvasGroup group, bool fadeIn)
    {
        float e = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
        while (e < m_UIFadeTime)
        {
            group.alpha = (fadeIn ? Mathf.Clamp01(e / m_UIFadeTime) : 1 - Mathf.Clamp01(e / m_UIFadeTime));
            e += Time.unscaledDeltaTime;
            yield return null;
        }
        group.alpha = (fadeIn ? 1 : 0);
        if (fadeIn)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }

    public void EnableOptions()
    {
        StartCoroutine(UIFade(m_mainMenuGroup, false));
        StartCoroutine(UIFade(m_optionsGroup, true));
    }
    
    public void ApplySettingsChange()
    {

    }

    public void DisableOptions()
    {
        StartCoroutine(UIFade(m_mainMenuGroup, true));
        StartCoroutine(UIFade(m_optionsGroup, false));
    }

    public void Quit()
    {
        GameManager.Instance.Quit();
    }
}
