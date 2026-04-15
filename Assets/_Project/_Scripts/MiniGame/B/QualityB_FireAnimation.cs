using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class QualityB_FireAnimation : MonoBehaviour
{
    [Header("Fire Graphics")]
    public List<RectTransform> fireGraphices;
    private List<Image> _cachedFireImages = new List<Image>();

    [Header("Animation Settings")]
    public float popDuration = 0.3f; // เวลาในการเด้ง Pop
    public float perfectScalPop = 1f;
    public float goodScalPop = 0.6f;
    public float missScalPop = 0.2f;

    // --- 1. เพิ่มการตั้งค่าความสั่นไหวของไฟตรงนี้ ---
    [Header("Flicker Settings")]
    public float flickerDuration = 1.0f; // ความเร็วของ 1 รอบการสั่น
    public Vector3 flickerStrength = new Vector3(0.08f, 0.08f, 0f); // ความแรงในการสั่น (Scale)
    public int flickerVibrato = 5; // ความถี่ในการสั่น (ยิ่งเยอะยิ่งสั่นรัว)

    [Header("Slider UI Elements")]
    public Image fireSliderBar;
    public Image fireSliderIcon;

    [Header("Slider Colors")]
    public Color perfectFireSliderBarColor = Color.cyan;
    public Color goodFireSliderBarColor = Color.yellow;
    public Color missFireSliderBarColor = Color.red;

    [Header("Fire Sprites")]
    public Sprite perfectFireSprite;
    public Sprite goodFireSprite;
    public Sprite missFireSprite;

    [Header("Ref")]
    private MiniGameBManager _miniGameBManager;

    private MiniGameBManager.FireQuality? _currentQuality = null;

    private void Awake()
    {
        _miniGameBManager = GetComponent<MiniGameBManager>();

        foreach (var fire in fireGraphices)
        {
            if (fire != null)
            {
                Image img = fire.GetComponentInChildren<Image>();
                _cachedFireImages.Add(img);
            }
            else
            {
                _cachedFireImages.Add(null);
            }
        }

        ResetFires();
    }

    private void OnEnable()
    {
        if (_miniGameBManager != null)
        {
            _miniGameBManager.OnHitQualityEvaluated += HandleQualityEvaluated;
            _miniGameBManager.OnGameFinished += HandleGameFinished;
        }
    }

    private void OnDisable()
    {
        if (_miniGameBManager != null)
        {
            _miniGameBManager.OnHitQualityEvaluated -= HandleQualityEvaluated;
            _miniGameBManager.OnGameFinished -= HandleGameFinished;
        }
    }

    private void HandleQualityEvaluated(MiniGameBManager.FireQuality quality, int score)
    {
        if (!_miniGameBManager.isPlaying) return;
        if (_currentQuality == quality) return;
        _currentQuality = quality;

        float targetScale = 0f;
        Color targetColor = missFireSliderBarColor;
        Sprite targetSprite = missFireSprite;

        switch (quality)
        {
            case MiniGameBManager.FireQuality.Perfect:
                targetScale = perfectScalPop;
                targetColor = perfectFireSliderBarColor;
                targetSprite = perfectFireSprite;
                break;
            case MiniGameBManager.FireQuality.Good:
                targetScale = goodScalPop;
                targetColor = goodFireSliderBarColor;
                targetSprite = goodFireSprite;
                break;
            case MiniGameBManager.FireQuality.Miss:
                targetScale = missScalPop;
                targetColor = missFireSliderBarColor;
                targetSprite = missFireSprite;
                break;
        }

        if (fireSliderBar != null)
        {
            fireSliderBar.DOKill();
            fireSliderBar.DOColor(targetColor, popDuration);
        }

        if (fireSliderIcon != null && targetSprite != null)
        {
            fireSliderIcon.sprite = targetSprite;
            fireSliderIcon.transform.DOKill();
            fireSliderIcon.transform.DOScale(1.2f, popDuration / 2f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                fireSliderIcon.transform.DOScale(1f, popDuration / 2f).SetEase(Ease.InQuad);
            });
        }

        for (int i = 0; i < fireGraphices.Count; i++)
        {
            RectTransform fireRect = fireGraphices[i];
            Image fireImage = _cachedFireImages[i];

            if (fireRect != null)
            {
                if (fireImage != null && targetSprite != null)
                {
                    fireImage.sprite = targetSprite;
                }

                fireRect.DOKill();

                // --- 2. เปลี่ยนจุด Animate ไฟ ตรงนี้ ---
                // สั่งเด้งไปหาขนาดเป้าหมายให้เสร็จก่อน
                fireRect.DOScale(targetScale, popDuration).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    // พอเด้งเสร็จปุ๊บ ให้เริ่มสั่นไหว (Flicker) แบบ Loop ทันที
                    // นำค่า flickerStrength มาคูณกับ targetScale เพื่อให้ไฟดวงใหญ่สั่นแรงกว่าไฟดวงเล็ก
                    fireRect.DOShakeScale(flickerDuration, flickerStrength * targetScale, flickerVibrato, 90f).SetLoops(-1);
                });
            }
        }
    }

    private void HandleGameFinished(Sprite reward, int count)
    {
        foreach (var fire in fireGraphices)
        {
            if (fire != null)
            {
                fire.DOKill(); // คำสั่งนี้จะหยุดการสั่นไหวที่ Loop อยู่ให้ด้วยอัตโนมัติ
                fire.DOScale(0f, popDuration).SetEase(Ease.InBack);
            }
        }

        ResetFires();
    }

    private void ResetFires()
    {
        for (int i = 0; i < fireGraphices.Count; i++)
        {
            RectTransform fireRect = fireGraphices[i];
            Image fireImage = _cachedFireImages[i];

            if (fireRect != null)
            {
                fireRect.localScale = new Vector3(missScalPop, missScalPop, missScalPop);

                if (fireImage != null && missFireSprite != null)
                {
                    fireImage.sprite = missFireSprite;
                }
            }
        }

        if (fireSliderBar != null) fireSliderBar.color = missFireSliderBarColor;
        if (fireSliderIcon != null && missFireSprite != null) fireSliderIcon.sprite = missFireSprite;

        _currentQuality = null;
    }
}