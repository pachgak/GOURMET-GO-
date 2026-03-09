using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ingredient : MonoBehaviour
{
    [Header("Physics Settings")]
    public float bottomYLimit = -800f;

    private float _gravity = 2500f;
    private RectTransform _rectTransform;
    private Vector2 _currentVelocity;
    private Image _image;

    // Property ให้ Manager หรือ Script อื่นๆ เข้าถึงได้
    public RectTransform Rect => _rectTransform;
    public Image IngredientImage => _image; // เปิดให้ดึง Image ไปใช้ได้

    public Action<UI_Ingredient> OnMissTarget;

    // Action ใหม่สำหรับตอนโดนฟัน
    public Action<UI_Ingredient> OnHitDestroySelf;

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

    void Update()
    {
        _currentVelocity.y -= _gravity * Time.deltaTime;
        _rectTransform.anchoredPosition += _currentVelocity * Time.deltaTime;

        if (_rectTransform.anchoredPosition.y < bottomYLimit)
        {
            MissTarget();
        }
    }

    void MissTarget()
    {
        OnMissTarget?.Invoke(this);
        Destroy(gameObject);
    }

    public void HitDestroySelf()
    {
        // ตะโกนบอกสคริปต์อื่น (เช่นตัว Splitter) ให้เสก Effect ซีกซ้ายขวา
        OnHitDestroySelf?.Invoke(this);

        // ทำลายตัวเอง
        Destroy(gameObject);
    }
}