using UnityEngine;

public abstract class BaseHpBarController : MonoBehaviour
{
    public float showTime = 30f;
    protected float timerShowing;

    protected EnemyHealth enemyHealth;
    protected EnemyDataBase enemyDataBase;
    protected Collider _collider; // 1. เพิ่มตัวแปรสำหรับเก็บ Collider

    protected virtual void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyDataBase = GetComponent<EnemyDataBase>();
        _collider = GetComponent<Collider>(); // 2. ดึงค่า Collider ใน Awake
    }

    protected virtual void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnTakeDamage += HandleTakeDamage;
            enemyHealth.OnDie += HandleDie;
        }
    }

    protected virtual void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnTakeDamage -= HandleTakeDamage;
            enemyHealth.OnDie -= HandleDie;
        }

        // --- แก้ปัญหาที่ 1 (Object Pool): ซ่อน UI ทันทีเมื่อ Object ถูก SetActive(false) ---
        timerShowing = 0;
        HideUI();
    }

    protected virtual void Update()
    {
        // --- แก้ปัญหาที่ 2 (ล่องหน): เช็คว่าถ้า Collider ปิดอยู่ ให้บังคับปิด UI ด้วย ---
        if (_collider != null && !_collider.enabled)
        {
            // ถ้า UI ยังโชว์อยู่ (timerShowing > 0) ให้ทำการปิดมัน
            if (timerShowing > 0)
            {
                timerShowing = 0;
                HideUI();
            }
            return; // หยุดการทำงาน Update ไม่ต้องรันนับเวลาต่อ
        }

        // ระบบนับเวลาเดิม
        if (timerShowing > 0)
        {
            timerShowing -= Time.deltaTime;
            if (timerShowing <= 0)
            {
                HideUI(); // หมดเวลาให้ซ่อน UI
            }
        }
    }

    public virtual void HandleTakeDamage(float damage)
    {
        // ป้องกันบัค: ถ้ามีระบบโจมตีทะลุล่องหน แล้วศัตรูโดนดาเมจตอน Collider ปิดอยู่ ก็ไม่ต้องโชว์หลอดเลือด
        if (_collider != null && !_collider.enabled) return;

        timerShowing = showTime; // รีเซ็ตเวลา

        string eName = "";
        if (enemyDataBase != null && enemyDataBase.enemy != null)
        {
            eName = enemyDataBase.enemy.enemyName;
        }

        ShowUI(eName);
    }

    protected virtual void HandleDie()
    {
        timerShowing = 0;
        HideUI(); // ศัตรูตาย ให้ซ่อน UI ทันที
    }

    protected abstract void ShowUI(string enemyName);
    protected abstract void HideUI();
}