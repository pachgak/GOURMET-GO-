using System;
using UnityEngine;

public class SpriteForeshorteningManager : MonoBehaviour
{
    public static SpriteForeshorteningManager instance;

    // *** Event ส่งค่า Multiplier ไปให้ Sprite ทุกตัว ***
    public event Action<float> OnForeshorteningUpdated;

    [Header("Camera Reference")]
    public Camera mainCamera;

    private float _lastCameraAngleX;
    private float _currentMultiplier = 1f;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        ForceUpdateForeshortening();
    }

    // แนะนำให้ใช้ LateUpdate เพราะกล้องมักจะขยับเสร็จตอน LateUpdate
    private void LateUpdate()
    {
        if (mainCamera == null) return;

        float currentAngleX = mainCamera.transform.eulerAngles.x;

        // *** Optimization: คำนวณใหม่เฉพาะตอนที่มุมกล้องขยับเท่านั้น! ***
        if (Mathf.Abs(currentAngleX - _lastCameraAngleX) > 0.01f)
        {
            ForceUpdateForeshortening();
        }
    }

    [ContextMenu("Force Update Now")]
    public void ForceUpdateForeshortening()
    {
        if (mainCamera == null) return;

        _lastCameraAngleX = mainCamera.transform.eulerAngles.x;
        float angleToCalc = _lastCameraAngleX;

        // ป้องกันค่าเข้าใกล้ 90 องศา (หารด้วย 0)
        if (angleToCalc >= 89f && angleToCalc <= 91f) angleToCalc = 89f;

        // คำนวณ Multiplier แค่ครั้งเดียวที่นี่!
        float cosTheta = Mathf.Cos(angleToCalc * Mathf.Deg2Rad);
        _currentMultiplier = 1f / cosTheta;

        // ประกาศเรียก Sprite ทุกตัวให้มารับค่าไปใช้
        OnForeshorteningUpdated?.Invoke(_currentMultiplier);
    }

    // เผื่อ Sprite เกิดใหม่ (Spawn) แล้วอยากดึงค่าไปใช้ทันทีโดยไม่ต้องรอ Event
    public float GetCurrentMultiplier()
    {
        return _currentMultiplier;
    }
}