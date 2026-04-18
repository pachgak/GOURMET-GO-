using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour ,ITakeDamage
{
    [Header("Health Settings")]
    public float baseMaxHealth = 100f;
    public float currentMaxHealth = 100f;
    [SerializeField] public float currentHealth;

    public bool isDead = false;

    public Slider playerHpBar;

    private PlayerMovement _playerMovement;

    [Header("Status Effects")]
    public bool isInvincible = false; // <--- เพิ่มตัวแปรนี้

    // --- ค่า Settings สำหรับ Slow Motion ---
    [Header("Slow Motion Settings")]
    public float slowMotionDuration = 0.25f; // ระยะเวลาสโลว์โมชั่น (วินาที)
    public float slowMotionTimeScale = 0.1f;  // ค่า Time.timeScale ขณะสโลว์โมชั่น (1.0 = ปกติ)
    private float _normalTimeScale = 1.0f; // ค่า Time.timeScale ปกติ

    public Action<float, GameObject> OnTakeDamage;
    public Action<float> OnCurrentChang;
    public Action<float, float> OnHealthChanged; // <--- รวบ Event
    public Action OnDie;

    public GameObject gameObjectOwner { get => gameObject; }
    Action<float,GameObject> ITakeDamage.OnTakeDamage { get => OnTakeDamage; set => OnTakeDamage = value; }

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        currentMaxHealth = baseMaxHealth;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setHp(currentMaxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ไปที่ฟังก์ชัน TakeDamage แล้วเพิ่มการดักจับบรรทัดแรกเลย:
    public void TakeDamage(float damage, GameObject customHitVFX = null)
    {
        // *** ถ้าเป็นอมตะอยู่ ให้ยกเลิกการรับดาเมจทันที! ***
        if (isInvincible) return; 

        if (_playerMovement != null && _playerMovement.isDashing)
        {
            Debug.Log("Doge");
            StartCoroutine(HandleSlowMotion());
            return; 
        }

        OnTakeDamage?.Invoke(damage, customHitVFX);
        CameraShakeManager.instance.ShakePlayerTakeDamage();
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
        currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
        if (currentHealth <= 0) Die();
    }

    public void addHp(float heal)
    {
        currentHealth += heal;
        currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
    }

    public void setHp(float value)
    {
        currentHealth = value;
        currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log(transform.name + " has been defeated.");
        isDead = true;
        //Destroy(gameObject);
        OnDie?.Invoke();
    }

    public void RestoreMaxHP()
    {
        currentHealth = currentMaxHealth;
        isDead = false;
    }
}
