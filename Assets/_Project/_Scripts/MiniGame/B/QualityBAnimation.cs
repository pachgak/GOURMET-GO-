using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // อย่าลืมใส่บรรทัดนี้

public class QualityBAnimation : MonoBehaviour
{
    [Header("Fire Graphics")]
    public List<RectTransform> fireGraphices;

    [Header("Animation Settings")]
    public float popDuration = 0.3f; // เวลาในการเด้ง Pop
    public float perfectScalPop = 1f;
    public float goodScalPop = 0.6f;
    public float missScalPop = 0.2f;

    [Header("Ref")]
    private MiniGameBManager _miniGameBManager;

    // ตัวแปรเก็บสถานะล่าสุด ป้องกันการรันแอนิเมชันซ้ำรัวๆ
    private MiniGameBManager.FireQuality? _currentQuality = null;

    private void Awake()
    {
        _miniGameBManager = GetComponent<MiniGameBManager>();

        // ซ่อนกราฟิกไฟทั้งหมดไว้ที่ Scale 0 ตอนเริ่มเกม
        ResetFires();
    }

    private void OnEnable()
    {
        if (_miniGameBManager != null)
        {
            _miniGameBManager.OnHitQualityEvaluated += HandleQualityEvaluated;
            _miniGameBManager.OnGameFinished += HandleGameFinished; // ดักฟังตอนจบเกมเพื่อดับไฟ
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
        // 1. ถ้าคุณภาพยังเหมือนเดิม (ผู้เล่นเลี้ยงหลอดนิ่งมาก) ไม่ต้องเล่นแอนิเมชันซ้ำ
        if (_currentQuality == quality) return;

        // 2. ถ้าเปลี่ยนสถานะ ให้จำสถานะใหม่ไว้
        _currentQuality = quality;

        // 3. กำหนดขนาดเป้าหมาย
        float targetScale = 0f;
        switch (quality)
        {
            case MiniGameBManager.FireQuality.Perfect:
                targetScale = perfectScalPop;
                break;
            case MiniGameBManager.FireQuality.Good:
                targetScale = goodScalPop;
                break;
            case MiniGameBManager.FireQuality.Miss:
                targetScale = missScalPop;
                break;
        }

        // 4. สั่ง Animate กองไฟทุกชิ้นใน List
        foreach (var fire in fireGraphices)
        {
            if (fire != null)
            {
                // หยุดแอนิเมชันเก่าที่อาจจะยังเล่นไม่จบ
                fire.DOKill();

                // เซ็ตขนาดกลับเป็น 0 เพื่อเตรียม Pop ตามที่คุณต้องการ!
                //fire.localScale = Vector3.zero;

                // สั่งเด้งไปหาขนาดเป้าหมาย พร้อมลูกเล่นสปริง (Ease.OutBack)
                fire.DOScale(targetScale, popDuration).SetEase(Ease.OutBack);
            }
        }
    }

    private void HandleGameFinished(Sprite reward, int count)
    {
        // ดับไฟ (เฟดขนาดลงเหลือ 0) ตอนจบมินิเกม
        foreach (var fire in fireGraphices)
        {
            if (fire != null)
            {
                fire.DOKill();
                fire.DOScale(0f, popDuration).SetEase(Ease.InBack);
            }
        }

        ResetFires();
    }

    private void ResetFires()
    {
        foreach (var fire in fireGraphices)
        {
            if (fire != null)
            {
                fire.localScale = new Vector3(missScalPop, missScalPop, missScalPop);
            }
        }
        _currentQuality = null;
    }
}