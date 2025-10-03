using UnityEngine;
using UnityEngine.UIElements;

public class HitEffect : MonoBehaviour
{
    public GameObject effecfHitPrefab;
    public GameObject damagePromptPrefab;
    public Vector3 offSet;
    public EnemyHealth enemyHealth;

    public void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        enemyHealth.OnTakeDamage += HandleInstantiateEffect;
    }
    private void OnDisable()
    {
        enemyHealth.OnTakeDamage -= HandleInstantiateEffect;
    }

    public void HandleInstantiateEffect(float damage)
    {
        Debug.Log("HitEffect");
       Vector3 effectPos = transform.localPosition + offSet;

        if (effecfHitPrefab != null)
        {
            //GameObject cloneEffecfHit = Instantiate(effecfHitPrefab, transform);
            GameObject cloneEffecfHit = ObjectPoolingManager.Instance.GetPoorObj(effecfHitPrefab);
            cloneEffecfHit.transform.parent = transform;
            cloneEffecfHit.transform.position = effectPos;

        }

        if (damagePromptPrefab != null)
        {
            //GameObject cloneDamagePrompt = Instantiate(damagePromptPrefab, transform);
            GameObject cloneDamagePrompt = ObjectPoolingManager.Instance.GetPoorObj(damagePromptPrefab);
            cloneDamagePrompt.transform.parent = transform;
            cloneDamagePrompt.transform.position = effectPos;
            if (cloneDamagePrompt.TryGetComponent(out DamagePrompt damagePrompt))
            {
                damagePrompt.SetDamageText(damage, offSet, transform);
            }
        }


    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
