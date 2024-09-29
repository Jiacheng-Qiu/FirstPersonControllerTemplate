using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLightHouse : MonoBehaviour
{
    [SerializeField]
    private float m_lightIntensityChange;
    [SerializeField]
    private Light m_pointLight;

    private void Start()
    {
        if (m_pointLight != null)
        {
            StartCoroutine(Flash());
        }
    }

    private IEnumerator Flash()
    {
        float e = 0;
        while (true)
        {
            if (e % 2 > 1)
            {
                m_pointLight.intensity += m_lightIntensityChange * Time.unscaledDeltaTime;
            } else
            {
                m_pointLight.intensity -= m_lightIntensityChange * Time.unscaledDeltaTime;

            }
            e += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
