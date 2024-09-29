using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameManager : CSingletonMono<GameManager>
{
    [Header("Scene Transition Settings")]
    [SerializeField]
    float BlackScreenDefaultWaitTime;
    [SerializeField]
    bool FadeOut = true;
    [Range(0.1f, 3f)][SerializeField]
    float FadeOutDuration = 1;
    [SerializeField]
    bool FadeIn = true;
    [Range(0.1f, 3f)][SerializeField]
    float FadeInDuration = 1;
    [Range(0.1f, 3f)][SerializeField]
    float EyeOpenDuration = .5f;
    [SerializeField]
    float VignetteDefaultValue = .2f;
    [SerializeField]
    AnimationCurve BlinkAnimCurve;
    [SerializeField]
    private VolumeProfile m_postProcessVolume;

    

    private void Start()
    {
        //Post processing related
        m_postProcessVolume.TryGet(out ColorAdjustments colorAdjustments);
        colorAdjustments.postExposure.value = 0;
        m_postProcessVolume.TryGet(out Vignette vignette);
        vignette.intensity.value = VignetteDefaultValue;
    }

    #region "SceneSwitching"
    public void SwitchScene(string sceneName, bool enableEyeBlinking = false) 
    {
        StartCoroutine(BlackScreenLoading(sceneName, enableEyeBlinking));
    }

    private IEnumerator BlackScreenLoading(string sceneName, bool enableEyeBlinking)
    {
        float elapsed = 0;
        m_postProcessVolume.TryGet(out ColorAdjustments colorAdjustments);
        // Fade out into black screen
        if (FadeOut)
        {
            elapsed = 0;
            while (elapsed < FadeOutDuration)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(0, -10, Mathf.Clamp01(elapsed / FadeOutDuration));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            colorAdjustments.postExposure.value = -10;
        }
        SceneManager.LoadScene("BlackScreenLoading");
        yield return new WaitForSeconds(BlackScreenDefaultWaitTime);

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!asyncOp.isDone)
        {
            float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
            EventBus.Broadcast<float>(EventTypes.LoadingProgress, progress);
            yield return null;
        }

        if (enableEyeBlinking)
        {
            m_postProcessVolume.TryGet(out Vignette vignette);
            vignette.intensity.value = 1;
        }

        // Fade into new scene
        if (FadeIn)
        {
            elapsed = 0;
            while (elapsed < FadeInDuration)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(-10, 0, Mathf.Clamp01(elapsed / FadeInDuration));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            colorAdjustments.postExposure.value = 0;
        }

        if (enableEyeBlinking)
        {
            m_postProcessVolume.TryGet(out Vignette vignette);
            elapsed = 0;
            while (elapsed < EyeOpenDuration)
            {
                vignette.intensity.value = BlinkAnimCurve.Evaluate(Mathf.Clamp01(elapsed / EyeOpenDuration));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            vignette.intensity.value = VignetteDefaultValue;
        }
    }
    #endregion

    public void Quit()
    {
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        //Post processing related
        m_postProcessVolume.TryGet(out ColorAdjustments colorAdjustments);
        colorAdjustments.postExposure.value = 0;
        m_postProcessVolume.TryGet(out Vignette vignette);
        vignette.intensity.value = VignetteDefaultValue;
    }


}
