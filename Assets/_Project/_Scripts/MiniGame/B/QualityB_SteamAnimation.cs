using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class QualityB_SteamAnimation : MonoBehaviour
{
    [Header("Steam References")]
    public GameObject steamPrefab;
    public RectTransform spawnArea;
    public float targetHeight = 200f;

    [Header("Animation Settings")]
    public float perfactHeat = 1f;
    public float goodHeat = 0.5f;
    public float missHeat = 0.1f;

    [Header("Heat Settings")]
    [Tooltip("ค่าความแรงไฟปัจจุบัน (0 = อ่อนสุด, 1 = แรงสุด)")]
    private float currentHeat = 0f;

    private MiniGameBManager _miniGameBManager;
    private MiniGameBManager.FireQuality? _currentQuality = null;
    private Coroutine _steamRoutine;

    private void Awake()
    {
        _miniGameBManager = GetComponent<MiniGameBManager>();
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

    // --- รับ Event การเปลี่ยนคุณภาพคะแนน ---
    private void HandleQualityEvaluated(MiniGameBManager.FireQuality quality, int score)
    {
        if (!_miniGameBManager.isPlaying) return;

        if (_currentQuality == quality) return;
        _currentQuality = quality;

        // ปรับค่าความแรงไฟ (Heat) ตามคุณภาพการกดของผู้เล่น
        switch (quality)
        {
            case MiniGameBManager.FireQuality.Perfect:
                currentHeat = perfactHeat; // ไฟแรงสุด เสกควันรัวๆ
                break;
            case MiniGameBManager.FireQuality.Good:
                currentHeat = goodHeat; // ไฟกลาง
                break;
            case MiniGameBManager.FireQuality.Miss:
                currentHeat = missHeat; // ไฟอ่อน ควันน้อย
                break;
        }

        // เริ่มลูปเสกควันถ้ายังไม่ได้เริ่ม
        if (_steamRoutine == null)
        {
            _steamRoutine = StartCoroutine(SteamSpawnerRoutine());
        }
    }

    private void HandleGameFinished(Sprite reward, int count)
    {
        Debug.Log("HandleGameFinished Steam");

        // จบเกม สั่งหยุดควัน
        if (_steamRoutine != null)
        {
            StopCoroutine(_steamRoutine);
            _steamRoutine = null;
            Debug.Log("_steamRoutine != null");
        }
        _currentQuality = null;
    }

    // --- ลูปเสกควัน (ความถี่เปลี่ยนตาม currentHeat อัตโนมัติ) ---
    private IEnumerator SteamSpawnerRoutine()
    {
        while (true)
        {
            Debug.Log("SteamSpawnerRoutine");
            float spawnDelay = Mathf.Lerp(1.5f, 0.15f, currentHeat);
            SpawnSingleSteam();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnSingleSteam()
    {
        // 1. เรียกใช้งานผ่าน Object Pooling แทน Instantiate
        // ใช้เวอร์ชัน Spawn(prefab, parent) ตามที่คุณเขียนไว้ใน Manager (พารามิเตอร์ setParant)
        GameObject steamObj = ObjectPoolingManager.Instance.Spawn(steamPrefab, spawnArea);
        RectTransform steamRect = steamObj.GetComponent<RectTransform>();
        Image steamImage = steamObj.GetComponent<Image>();

        // 2. --- สำคัญมาก: การรีเซ็ตสถานะ (Reset State) ---
        // หยุดแอนิเมชันเก่าที่อาจจะหลงเหลืออยู่
        steamRect.DOKill();
        steamImage.DOKill();

        // รีเซ็ต Scale และ Alpha กลับเป็นค่าเริ่มต้น
        steamRect.localScale = Vector3.one;
        steamImage.color = new Color(steamImage.color.r, steamImage.color.g, steamImage.color.b, 1f);

        // สุ่ม X ในพื้นที่ปากหม้อ และเซ็ตแกน Y เป็น 0 เพื่อเริ่มจากด้านล่างเสมอ
        float randomX = Random.Range(-spawnArea.rect.width / 2f, spawnArea.rect.width / 2f);
        steamRect.anchoredPosition = new Vector2(randomX, 0f);

        // 3. --- พารามิเตอร์ DOTween เปลี่ยนตามความแรงไฟ ---
        float floatDuration = Mathf.Lerp(3.0f, 1.0f, currentHeat);
        float endScale = Mathf.Lerp(1.0f, 2.0f, currentHeat);
        float wiggleDistance = Mathf.Lerp(15f, 40f, currentHeat);
        float wiggleSpeed = floatDuration / 4f;

        // ลอยขึ้น
        steamRect.DOAnchorPosY(targetHeight, floatDuration).SetEase(Ease.OutSine);

        // ขยาย
        steamRect.DOScale(endScale, floatDuration).SetEase(Ease.OutQuad);

        // ส่าย
        steamRect.DOAnchorPosX(steamRect.anchoredPosition.x + wiggleDistance, wiggleSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // 4. เฟดและส่งคืน Pool 
        steamImage.DOFade(0f, floatDuration).SetEase(Ease.InQuad)
            .OnComplete(() => {
                steamRect.DOKill();

                // คืน Object เข้า Pool แทนการ Destroy
                ObjectPoolingManager.Instance.Respawn(steamObj);
            });
    }
}