using UnityEngine;
using static IHurtBox;

public class PersonHitbox : BaseHitBox
{
    // กำหนดขนาดและค่า Offset ของ Hitbox ได้ใน Inspector
    public Vector3 attackOffset = new Vector3(0, 0, 1f);
    public Vector3 attackSize = new Vector3(1.5f, 1.5f, 1.5f);

    public LayerMask wallLayer; // ตั้งค่า Layer ของศัตรูใน Inspector

    Collider _colider;

    private void Awake()
    {
        _colider = GetComponent<Collider>();
    }

    private void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Mathf.Log(targetLayer.value, 2))
        {
            Collider hitCollider = other;

            if (hitCollider.TryGetComponent(out ITakeDamage canTakeDamage))
            {
                /*switch (damageType)
                {
                    case DamageType.NoneOwner:
                        if (hitCollider.gameObject != ownerHit) canTakeDamage.TakeDamage(damage);
                        break;

                    case DamageType.NoneTeam:
                        TeamBaner hitTeamBaner = hitCollider.GetComponent<TeamBaner>();
                        TeamBaner ownerTeamBaner = ownerHit.GetComponent<TeamBaner>();
                        if (ownerTeamBaner == null) return;
                        if (hitTeamBaner == null)
                        {
                            canTakeDamage.TakeDamage(damage);
                        }
                        else if (hitTeamBaner.banner != ownerTeamBaner)
                        {
                            canTakeDamage.TakeDamage(damage);
                        }
                        break;

                    case DamageType.AllEntity:
                        canTakeDamage.TakeDamage(damage);
                        break;
                }*/
                canTakeDamage.TakeDamage(damage);
            }

            if (hitCollider.TryGetComponent(out IKnockbackable knockbackable))
            {
                knockbackable.GetKnockedBack(knockbackDirection, knockbackForce);
            }

            //playerSound and CameraShack
            if (ownerHit == CameraShakeManager.instance.playerGameObject)
            {
                CameraShakeManager.instance.ShakePlayerAttack();
            }

            ReturnObjectToPool();
        }
        if (other.gameObject.layer == Mathf.Log(wallLayer.value, 2))
        {
            ReturnObjectToPool();
        }
    }

    public override void PerformAttack()
    {
        _colider.enabled = true;
    }

    private void ReturnObjectToPool()
    {
        _colider.enabled = false;
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }
}
