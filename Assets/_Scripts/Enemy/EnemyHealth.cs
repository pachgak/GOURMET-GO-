// ใน Script EnemyHealth.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public class EnemyHealth : MonoBehaviour , ITakeDamage
{
    // ... (โค้ดเดิม) ...
    public float maxHealth = 100f;
    [SerializeField] public float currentHealth;
    private Rigidbody rb;
    private NavMeshAgent agent;
    private AudioSource _audioSource;
    private Collider _collider;
    private Coroutine enableAgentCoroutine; // เพิ่มตัวแปรสำหรับเก็บ Coroutine
    public bool isDead;
    public bool isRespawnNow = true;
    public AudioClip dieClip;

    //private HitEffect _hitEffect;

    public Action<float> OnTakeDamage;
    public Action<float> OnCurrentChang;
    public Action OnDie;


    public GameObject gameObjectOwner => gameObject;

    Action<float> ITakeDamage.OnTakeDamage { get => OnTakeDamage; set => OnTakeDamage = value;}

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        _collider.enabled = true;
    }

    void Start()
    {
        currentHealth = maxHealth;
        
        //_hitEffect = GetComponent<HitEffect>();

        // ตั้งค่าเริ่มต้น: ให้ Rigidbody เป็น Kinematic
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void TakeDamage(float damage)
    {
        OnTakeDamage?.Invoke(damage);

        removeHp(damage);

        //if(_hitEffect != null) _hitEffect.InstantiateEffect(damage);
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        OnDie?.Invoke();
        _collider.enabled = false;

        Debug.Log(transform.name + " has been defeated.");
        if (isRespawnNow) RetrunToPoor();

        if (_audioSource != null && dieClip != null)
        {
            _audioSource.PlayOneShot(dieClip);
        }
    }

    public void RetrunToPoor()
    {
        ObjectPoolingManager.Instance.Respawn(gameObject);
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
}