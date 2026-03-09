using UnityEngine;

public class DestoryPersonHitbox : MonoBehaviour
{
    private PersonHitbox personHitbox;

    private void Awake()
    {
        personHitbox = GetComponent<PersonHitbox>();
    }

    private void OnEnable()
    {
        if(personHitbox != null) personHitbox.OnAttackHit += ReturnObjectToPool;
    }

    private void OnDisable()
    {
        if (personHitbox != null) personHitbox.OnAttackHit -= ReturnObjectToPool;
    }

    private void ReturnObjectToPool(Collider[] hits)
    {
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }
}
