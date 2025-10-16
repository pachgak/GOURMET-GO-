using UnityEngine;

public class DamagePromptController : MonoBehaviour
{
    public GameObject damagePromptPrefab;
    public Vector3 offSet;
    public ITakeDamage takeDamage;

    public void Awake()
    {
        takeDamage = GetComponent<ITakeDamage>();
    }

    private void OnEnable()
    {
        takeDamage.OnTakeDamage += HandleInstantiateEffect;
    }
    private void OnDisable()
    {
        takeDamage.OnTakeDamage -= HandleInstantiateEffect;
    }

    public void HandleInstantiateEffect(float damage)
    {
        Vector3 effectPos = transform.position + offSet;

        if (damagePromptPrefab != null)
        {
            //GameObject cloneDamagePrompt = Instantiate(damagePromptPrefab, transform);
            GameObject cloneDamagePrompt = ObjectPoolingManager.Instance.Spawn(damagePromptPrefab);
            cloneDamagePrompt.transform.position = effectPos;
            if (cloneDamagePrompt.TryGetComponent(out DamagePromptText damagePrompt))
            {
                damagePrompt.SetDamageText(damage, offSet, transform);
            }
        }


    }
}
