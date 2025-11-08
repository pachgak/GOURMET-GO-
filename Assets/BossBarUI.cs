using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossBarUI : MonoBehaviour
{
    public Slider hpBar;
    public TMP_Text hpCountText;
    public TMP_Text nameText;
    private EnemyHealth _healthTarget;
    private BossBarController _ConttrollerTarget;

    //public float timeBossBar;
    //private float _timer;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void HeadledTakeDamaget(float hpCurrent)
    {
        ShowHp(hpCurrent);
    }

    public void HeadleDie()
    {
        DisableBossBar();
    }
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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

    public void SetData(BossBarController bossBarController, EnemyHealth healthTarget,EnemySO enemy)
    {
        if (_healthTarget != null) ResingTarget();
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        _healthTarget = healthTarget;
        _ConttrollerTarget = bossBarController;

        _healthTarget.OnCurrentChang += HeadledTakeDamaget;
        _healthTarget.OnDie += HeadleDie;

        _ConttrollerTarget.isShowing = true;

        hpBar.maxValue = _healthTarget.maxHealth;
        nameText.text = enemy.name;



        ShowHp(_healthTarget.currentHealth);

        //SetShowBossbar(true);
    }

    //public void SetShowBossbar(bool anser)
    //{
    //    _image.enabled = anser;
    //    hpBar.gameObject.SetActive(anser);
    //    nameText.gameObject.SetActive(anser);

    //    if (!anser && _healthTarget != null)
    //    {
    //        ResingTarget();
    //    }
    //}

    private void ShowHp(float currentHp)
    {
        hpBar.value = currentHp;
        hpCountText.text = $"{currentHp} / {hpBar.maxValue}";
    }

    public void DisableBossBar()
    {
        ResingTarget();

        gameObject.SetActive(false);
    }

    private void ResingTarget()
    {
        _healthTarget.OnCurrentChang -= HeadledTakeDamaget;
        _healthTarget.OnDie -= HeadleDie;

        _ConttrollerTarget.isShowing = false;
        _healthTarget = null;
        _ConttrollerTarget = null;
    }
}
