using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting; // สำหรับใช้ Coroutine

public class PickUpPromptUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    private CanvasGroup canvasGroup; // ได้จาก component Canvas Group

    [Header("Fade Color")]
    public Color colorAdd = Color.green;
    public Color colorRemove = Color.red;

    [Header("Fade Settings")]
    [SerializeField] private float showDuration = 1.5f; // ระยะเวลาคงที่ก่อนเริ่มเฟด
    [SerializeField] private float fadeDuration = 0.5f; // ระยะเวลาในการเฟดหายไป

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    /// <summary>
    /// กำหนดค่าเริ่มต้นของ UI และเริ่มกระบวนการแสดงผล
    /// </summary>
    public void SetupAndShow(Sprite icon, int quantity)
    {
        // 1. ตั้งค่าข้อมูล
        itemIcon.sprite = icon;
        quantityText.text = (quantity >= 0) ? $"Get +{Mathf.Abs(quantity)} ea": $"Remove -{Mathf.Abs(quantity)} ea";  // แสดงเป็น "+X"
        quantityText.color = (quantity >= 0) ? colorAdd : colorRemove;

        canvasGroup.alpha = 1f; // ทำให้ UI แสดงผลทันที
        transform.SetAsFirstSibling();

        // 2. เริ่ม Coroutine เพื่อควบคุมการหายไป
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutRoutine());

        transform.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
    }

    private IEnumerator FadeOutRoutine()
    {
        // 1. รอตามเวลาที่กำหนด (Show Duration)
        yield return new WaitForSeconds(showDuration);

        // 2. เริ่มกระบวนการเฟดเอาท์
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // คำนวณค่า alpha จาก 1.0 (ทึบ) ไป 0.0 (โปร่งใส)
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null; // รอจนกว่าจะถึงเฟรมถัดไป
        }

        // 3. สิ้นสุดกระบวนการ
        canvasGroup.alpha = 0f;
        // ทำลาย GameObject เมื่อเฟดหายไปแล้ว
        ReturnObjectToPool();
    }

    private void ReturnObjectToPool()
    {
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }
}