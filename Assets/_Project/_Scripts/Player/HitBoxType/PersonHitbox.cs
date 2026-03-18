using System;
using UnityEngine;
using static IHitBox;

public class PersonHitbox : BaseHitBox
{
    // กำหนดขนาดและค่า Offset ของ Hitbox ได้ใน Inspector
    //public Vector3 attackOffset = new Vector3(0, 0, 1f);
    //public Vector3 attackSize = new Vector3(1.5f, 1.5f, 1.5f);

    public LayerMask wallLayer; // ตั้งค่า Layer ของศัตรูใน Inspector

    Collider _colider;

    //public event Action OnHit;

    protected virtual void Awake()
    {
        _colider = GetComponent<Collider>();
    }

    private void Start()
    {

    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Hit PersonHitbox by : {gameObject.name}");
        //Debug.Log($"other layer :{other.gameObject.name} / {other.gameObject.layer}");

        if (((1 << other.gameObject.layer) & targetLayer.value) != 0)
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
                knockbackable.GetKnockedBack(knockbackDirection, knockbackForce, knockbackTime);
            }

            //playerSound and CameraShack
            if (ownerHit == CameraShakeManager.instance.playerGameObject)
            {
                CameraShakeManager.instance.ShakePlayerAttack();
            }

            //OnHit Active
            Collider[] colliders = new Collider[1] { hitCollider };
            OnAttackHit?.Invoke(colliders);

            DisableAttack();
            //ReturnObjectToPool();
        }
        if (other.gameObject.layer == Mathf.Log(wallLayer.value, 2))
        {
            //OnHit Active
            Collider[] colliders = new Collider[1];
            colliders[0] = other;
            OnAttackHit?.Invoke(colliders);

            DisableAttack();
            //ReturnObjectToPool();
        }
    }

    public override void PerformAttack()
    {
        _colider.enabled = true;
    }

    public void DisableAttack()
    {
        _colider.enabled = false;
    }



    private void ReturnObjectToPool()
    {
        _colider.enabled = false;
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }

    private void OnDrawGizmos()
    {
        // 1. เช็คและดึง BoxCollider มาวาด
        if (TryGetComponent(out BoxCollider boxCol))
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                transform.TransformPoint(boxCol.center),
                transform.rotation,
                transform.lossyScale
            );

            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawCube(Vector3.zero, boxCol.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, boxCol.size);

            Gizmos.matrix = Matrix4x4.identity;
        }

        // 2. เช็คและดึง SphereCollider มาวาด
        if (TryGetComponent(out SphereCollider sphereCol))
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

            // หาจุดศูนย์กลางของ Sphere ในพิกัดโลก (World Space)
            Vector3 worldCenter = transform.TransformPoint(sphereCol.center);

            // คำนวณรัศมีที่แท้จริง
            float maxScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z)));
            float actualRadius = sphereCol.radius * maxScale;

            // วาดทรงกลม
            Gizmos.DrawSphere(worldCenter, actualRadius);

            // วาดเส้นขอบ
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(worldCenter, actualRadius);
        }
    }
}
