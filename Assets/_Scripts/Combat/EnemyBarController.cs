using UnityEngine;

public class EnemyBarController : MonoBehaviour
{
    public EnemyBarUI enemyBarUIPrefab;
    public Canvas canvasWorldParant;

    public Vector3 offset;

    [HideInInspector] public EnemyBarUI enemyBarUI;

    public float showTime = 30f;
    private float timerShowing;

    [HideInInspector] public EnemyHealth enemyHealth;
    [HideInInspector] public EnemyDataBase enemy;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemy = GetComponent<EnemyDataBase>();
    }

    private void OnEnable()
    {
        enemyHealth.OnTakeDamage += HeadleTakeDamage;
    }
    private void OnDisable()
    {
        enemyHealth.OnTakeDamage -= HeadleTakeDamage;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyBarUI != null && timerShowing > 0)
        {
            timerShowing -= Time.deltaTime;

            if (timerShowing <= 0) enemyBarUI.DisableBossBar();
        }
    }

    public void HeadleTakeDamage(float damage)
    {
        if (enemyBarUI == null)
        {
            GameObject enemyBarClone = ObjectPoolingManager.Instance.Spawn(enemyBarUIPrefab.gameObject, canvasWorldParant.transform);
            enemyBarUI = enemyBarClone.GetComponent<EnemyBarUI>();
            enemyBarUI.SetData(this, enemyHealth, enemy.enemy);
        }
        timerShowing = showTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + offset, 0.1f);
    }
}
