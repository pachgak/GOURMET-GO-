using UnityEngine;

public class BossBarController : MonoBehaviour
{
    public BossBarUI bossBarUI;
    [HideInInspector] public bool isShowing = false;

    [HideInInspector] public EnemyHealth enemyHealth;
    [HideInInspector] public EnemyDataBase enemy;


    public float showTime = 30f;
    private float timerShowing;

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
        if (isShowing && timerShowing > 0)
        {
            timerShowing -= Time.deltaTime;

            if(timerShowing <= 0) bossBarUI.DisableBossBar();
        }
    }

    public void HeadleTakeDamage(float damage)
    {
        if(!isShowing) bossBarUI.SetData(this, enemyHealth, enemy.enemy);
        timerShowing = showTime;
    }
}
