using UnityEngine;

// สืบทอดจาก EnemySoundController แทน MonoBehaviour
public class LatchEnemySoundController : EnemySoundController
{
    [Header("Latch Specific Audio")]
    private LatchController _latchController;
    [SerializeField] private AudioClip latchClip; // เสียงตอนเกาะหัว

    private bool _isLatched = false;

    protected override void Awake()
    {
        base.Awake(); // เรียกใช้งานการหาค่าเริ่มต้นจากตัวแม่ (AudioSource, Movement)
        _latchController = GetComponent<LatchController>();
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // ให้ตัวแม่ไปสมัคร Event การเดินตามปกติ
        if (_latchController != null)
        {
            _latchController.OnLatchStateChanged += HandleLatchSound; // สมัคร Event เกาะหัวเพิ่ม
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_latchController != null)
        {
            _latchController.OnLatchStateChanged -= HandleLatchSound;
        }
    }

    private void HandleLatchSound(bool isLatched)
    {
        _isLatched = isLatched;

        if (isLatched)
        {
            // ตอนเกาะหัว ให้เล่นเสียง Latch วนลูป
            _audioSource.clip = latchClip;
            _audioSource.loop = true;
            _audioSource.Play();
        }
        else
        {
            // ถ้าหลุดจากการเกาะ ให้หยุดเสียงเกาะ
            if (_audioSource.clip == latchClip)
            {
                _audioSource.Stop();
            }
        }
    }

    // *** หัวใจสำคัญของการ Override ***
    // เราเขียนทับฟังก์ชันเดิน เพื่อเช็คว่า "ถ้าเกาะหัวอยู่ ห้ามเล่นเสียงเดินแทรกเด็ดขาด"
    protected override void HandleWalkSound(bool isMoving)
    {
        if (_isLatched) return;

        // ถ้าไม่ได้เกาะหัวอยู่ ก็ปล่อยให้ลอจิกการเล่นเสียงเดินของตัวแม่ทำงานไปตามปกติ
        base.HandleWalkSound(isMoving);
    }
}