using UnityEngine;
using DG.Tweening;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Settings")]
    public CanvasGroup blackScreen;
    public float defaultFadeDuration = 1.5f;
    public float defaultStartDelay = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // àÃÔèÁà¡ÁÁÒãËéà¿´à¢éÒ (¨Ò¡´Óä»ãÊ) ·Ñ¹·Õ
        FadeIn(defaultFadeDuration, defaultStartDelay);
    }

    // ¿Ñ§¡ìªÑ¹à¿´à¢éÒ (¨Í´Ó¤èÍÂæ ËÒÂä»)
    public void FadeIn(float duration, float delay = 0f)
    {
        Debug.Log("FadeIn");

        if (blackScreen == null) return;

        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 1f;

        blackScreen.DOFade(0f, duration)
            .SetDelay(delay)
            .OnComplete(() => blackScreen.gameObject.SetActive(false));
        Debug.Log("OnComplete");
    }

    // ¿Ñ§¡ìªÑ¹à¿´ÍÍ¡ (¨Í´Ó¤èÍÂæ â¼ÅèÁÒ) - àÍÒäÇéãªéµÍ¹à»ÅÕèÂ¹©Ò¡ËÃ×Í¨ºà¡Á
    public void FadeOut(float duration, float delay = 0f, System.Action onComplete = null)
    {
        if (blackScreen == null) return;

        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 0f;

        blackScreen.DOFade(1f, duration)
            .SetDelay(delay)
            .OnComplete(() => onComplete?.Invoke());
    }
}