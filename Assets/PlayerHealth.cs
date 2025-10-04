using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour ,ITakeDamage
{
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public bool isDead = false;

    public Action<float> OnTakeDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnTakeDamage?.Invoke(damage);

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
