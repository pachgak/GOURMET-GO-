using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseHpBarUI : MonoBehaviour
{
    public Slider hpBar;
    public TMP_Text hpCountText;
    public TMP_Text nameText;

    protected EnemyHealth _healthTarget;

    // รับค่า Health และ ชื่อ (ถ้ามี)
    public virtual void SetData(EnemyHealth healthTarget, string enemyName = "")
    {
        if (_healthTarget != null) ResetTarget();
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        _healthTarget = healthTarget;

        // UI สนใจแค่ตอนเลือดลดลง/เพิ่มขึ้น
        _healthTarget.OnCurrentChang += HandleHealthChange;

        hpBar.maxValue = _healthTarget.maxHealth;

        // จัดการเรื่องชื่อ
        if (nameText != null)
        {
            if (!string.IsNullOrEmpty(enemyName))
            {
                nameText.text = enemyName;
                nameText.gameObject.SetActive(true);
            }
            else
            {
                nameText.text = "";
                nameText.gameObject.SetActive(false);
            }
        }

        ShowHp(_healthTarget.currentHealth);
    }

    protected virtual void HandleHealthChange(float hpCurrent)
    {
        ShowHp(hpCurrent);
    }

    protected void ShowHp(float currentHp)
    {
        hpBar.value = currentHp;
        if (hpCountText != null) hpCountText.text = $"{currentHp} / {hpBar.maxValue}";
    }

    // ฟังก์ชันสำหรับซ่อนหลอดเลือด (ให้คลาสลูกไปเขียนต่อว่าจะซ่อนแบบไหน)
    public virtual void DisableBar()
    {
        ResetTarget();
        gameObject.SetActive(false);
    }

    // ยกเลิกการผูก Event เพื่อป้องกัน Memory Leak
    protected virtual void ResetTarget()
    {
        if (_healthTarget != null)
        {
            _healthTarget.OnCurrentChang -= HandleHealthChange;
            _healthTarget = null;
        }
    }
}