using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour ,ITakeDamage
{
    public float maxHealth = 100f;
    [SerializeField] public float currentHealth;

    public bool isDead = false;

    public Slider playerHpBar;

    private PlayerMovement _playerMovement;

    // --- ค่า Settings สำหรับ Slow Motion ---
    [Header("Slow Motion Settings")]
    public float slowMotionDuration = 0.25f; // ระยะเวลาสโลว์โมชั่น (วินาที)
    public float slowMotionTimeScale = 0.1f;  // ค่า Time.timeScale ขณะสโลว์โมชั่น (1.0 = ปกติ)
    private float _normalTimeScale = 1.0f; // ค่า Time.timeScale ปกติ

    public Action<float> OnTakeDamage;
    public Action<float> OnCurrentChang;

    public GameObject gameObjectOwner { get => gameObject; }
    Action<float> ITakeDamage.OnTakeDamage { get => OnTakeDamage; set => OnTakeDamage = value; }

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setHp(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        if (_playerMovement != null && _playerMovement.isDashing)
        {
            Debug.Log("Doge");

            // ถ้ากำลัง Dash ให้ทำ Slow Motion
            StartCoroutine(HandleSlowMotion());

            // เนื่องจากคุณต้องการให้มัน 'return' (ไม่รับดาเมจ) ในโค้ดเดิม
            // ถ้าต้องการให้รับดาเมจด้วย ให้ย้าย removeHp(damage); เข้ามาที่นี่
            return; // ไม่รับดาเมจตามโค้ดเดิม
        }

        OnTakeDamage?.Invoke(damage);
        removeHp(damage);

        
    }

    IEnumerator HandleSlowMotion()
    {
        // 1. ตั้งค่า TimeScale ให้เป็น Slow Motion
        Time.timeScale = slowMotionTimeScale;

        // 2. รอตามระยะเวลาที่ตั้งไว้
        // ใช้ WaitForSecondsRealtime เพื่อให้ Coroutine นับเวลาตามเวลาจริง ไม่ใช่เวลาที่ถูกสโลว์
        yield return new WaitForSecondsRealtime(slowMotionDuration);

        // 3. ตั้งค่า TimeScale กลับเป็นปกติ
        Time.timeScale = _normalTimeScale;
    }

    public void removeHp(float damage)
    {
        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnCurrentChang?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void addHp(float heal)
    {
        currentHealth += heal;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnCurrentChang?.Invoke(currentHealth);
    }

    public void setHp(float value)
    {
        currentHealth = value;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnCurrentChang?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(transform.name + " has been defeated.");
        isDead = true;
        //Destroy(gameObject);
    }

}
