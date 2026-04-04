using UnityEngine;

public class AoeHitBox : PersonHitbox
{
    protected override void Awake() 
    {
        base.Awake();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Mathf.Log(targetLayer.value, 2))
        {
            Collider hitCollider = other;

            if (hitCollider.TryGetComponent(out ITakeDamage canTakeDamage))
            {
                canTakeDamage.TakeDamage(damage);
            }

            if (hitCollider.TryGetComponent(out IKnockbackable knockbackable))
            {
                knockbackable.GetKnockedBack((hitCollider.transform.position - ownerHit.transform.position).normalized, knockbackForce,knockbackTime);
            }

            //playerSound and CameraShack
            if (ownerHit == CameraShakeManager.instance.playerGameObject)
            {
                CameraShakeManager.instance.ShakePlayerAttack();
            }
        }
    }
}
