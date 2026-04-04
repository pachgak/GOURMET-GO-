using UnityEngine;

public class SelfLightningDetector : MonoBehaviour
{
    private BaseHitBox _baseHitBox;
    private float _checkInterval = 0.1f;
    private float _timer = 0f;

    // *** 1. เพิ่มตัวแปร Flag เช็คการทำงาน ***
    private bool _hasTriggered = false;

    private void Awake()
    {
        _baseHitBox = GetComponent<BaseHitBox>();
    }

    private void OnEnable()
    {
        _timer = 0f;
        // *** 2. รีเซ็ตค่าทุกครั้งที่ถูกดึงมาจาก Object Pool ***
        _hasTriggered = false;
    }

    private void Update()
    {
        // *** 3. ถ้าชาร์จพลังให้บอสไปแล้ว ก็ไม่ต้องทำอะไรอีก ***
        if (_hasTriggered) return;

        if (_baseHitBox == null || _baseHitBox.ownerHit == null) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            CheckSelfHit();
            _timer = _checkInterval;
        }
    }

    private void CheckSelfHit()
    {
        // สั่งให้ Hitbox คืนค่าคนที่อยู่ในวงมาให้หมด! 
        Collider[] hits = _baseHitBox.GetCollidersInArea(~0);

        foreach (var hit in hits)
        {
            // ถ้าคนที่โดน คือคนที่เสกสายฟ้านี้ (ตัวบอส)
            if (hit.gameObject == _baseHitBox.ownerHit)
            {
                if (hit.gameObject.TryGetComponent(out ZanderCombat zander))
                {
                    zander.TakeSelfLightning(); // ส่งประจุชาร์จเข้าบอส

                    // *** 4. เปลี่ยนมาเปิด Flag แทนการปิด Script ***
                    _hasTriggered = true;
                    return;
                }
            }
        }
    }
}