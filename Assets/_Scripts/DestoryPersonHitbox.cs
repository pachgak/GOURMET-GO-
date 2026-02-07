using UnityEngine;

public class DestoryPersonHitbox : MonoBehaviour
{
    private PersonHitbox personHitbox;

    private void Awake()
    {
        personHitbox = GetComponent<PersonHitbox>();

        personHitbox.OnAttackHit += ReturnObjectToPool;
    }

    private void ReturnObjectToPool(Collider[] hits)
    {
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }
}
