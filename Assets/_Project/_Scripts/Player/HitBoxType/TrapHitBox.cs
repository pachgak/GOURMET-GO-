using UnityEngine;
using UnityEngine.Timeline;
using static IHitBox;

public class TrapHitBox : BaseHitBox
{
    public float attackRadius = 1.5f;
    // กำหนดขนาดและค่า Offset ของ Hitbox ได้ใน Inspector
    //public Vector3 attackOffset = new Vector3(0, 0, 1f);
    //public Vector3 attackSize = new Vector3(1.5f, 1.5f, 1.5f);

    Collider _colider;
    public bool isTriger;

    protected virtual void Awake()
    {
        _colider = GetComponent<Collider>();
    }

    private void Start()
    {
         
    }

    public override Collider[] GetCollidersInArea(LayerMask mask)
    {
        return Physics.OverlapSphere(transform.position, attackRadius, mask);
    }

    private void Update()
    {
        if (!isTriger) return;

        // ใช้ Physics.OverlapBox เพื่อหา Collider ทั้งหมดที่อยู่ใน Hitbox
        Collider[] hitColliders = GetCollidersInArea(targetLayer);

        if (hitColliders.Length > 0)
        {
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent(out ITakeDamage canTakeDamage))
                {
                    canTakeDamage.TakeDamage(damage);
                }

                if (hitCollider.TryGetComponent(out IKnockbackable knockbackable))
                {
                    knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                    knockbackable.GetKnockedBack(knockbackDirection, knockbackForce, knockbackTime);
                }
            }

            //playerSound and CameraShack
            if (hitColliders.Length > 0 && ownerHit == CameraShakeManager.instance.playerGameObject)
            {
                CameraShakeManager.instance.ShakePlayerAttack();
            }

            ReturnObjectToPool();
        }
    }

    //protected virtual void OnTriggerEnter(Collider other)
    //{
    //    return;

    //    Debug.Log($"Hit PersonHitbox by : {gameObject.name}");
    //    Debug.Log($"other layer :{other.gameObject.name} / {other.gameObject.layer}");

    //    if (other.gameObject.layer == Mathf.Log(targetLayer.value, 2))
    //    {
    //        Collider hitCollider = other;

    //        if (hitCollider.TryGetComponent(out ITakeDamage canTakeDamage))
    //        {
    //            /*switch (damageType)
    //            {
    //                case DamageType.NoneOwner:
    //                    if (hitCollider.gameObject != ownerHit) canTakeDamage.TakeDamage(damage);
    //                    break;

    //                case DamageType.NoneTeam:
    //                    TeamBaner hitTeamBaner = hitCollider.GetComponent<TeamBaner>();
    //                    TeamBaner ownerTeamBaner = ownerHit.GetComponent<TeamBaner>();
    //                    if (ownerTeamBaner == null) return;
    //                    if (hitTeamBaner == null)
    //                    {
    //                        canTakeDamage.TakeDamage(damage);
    //                    }
    //                    else if (hitTeamBaner.banner != ownerTeamBaner)
    //                    {
    //                        canTakeDamage.TakeDamage(damage);
    //                    }
    //                    break;

    //                case DamageType.AllEntity:
    //                    canTakeDamage.TakeDamage(damage);
    //                    break;
    //            }*/
    //            canTakeDamage.TakeDamage(damage);
    //        }

    //        if (hitCollider.TryGetComponent(out IKnockbackable knockbackable))
    //        {
    //            knockbackable.GetKnockedBack(transform.position - other.transform.position, knockbackForce,knockbackTime);
    //        }

    //        //playerSound and CameraShack
    //        if (ownerHit == CameraShakeManager.instance.playerGameObject)
    //        {
    //            CameraShakeManager.instance.ShakePlayerAttack();
    //        }

    //        ReturnObjectToPool();
    //    }
    //    //if (other.gameObject.layer == Mathf.Log(wallLayer.value, 2))
    //    //{
    //    //    ReturnObjectToPool();
    //    //}
    //}

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
