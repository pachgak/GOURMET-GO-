using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ingredient : MonoBehaviour
{
    [Header("Physics Settings")]
    public float leftXLimit = -1000f;
    public float leftXMiss = -750f;

    private float _gravity = 2500f;
    private RectTransform _rectTransform;
    private Vector2 _currentVelocity;
    private Image _image;

    // ตัวแปรใหม่สำหรับคุมการหมุน
    private float _angularVelocity;

    public RectTransform Rect => _rectTransform;
    public Image IngredientImage => _image;

    public Action<UI_Ingredient> OnMissTarget;
    public Action<UI_Ingredient, float> OnHitDestroySelf;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    public void SetGravity(float setGravity)
    {
        _gravity = setGravity;
    }

    public void SetVelocity(Vector2 startVelocity)
    {
        _currentVelocity = startVelocity;
    }

    // ฟังก์ชันใหม่สำหรับตั้งค่าการหมุนให้มาจบตรงเป้าพอดี
    public void SetRotation(float startOffset, float timeToReachTarget)
    {
        // 1. เก็บองศาเดิมของ Prefab (สมมติว่าจัดไว้ให้ชี้ขึ้นบนตรงๆ)
        float originRotationZ = _rectTransform.localEulerAngles.z;

        // 2. จับหมุนไปที่จุดเริ่มต้น (องศาเดิม - ค่าที่อยากให้เบี้ยวไปตอนเกิด)
        float startRotationZ = originRotationZ - startOffset;
        _rectTransform.localRotation = Quaternion.Euler(0, 0, startRotationZ);

        // 3. คำนวณความเร็วหมุน (เมื่อเวลาผ่านไป = timeToReachTarget มันจะหมุนกลับมาเท่า origin พอดีเป๊ะ)
        _angularVelocity = startOffset / timeToReachTarget;
    }

    void Update()
    {
        // --- ส่วนการเคลื่อนที่ ---
        _currentVelocity.y -= _gravity * Time.deltaTime;
        _rectTransform.anchoredPosition += _currentVelocity * Time.deltaTime;

        // --- ส่วนการหมุน ---
        if (_angularVelocity != 0)
        {
            _rectTransform.Rotate(0, 0, _angularVelocity * Time.deltaTime);
        }

        // --- เช็คหลุดระยะตี ---
        if (_rectTransform.anchoredPosition.x < leftXMiss)
        {
            MissTarget();
        }

        // --- เช็คหลุดขอบจอซ้าย ---
        if (_rectTransform.anchoredPosition.x < leftXLimit)
        {
            Destroy(gameObject);
        }
    }

    void MissTarget()
    {
        GetComponent<Image>().color = Color.gray;

        OnMissTarget?.Invoke(this);
    }

    public void HitDestroySelf(float rightFillAmount)
    {
        // ตะโกนบอก Splitter พร้อมแนบค่า Fill ไปให้
        OnHitDestroySelf?.Invoke(this, rightFillAmount);
        Destroy(gameObject);
    }
}