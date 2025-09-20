// ใน Script EnemyHealth.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyHealth : MonoBehaviour , ITakeDamage
{
    // ... (โค้ดเดิม) ...
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    private Rigidbody rb;
    private NavMeshAgent agent;
    private Coroutine enableAgentCoroutine; // เพิ่มตัวแปรสำหรับเก็บ Coroutine

    private HitEffect _hitEffect;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        _hitEffect = GetComponent<HitEffect>();

        // ตั้งค่าเริ่มต้น: ให้ Rigidbody เป็น Kinematic
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void TakeDamage(float damage)
    {

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }

        if(_hitEffect != null) _hitEffect.InstantiateEffect(damage);
    }

    private void Die()
    {
        Debug.Log(transform.name + " has been defeated.");
        Destroy(gameObject);
    }
}