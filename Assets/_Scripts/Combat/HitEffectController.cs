using UnityEngine;
using UnityEngine.UIElements;

public class HitEffectController : MonoBehaviour
{
    public GameObject effecfHitPrefab;
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
       Vector3 effectPos = transform.localPosition + offSet;

        if (effecfHitPrefab != null)
        {
            //GameObject cloneEffecfHit = Instantiate(effecfHitPrefab, transform);
            GameObject cloneEffecfHit = ObjectPoolingManager.Instance.Spawn(effecfHitPrefab);
            cloneEffecfHit.transform.parent = transform;
            cloneEffecfHit.transform.position = effectPos;

        }
    }
}
