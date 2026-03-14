using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Inventory.Model;
using System;
using Random = UnityEngine.Random;
using DG.Tweening;

public class MiniGameBManager : MiniGameBase
{
    [Header("Game Settings")]
    public float hitSize = 60f;
    public float hitPerfectSize = 25f; // เปลี่ยนมาใช้อันนี้แทน!

    [Header("Stardew Physics Mechanics")]
    public float gravity = 1.5f;
    public float thrust = 3.0f;
    public float maxFallSpeed = 1.5f;
    public float maxUpSpeed = 1.5f;
    public float bounciness = 0.3f;

    [Header("Target AI Settings")]
    public float targetMoveSpeed = 0.5f;
    public float maxTargetChangeInterval = 1f; // (แก้ชื่อตัวพิมพ์ใหญ่ให้นิดนึงครับ)
    public float minTargetChangeInterval = 3.0f;

    [Tooltip("ระยะห่างขั้นต่ำที่เป้าหมายต้องกระโดดหนี (0.1 - 0.3 กำลังดี)")]
    public float minDistanceVal = 0.2f;

    [Header("Scoring")]
    public float scoreTickInterval = 0.1f;
    public int perfectTickScore = 2;
    public int goodTickScore = 1;

    [Header("UI References")]
    public RectTransform sliderArea;
    public Slider playerBar;
    public RectTransform goodHitUI;
    public RectTransform perfectHitUI;
    public Slider targetPoint;

    [Header("Action Event")]
    public Action OnPlaySoundTick;
    public Action<FireQuality, int> OnHitQualityEvaluated;

    // ตัวแปรระบบหลังบ้าน
    private float _playerVelocity;
    private float _targetDestination;
    private float _normalizedGoodHalf;
    private float _normalizedPerfectHalf;

    private Coroutine _scoringRoutine;
    private Coroutine _targetAIRoutine;

    public enum FireQuality
    {
        Perfect,
        Good,
        Miss
    }

    private void Awake()
    {
        gameplayPanel.SetActive(false);
    }

    public override void SetupFromRecipe(CookingRecipeSO recipe, int targetMaxScore, int cookCount)
    {
        base.SetupFromRecipe(recipe, targetMaxScore, cookCount);

        // 1. จัดการขนาดของ UI โดยใช้ขนาดที่ตั้งไว้ตรงๆ เลย
        if (goodHitUI != null)
        {
            goodHitUI.sizeDelta = new Vector2(goodHitUI.sizeDelta.x, hitSize);
        }
        if (perfectHitUI != null)
        {
            // ใช้ hitPerfectSize แทนการคำนวณเปอร์เซ็นต์
            perfectHitUI.sizeDelta = new Vector2(perfectHitUI.sizeDelta.x, hitPerfectSize);
        }

        // 2. คำนวณระยะ Normalized (0 ถึง 1) สำหรับใช้เช็คเป้าหมายและ AI
        float totalHeight = sliderArea.rect.height;
        if (totalHeight > 0)
        {
            _normalizedGoodHalf = (hitSize / totalHeight) / 2f;
            // ใช้ hitPerfectSize มาคำนวณ Normalized
            _normalizedPerfectHalf = (hitPerfectSize / totalHeight) / 2f;
        }

        playerBar.value = _normalizedPerfectHalf;
        targetPoint.value = 0.5f;
        _playerVelocity = 0f;
    }

    public override void StartGame()
    {
        if (isPlaying) return;
        isPlaying = true;

        _scoringRoutine = StartCoroutine(ScoringRoutine());
        _targetAIRoutine = StartCoroutine(TargetAIRoutine());
    }

    public override void EndGame()
    {
        isPlaying = false;
        isReady = false;

        if (_scoringRoutine != null) StopCoroutine(_scoringRoutine);
        if (_targetAIRoutine != null) StopCoroutine(_targetAIRoutine);

        // หยุด DOTween ด้วยกันบั๊กหลอดไหล
        if (targetPoint != null) targetPoint.DOKill();

        OnGameFinished?.Invoke(rewardSprite, rewardCount);
        Debug.Log(" Boiling Game Finished!");
    }

    private void Update()
    {
        if (!isReady) return;
        UpdatePlayerPhysics();
    }

    private void UpdatePlayerPhysics()
    {
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            _playerVelocity += thrust * Time.deltaTime;
        }
        else
        {
            _playerVelocity -= gravity * Time.deltaTime;
        }

