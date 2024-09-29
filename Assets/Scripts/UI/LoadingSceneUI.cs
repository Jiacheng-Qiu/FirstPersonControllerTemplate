using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_Text;
    [SerializeField]
    Image m_loading;

    private void Start()
    {
        m_Text.text = "0%";
        m_loading.fillAmount = 0;
        EventBus.AddListener<float>(EventTypes.LoadingProgress, UpdateLoading);
    }
    

    private void UpdateLoading(float progress)
    {
        try
        {
            m_loading.fillAmount = progress;
            m_Text.text = ((int)(progress * 100)).ToString() + "%";
        } catch
        {

        }
    }
}
