using UnityEngine;
using System.Collections;

public class SlashEffectController : MonoBehaviour
{
    [Header("Effects")]
    public GameObject slashEffect1;
    public GameObject slashEffect2;
    public float slashDuration = 0.15f;

    [Header("Rotation Settings")]
    public bool randomizeRotation = true; // เปิด/ปิด การสุ่มหมุนรอยฟัน

    [Tooltip("องศาต่ำสุดที่จะบิดเบี้ยวจากทรงเดิม (ค่าติดลบคือทวนเข็ม)")]
    public float minRotationOffset = -20f;

    [Tooltip("องศาสูงสุดที่จะบิดเบี้ยวจากทรงเดิม (ค่าบวกคือตามเข็ม)")]
    public float maxRotationOffset = 20f;

    // ตัวแปรซ่อนไว้เก็บค่าองศา Z ดั้งเดิม
    private float _originRotationZ1;
    private float _originRotationZ2;

    void Awake()
    {
        // 1. เก็บค่า Rotation Z ดั้งเดิมของเอฟเฟคแต่ละอันไว้ก่อนปิดการมองเห็น
        if (slashEffect1 != null)
        {
            _originRotationZ1 = slashEffect1.transform.localEulerAngles.z;
            slashEffect1.SetActive(false);
        }

        if (slashEffect2 != null)
        {
            _originRotationZ2 = slashEffect2.transform.localEulerAngles.z;
            slashEffect2.SetActive(false);
        }
    }

    public void PlaySlashEffect()
    {
        // เปิด GameObject ให้มองเห็น
        if (slashEffect1 != null) slashEffect1.SetActive(true);
        if (slashEffect2 != null) slashEffect2.SetActive(true);

        // สุ่มหมุนแกน Z โดยเอาไปบวกกับค่า Origin
        if (randomizeRotation)
        {
            // สุ่มค่าองศาที่จะเอาไปบิดเพิ่ม (Offset) แยกของใครของมัน
            float randomOffset = Random.Range(minRotationOffset, maxRotationOffset);

            // เซ็ตค่าให้ = ค่าเดิมตั้งต้น + ค่าที่สุ่มได้
            if (slashEffect1 != null)
                slashEffect1.transform.localRotation = Quaternion.Euler(0, 0, _originRotationZ1 + randomOffset);

            if (slashEffect2 != null)
                slashEffect2.transform.localRotation = Quaternion.Euler(0, 0, _originRotationZ2 + randomOffset);
        }

        // หยุด Coroutine เก่า (ถ้ามี) แล้วเริ่มใหม่
        StopAllCoroutines();
        StartCoroutine(HideSlashEffectRoutine());
    }

    private IEnumerator HideSlashEffectRoutine()
    {
        yield return new WaitForSeconds(slashDuration);

        // ปิด GameObject ให้หายไป
        if (slashEffect1 != null) slashEffect1.SetActive(false);
        if (slashEffect2 != null) slashEffect2.SetActive(false);
    }
}