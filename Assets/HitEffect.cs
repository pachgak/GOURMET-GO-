using UnityEngine;
using UnityEngine.UIElements;

public class HitEffect : MonoBehaviour
{
    public GameObject effecfHitPrefab;
    public GameObject damagePromptPrefab;
    public Vector3 offSet;

    public void InstantiateEffect(float damage)
    {
        Debug.Log("HitEffect");
       Vector3 effectPos = transform.localPosition + offSet;

        if (effecfHitPrefab != null)
        {
            GameObject cloneEffecfHit = Instantiate(effecfHitPrefab, transform);
            cloneEffecfHit.transform.position = effectPos;

        }

        if (damagePromptPrefab != null)
        {
            GameObject cloneDamagePrompt = Instantiate(damagePromptPrefab, transform);
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
