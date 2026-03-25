using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBarUI : MonoBehaviour
{
    public Slider hpBar;
    public TMP_Text hpCountText;
    public TMP_Text nameText;


    private EnemyHealth _healthTarget;
    private EnemyBarController _enemyBarController;

    //public float timeBossBar;
    //private float _timer;

    private void Awake()
    {

    }

    public void HeadledTakeDamaget(float hpCurrent)
    {
        ShowHp(hpCurrent);
    }

    public void HeadleDie()
    {
        DisableBossBar();
    }

    // Update is called once per frame
    void Update()
    {
        if(_enemyBarController != null) transform.position = (_enemyBarController.transform.position + _enemyBarController.offset);
        //if (_timer > 0)
        //{
        //    _timer -= Time.deltaTime;

        //    if(_timer <= 0) enabled = false;
        //}
    }

    private void OnEnable()
    {
        if (_healthTarget != null)
        {
            ResingTarget();
        }
    }

    // เปลี่ยนพารามิเตอร์ตัวสุดท้ายเป็น string enemyName = "" (กำหนดค่าเริ่มต้นเป็นค่าว่างเผื่อไม่ส่งมา)
    public void SetData(EnemyBarController bossBarController, EnemyHealth healthTarget, string enemyName = "")
    {
        if (_healthTarget != null) ResingTarget();
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        _healthTarget = healthTarget;
        _enemyBarController = bossBarController; // สำหรับ EnemyBarUI จะเป็น _enemyBarController

        _healthTarget.OnCurrentChang += HeadledTakeDamaget;
        _healthTarget.OnDie += HeadleDie;

        _enemyBarController.isShowing = true;

        hpBar.maxValue = _healthTarget.maxHealth;

        // --- ส่วนที่ปรับปรุงใหม่ ---
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
                nameText.gameObject.SetActive(false); // ซ่อนชื่อถ้าไม่มีการส่งมา
            }
        }
        // -------------------------

        ShowHp(_healthTarget.currentHealth);
    }

    private void ShowHp(float currentHp)
    {
        hpBar.value = currentHp;
        hpCountText.text = $"{currentHp} / {hpBar.maxValue}";
    }

    public void DisableBossBar()
    {
        ResingTarget();

        ReturnObjectToPool();
        //gameObject.SetActive(false);
    }

    public void ReturnObjectToPool()
    {
        ObjectPoolingManager.Instance.Respawn(this.gameObject);
    }

    private void ResingTarget()
    {
        _healthTarget.OnCurrentChang -= HeadledTakeDamaget;
        _healthTarget.OnDie -= HeadleDie;

        _enemyBarController.enemyBarUI = null;

        _healthTarget = null;
        _enemyBarController = null;
    }
}

