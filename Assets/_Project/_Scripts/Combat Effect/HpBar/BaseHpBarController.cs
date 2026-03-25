using UnityEngine;

public abstract class BaseHpBarController : MonoBehaviour
{
    public float showTime = 30f;
    protected float timerShowing;

    protected EnemyHealth enemyHealth;
    protected EnemyDataBase enemyDataBase;

    protected virtual void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyDataBase = GetComponent<EnemyDataBase>();
    }

    protected virtual void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnTakeDamage += HandleTakeDamage;
            enemyHealth.OnDie += HandleDie; // Controller เป็นคนฟังสั่งตายเอง
        }
    }

    protected virtual void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnTakeDamage -= HandleTakeDamage;
            enemyHealth.OnDie -= HandleDie;
        }
    }

    protected virtual void Update()
    {
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

    // บังคับให้คลาสลูกต้องเขียนระบบ Show/Hide ของตัวเอง
    protected abstract void ShowUI(string enemyName);
    protected abstract void HideUI();
}