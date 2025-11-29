using UnityEngine;

public class BearParticleController : MonoBehaviour
{
    private EnemyHealth _enemyHealth;
    [SerializeField] private ParticleSystem auraParticle; // ลาก Particle System มาใส่
    [SerializeField] private SpriteRenderer iconAngry; // ลาก Particle System มาใส่
    [SerializeField] private Vector3 auraOffSet;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        ToggleAngryVfx(false);

        _enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        _enemyHealth.OnCurrentChang += HandleCurrentChang;
    }

    private void OnDisable()
    {
        _enemyHealth.OnCurrentChang -= HandleCurrentChang;
    }

    private void HandleCurrentChang(float hp)
    {
        if (auraParticle == null) return;

        if (hp <= _enemyHealth.maxHealth / 2 && !auraParticle.isPlaying)
        {
            ToggleAngryVfx(true);
        }
        else if(hp >= _enemyHealth.maxHealth && auraParticle.isPlaying)
        {
            ToggleAngryVfx(false);
        }
    }

    public void ToggleAngryVfx(bool awn)
    {
        if(awn) auraParticle.Play();
        else auraParticle.Stop();

        iconAngry.enabled = awn;
    }



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
