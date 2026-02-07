using UnityEngine;
using DG.Tweening; // อย่าลืม using DOTween

public class FoodTreeAnimation : MonoBehaviour
{
    [Header("Dependencies")]
    private FoodTree _foodTree;
    public Transform visualChild; // ลากตัว Graphics มาใส่

    [Header("Animation Settings")]
    [Tooltip("ย่อลงไปเหลือเท่าไหร่ (เช่น 0.7 คือย่อเหลือ 70%)")]
    public float squashScale = 0.7f;
    [Tooltip("เวลาช่วงทีย่อลง (วินาที)")]
    public float squashDuration = 0.1f;
    [Tooltip("เวลาช่วงที่ดีดกลับ (วินาที)")]
    public float bounceDuration = 0.4f;

    private Vector3 initialScale;

    private void Awake()
    {
        if (_foodTree == null) _foodTree = GetComponent<FoodTree>();
        if (visualChild == null)
        {
            // ลองหาจาก SpriteRenderer ในลูกๆ
            SpriteRenderer spriteInChild = GetComponentInChildren<SpriteRenderer>();

            if (spriteInChild != null)
            {
                visualChild = spriteInChild.transform;
            }
            else
            {
                // ถ้าหาไม่เจอจริงๆ ค่อยใช้ท่าไม้ตาย GetChild(0) หรือใช้ตัวเอง
                if (transform.childCount > 0)
                    visualChild = transform.GetChild(0);
                else
                    visualChild = transform; // ไม่มีลูก ก็ขยับตัวเอง (กัน Error)
            }
        }

        // จำค่า Scale เริ่มต้นไว้ (เผื่อต้นไม้แต่ละต้นขนาดไม่เท่ากัน)
        initialScale = visualChild.localScale;

    }

    private void OnEnable()
    {
        if (_foodTree != null)
        {
            _foodTree.OnPick += PlayPickAnimation;
        }
    }

    private void OnDisable()
    {
        if (_foodTree != null)
        {
            _foodTree.OnPick -= PlayPickAnimation;
        }

        // ควร Kill tween เสมอเมื่อ object ถูกปิด เพื่อกัน error
        visualChild.DOKill();
    }

    private void PlayPickAnimation()
    {
        // หยุด Tween เก่าก่อน (ถ้ามี) แล้ว reset scale
        visualChild.DOKill(true);
        visualChild.localScale = initialScale;

        // Sequence คือการลำดับเหตุการณ์: ย่อลง -> ดีดกลับ
        Sequence seq = DOTween.Sequence();

        // 1. Squash: ย่อแกน Y ลงอย่างรวดเร็ว (Scale Y ลดลง, Scale X อาจจะป่องออกนิดนึงก็ได้แต่เอาแค่ Y ตามที่ขอ)
        seq.Append(visualChild.DOScaleY(initialScale.y * squashScale, squashDuration).SetEase(Ease.OutQuad));

        // 2. Bounce Back: ดีดกลับไปค่าเดิมแบบเด้งดึ๋ง (Elastic)
        seq.Append(visualChild.DOScaleY(initialScale.y, bounceDuration).SetEase(Ease.OutElastic));
    }
}