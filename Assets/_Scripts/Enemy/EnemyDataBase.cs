using UnityEngine;

public class EnemyDataBase : MonoBehaviour
{
    public EnemySO enemy;

    public EnemyHealth _enemyHealth;
    private void Awake()
    {
        _enemyHealth = GetComponent<EnemyHealth>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _enemyHealth.maxHealth = enemy.hp;
        _enemyHealth.setHp(enemy.hp);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
