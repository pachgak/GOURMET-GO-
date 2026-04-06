using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("ลาก Sprite ขา แขน หัว ตัว มาใส่ ")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white;

    [Tooltip("ระยะเวลาทั้งหมดที่ตัวละครจะกระพริบ (วินาที)")]
    [SerializeField] private float totalFlashDuration = 0.6f;

    [Tooltip("จำนวนครั้งที่ต้องการให้กระพริบภายในระยะเวลาด้านบน")]
    [SerializeField] private int flashCount = 1;

    [Tooltip("ทำให้มันค่อยจางหายไป")]
    [SerializeField] bool isLerp = true;

    private MaterialPropertyBlock propertyBlock;
    private int flashAmountProperty;
    private int flashColorProperty;

    private Coroutine flashCoroutine;
    private ITakeDamage takeDamageComponent;

    void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        flashAmountProperty = Shader.PropertyToID("_FlashAmount");
        flashColorProperty = Shader.PropertyToID("_FlashColor");

        takeDamageComponent = GetComponentInParent<ITakeDamage>();

        if (takeDamageComponent == null)
        {
            Debug.LogWarning("DamageFlash: ไม่พบ Component ที่มี ITakeDamage บน " + gameObject.name);
        }

    }

    private void OnEnable()
    {
        if (takeDamageComponent != null)
        {
            takeDamageComponent.OnTakeDamage += HandleTakeDamage;
        }
    }

    private void OnDisable()
    {
        if (takeDamageComponent != null)
        {
            takeDamageComponent.OnTakeDamage -= HandleTakeDamage;
        }
    }

    private void HandleTakeDamage(float damageAmount)
    {
        PlayHitFlash();
    }

    [ContextMenu("Test Hit Flash")]
    public void PlayHitFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = (isLerp) ? StartCoroutine(FlashRoutineLerp()) : StartCoroutine(FlashRoutineNormal());
    }

    private IEnumerator FlashRoutineNormal()
    {
        float flashInterval = totalFlashDuration / (flashCount * 2f);

        for (int i = 0; i < flashCount; i++)
        {
            // จังหวะที่ 1: สว่างเป็นสีแฟลช
            ApplyFlashState(1f, flashColor);
            yield return new WaitForSeconds(flashInterval);

            // จังหวะที่ 2: ดับกลับเป็นสีปกติ
            ApplyFlashState(0f, flashColor);
            yield return new WaitForSeconds(flashInterval);
        }

        // เพื่อความชัวร์ตอนจบ สั่งให้ทุกชิ้นส่วนกลับเป็นสีปกติ
        ApplyFlashState(0f, flashColor);
        flashCoroutine = null;
    }

    private IEnumerator FlashRoutineLerp()
    {
        // เริ่มต้นด้วยการจับเวลา
        float elapsedTime = 0f;

        // วนลูปทำงานทุกเฟรม จนกว่าเวลาที่ผ่านไป จะมากกว่าเวลาที่เราตั้งไว้
        while (elapsedTime < totalFlashDuration)
        {
            // elapsedTime / totalFlashDuration จะได้ค่า 0.0 ถึง 1.0 (คิดเป็น % ของเวลาที่ผ่านไป)
            // Mathf.Lerp(1f, 0f, %) จะค่อยๆ เปลี่ยนค่าจาก 1 ลดลงไปหา 0 อย่างนุ่มนวล
            float currentFlashAmount = Mathf.Lerp(1f, 0f, (elapsedTime / totalFlashDuration));

            // นำค่าที่ค่อยๆ ลดลง ไปอัปเดตให้ Material
            ApplyFlashState(currentFlashAmount, flashColor);

            // บวกเวลาที่ผ่านไปในเฟรมนี้
            elapsedTime += Time.deltaTime;

            // yield return null หมายถึง "หยุดพักตรงนี้ก่อน แล้วเดี๋ยวค่อยมารันต่อในเฟรมถัดไป"
            yield return null;
        }

        // เพื่อความชัวร์ตอนจบ สั่งให้ทุกชิ้นส่วนกลับเป็นสีปกติ (Amount = 0)
        ApplyFlashState(0f, flashColor);
        flashCoroutine = null;
    }

    // --- ฟังก์ชันตัวช่วยสำหรับวนลูปจัดการทุกชิ้นส่วนพร้อมกัน ---
    private void ApplyFlashState(float amount, Color color)
    {
        // วนลูปเข้าไปจัดการ SpriteRenderer แต่ละชิ้นใน Array
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null) // เช็คไว้เผื่อในเกมมีระบบแขนขาด/ขาขาด แล้วชิ้นส่วนโดนทำลายไปแล้ว
            {
                sr.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(flashColorProperty, color);
                propertyBlock.SetFloat(flashAmountProperty, amount);
                sr.SetPropertyBlock(propertyBlock);
            }
        }
    }
}