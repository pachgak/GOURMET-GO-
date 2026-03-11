using UnityEngine;
using UnityEngine.UI;

public class UI_HalfIngredient : MonoBehaviour
{
    public float bottomYLimit = -800f; // ขอบจอด้านล่าง

    private Vector2 velocity; // ความเร็วกระเด็น
    private float rotateSpeed; // ความเร็วการหมุน
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // ฟังก์ชันนี้ให้ UI_Ingredient ตัวแม่ เป็นคนเรียกเพื่อตั้งค่า
    public void Setup(Sprite sprite, int fillOrigin, Vector2 startVelocity, float rotSpeed, float targetFillAmount)
    {
        Image img = GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = fillOrigin;

        // ให้มันตัดภาพตามสัดส่วนที่ส่งมาเป๊ะๆ!
        img.fillAmount = targetFillAmount;

        velocity = startVelocity;
        rotateSpeed = rotSpeed;
    }

    void Update()
    {
        // 1. เพิ่มแรงโน้มถ่วงให้ตกลงมา (ให้ค่าติดลบเพิ่มขึ้นเรื่อยๆ)
        velocity.y -= 2500f * Time.deltaTime;

        // 2. เคลื่อนที่
        rectTransform.anchoredPosition += velocity * Time.deltaTime;

        // 3. หมุนแกน Z
        rectTransform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        // 4. เช็คตกจอแล้วทำลายทิ้ง
        if (rectTransform.anchoredPosition.y < bottomYLimit)
        {
            Destroy(gameObject);
        }
    }
}