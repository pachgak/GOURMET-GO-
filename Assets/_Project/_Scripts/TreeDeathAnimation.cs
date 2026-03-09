using UnityEngine;
using DG.Tweening; // อย่าลืมใส่ DOTween

public class TreeDeathAnimation : MonoBehaviour
{
    [Header("Dependencies")]
    private EnemyHealth _enemyHealth;
    private SpawnItemDropPoor _spawnItemScript;
    public Transform visualChild; // ลากตัว Graphics มาใส่

    [Header("Animation Settings")]
    public float fallDuration = 1.0f; // เวลาที่ใช้ในการล้ม
    public float destroyDelay = 0.5f; // เวลาหน่วงหลังดรอปของเสร็จก่อนตัวหายไป
    public float endAngle = -90f; // เวลาหน่วงหลังดรอปของเสร็จก่อนตัวหายไป

    private Quaternion initialRotation; // เก็บค่าการหมุนเดิมไว้

    private void Awake()
    {
        if (_enemyHealth == null) _enemyHealth = GetComponent<EnemyHealth>();
        if (_spawnItemScript == null) _spawnItemScript = GetComponent<SpawnItemDropPoor>();
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

        // จำค่าการหมุนเริ่มต้น (ตอนต้นไม้ตั้งตรง)
        initialRotation = visualChild.localRotation;
    }

    private void Start()
    {
        if (_spawnItemScript != null) _enemyHealth.OnDie -= _spawnItemScript.HealdeDropPoorItems;
    }

    private void OnEnable()
    {
        // 1. Reset การหมุนกลับมาตั้งตรงทุกครั้งที่ Respawn
        visualChild.localRotation = initialRotation;

        // 2. Subscribe Event เมื่อตาย
        if (_enemyHealth != null)
        {
            _enemyHealth.OnDie += PlayFallAnimation;
        }
    }

    private void OnDisable()
    {
        if (_enemyHealth != null)
        {
            _enemyHealth.OnDie -= PlayFallAnimation;
        }

        // Kill tween เพื่อความปลอดภัย
        visualChild.DOKill();
    }

    private void PlayFallAnimation()
    {
        // ใช้ DOTween หมุนแกน Z ไป -90 องศา (ล้มลง)
        // SetRelative(true) เพื่อให้หมุนเพิ่มจากเดิม หรือจะใช้แบบเจาะจงก็ได้
        // ตรงนี้ผมใช้แบบหมุนไปหาค่า -90 ตรงๆ 
        Vector3 targetRotation = new Vector3(visualChild.localEulerAngles.x, visualChild.localEulerAngles.y, endAngle);

        visualChild.DOLocalRotate(targetRotation, fallDuration)
            .SetEase(Ease.InBack) // Ease.InBack จะทำให้มีการโยกนิดนึงก่อนล้ม ดูมีน้ำหนัก
            .OnComplete(() =>
            {
                // 1. เมื่อล้มเสร็จ -> สั่งดรอปของ
                if (_spawnItemScript != null)
                {
                    _spawnItemScript.HealdeDropPoorItems();
                }

                // 2. หน่วงเวลาแป๊บนึง แล้วค่อยเอา Object กลับลง Pool
                DOVirtual.DelayedCall(destroyDelay, () =>
                {
                    if (_enemyHealth != null)
                    {
                        //_enemyHealth.RetrunToPoor();
                    }
                });
            });
    }
}