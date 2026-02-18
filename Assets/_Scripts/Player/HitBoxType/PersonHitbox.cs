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

    public event Action OnHit;

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
        // พยายามดึง BoxCollider มาใช้งาน
        BoxCollider boxCol = GetComponent<BoxCollider>();

        if (boxCol != null)
        {
            // ตั้งค่าสี (แดงโปร่งแสง)
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

            // คำนวณ Matrix เพื่อให้ Gizmo หมุนและขยับตาม Object
            // boxCol.center คือตำแหน่ง Offset ภายใน Collider
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                transform.TransformPoint(boxCol.center),
                transform.rotation,
                transform.lossyScale // ใช้ lossyScale เพื่อให้ขนาด Gizmo ตรงกับ Scale ของ Object จริงๆ
            );

            Gizmos.matrix = rotationMatrix;

            // วาดกล่องตามขนาดของ BoxCollider
            // วาดที่ Vector3.zero เพราะเราเซตตำแหน่งไว้ที่ Matrix แล้ว
            Gizmos.DrawCube(Vector3.zero, boxCol.size);

            // วาดเส้นขอบให้ดูคมชัดขึ้น
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, boxCol.size);

            // คืนค่า Matrix กลับเป็นค่าเริ่มต้น
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