        _playerVelocity = Mathf.Clamp(_playerVelocity, -maxFallSpeed, maxUpSpeed);
        playerBar.value += _playerVelocity * Time.deltaTime;

        float minVal = _normalizedGoodHalf;
        float maxVal = 1f - _normalizedGoodHalf;

        if (playerBar.value <= minVal)
        {
            playerBar.value = minVal;
            if (_playerVelocity < 0) _playerVelocity = -_playerVelocity * bounciness;
        }
        else if (playerBar.value >= maxVal)
        {
            playerBar.value = maxVal;
            if (_playerVelocity > 0) _playerVelocity = 0f;
        }
    }

    private IEnumerator TargetAIRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (isPlaying)
        {
            float currentPos = targetPoint.value;
            float absoluteMin = _normalizedGoodHalf;
            float absoluteMax = 1f - _normalizedGoodHalf;

            bool canGoUp = (currentPos + minDistanceVal) <= absoluteMax;
            bool canGoDown = (currentPos - minDistanceVal) >= absoluteMin;
            bool isGoingUp = true;

            if (canGoUp && canGoDown)
            {
                isGoingUp = Random.value > 0.5f;
            }
            else if (canGoUp)
            {
                isGoingUp = true;
            }
            else if (canGoDown)
            {
                isGoingUp = false;
            }
            else
            {
                _targetDestination = Random.Range(absoluteMin, absoluteMax);
                continue;
            }

            if (isGoingUp)
            {
                float tempMin = currentPos + minDistanceVal;
                float tempMax = absoluteMax;
                _targetDestination = Random.Range(tempMin, tempMax);
            }
            else
            {
                float tempMin = absoluteMin;
                float tempMax = currentPos - minDistanceVal;
                _targetDestination = Random.Range(tempMin, tempMax);
            }

            float distance = Mathf.Abs(targetPoint.value - _targetDestination);
            float moveDuration = distance / targetMoveSpeed;

            targetPoint.DOValue(_targetDestination, moveDuration).SetEase(Ease.InOutQuad);

            float waitTime = Random.Range(maxTargetChangeInterval, minTargetChangeInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator ScoringRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (isPlaying && currentScore < maxScore)
        {
            yield return new WaitForSeconds(scoreTickInterval);

            float distance = Mathf.Abs(playerBar.value - targetPoint.value);

            if (distance <= _normalizedPerfectHalf)
            {
                AddScore(perfectTickScore);
                OnHitQualityEvaluated?.Invoke(FireQuality.Perfect, perfectTickScore);
                OnPlaySoundTick?.Invoke();
            }
            else if (distance <= _normalizedGoodHalf)
            {
                AddScore(goodTickScore);
                OnHitQualityEvaluated?.Invoke(FireQuality.Good, goodTickScore);
                OnPlaySoundTick?.Invoke();
            }
            else
            {
                AddScore(0); // หรือหักคะแนนก็ได้
                OnHitQualityEvaluated?.Invoke(FireQuality.Miss, 0);
                // OnPlaySoundTick?.Invoke(); (ถ้า Miss ปกติอาจจะไม่เล่นเสียงติ๊งครับ)
            }
        }
    }

    private void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        if (currentScore < 0) currentScore = 0;

        OnScoreUpdated?.Invoke(currentScore);

        if (currentScore >= maxScore && isPlaying)
        {
            EndGame();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerBar != null && playerBar.handleRect != null && sliderArea != null)
        {
            Vector3 centerPosition = playerBar.handleRect.position;

            float scaleX = sliderArea.lossyScale.x;
            float scaleY = sliderArea.lossyScale.y;

            float worldWidth = sliderArea.rect.width * scaleX;

            // แก้ไขตรงนี้: ใช้ตัวแปร hitPerfectSize ตรงๆ เลย
            float worldGoodHeight = hitSize * scaleY;
            float worldPerfectHeight = hitPerfectSize * scaleY;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(centerPosition, new Vector3(worldWidth, worldGoodHeight, 0f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(centerPosition, new Vector3(worldWidth, worldPerfectHeight, 0f));
        }

        if (targetPoint != null && targetPoint.handleRect != null && sliderArea != null)
        {
            Gizmos.color = Color.red;
            float targetRadius = 15f * sliderArea.lossyScale.y;
            Gizmos.DrawWireSphere(targetPoint.handleRect.position, targetRadius);
        }
    }
}