using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttacksSkill;
using static BearCombat;

[CreateAssetMenu(fileName = "New Enemys Skill", menuName = "Enemys/Enemys Skill")]
public class EnemySkillSO : ScriptableObject
{
    public string skillName;

    public AttacksSkill.SpawnSkillPrefabsType spawnSkillPrefabsType;
    public AttacksSkill.SkillSetp[] _skillSetp;

    // รับ parameter speedMultiplier (default = 1.0f คือความเร็วปกติ)
    public IEnumerator UseSkill(GameObject enemy, Transform target, float speedMultiplier = 1.0f)
    {
        yield return Setplay(enemy, target, speedMultiplier);
    }

    private IEnumerator Setplay(GameObject enemy, Transform target, float speedMultiplier)
    {
        // วนลูปตาม Step ของสกิล
        for (int i = 0; i < _skillSetp.Length; i++)
        {
            // 1. คำนวณเวลาที่ต้องรอ (Wait Time)
            float originalWaitTime = 0f;
            if (i == 0)
            {
                originalWaitTime = _skillSetp[i].playAtTime;
            }
            else
            {
                originalWaitTime = _skillSetp[i].playAtTime - _skillSetp[i - 1].playAtTime;
            }

            // นำเวลาเดิมมา "หาร" ด้วย speedMultiplier
            // ตัวอย่าง: ถ้ารอ 1 วินาที และ speedMultiplier = 2 (เร็ว 2 เท่า) -> จะเหลือรอแค่ 0.5 วินาที
            yield return new WaitForSeconds(originalWaitTime / speedMultiplier);

            // 2. เตรียมตัวแปรต่างๆ
            GameObject skillPrefabs = _skillSetp[i].skillPrefabs;
            bool haveDash = _skillSetp[i].haveDash;
            float damage = _skillSetp[i].damage;
            float knockbackForce = _skillSetp[i].knockbackForce;

            // Speed ของ Projectile อาจจะให้เท่าเดิมหรือคูณด้วยก็ได้ ขึ้นอยู่กับ Design
            // ในที่นี้ผมปล่อยเท่าเดิมตามค่าที่ตั้งไว้ (หรือจะคูณ speedMultiplier ก็ได้ถ้าต้องการกระสุนไวด้วย)
            float speed = _skillSetp[i].speed;

            float skillFar = _skillSetp[i].skillFar;
            float skillHight = _skillSetp[i].skillHight;
            Vector2 skillOffset = new Vector2(_skillSetp[i].skillFar, _skillSetp[i].skillHight);

            // 3. Logic การทำงาน (Dash หรือ Spawn Attack)
            if (haveDash && enemy.TryGetComponent(out BaseEnemyMovement enemyMovement))
            {
                // คำนวณทิศทางการพุ่ง
                Vector3 directionDash = (target.position - enemy.transform.position).normalized;

                // *** สูตรคำนวณ Dash เมื่อความเร็วเปลี่ยน ***
                // ระยะทาง = ความเร็ว x เวลา
                // เพื่อให้ระยะพุ่งเท่าเดิม แต่ไวขึ้น:
                // - ความเร็วต้อง "คูณ" speedMultiplier
                // - เวลาต้อง "หาร" speedMultiplier
                float adjustedDashSpeed = _skillSetp[i].dashSpeed * speedMultiplier;
                float adjustedDashTime = _skillSetp[i].dashTime / speedMultiplier;

                enemyMovement.SkillDash(directionDash, adjustedDashSpeed, adjustedDashTime);
            }
            else if (skillPrefabs != null)
            {
                InstallAttackHit(skillPrefabs, enemy, target.position, (target.position - enemy.transform.position).normalized, skillOffset, damage, knockbackForce, speed);
            }
        }
    }

    private void InstallAttackHit(GameObject skillPrefabs, GameObject enemy, Vector3 mousePosition, Vector3 attackDirection, Vector2 offSet, float damage, float knockbackForce, float speed)
    {
        GameObject attackInstance = null;
        Vector3 directionToMouse = (mousePosition - enemy.transform.position).normalized;
        Vector3 posInstance = Vector3.zero;
        Vector3 targetVecter = Vector3.zero;
        Vector3 hightPos = new Vector3(0, offSet.y, 0);

        // สร้าง GameObject ของการโจมตี
        switch (spawnSkillPrefabsType)
        {
            case AttacksSkill.SpawnSkillPrefabsType.PlayerParent:
                //attackInstance = Instantiate(skillPrefabs, playerTransform);
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefabs, enemy.transform);
                //attackInstance.transform.parent = playerTransform;

                posInstance = enemy.transform.position + (attackDirection * offSet.x);
                targetVecter = attackDirection;
                break;
            case AttacksSkill.SpawnSkillPrefabsType.PlayerWorld:
                //attackInstance = Instantiate(skillPrefabs, playerTransform.position, Quaternion.identity);
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefabs, enemy.transform.position);
                //attackInstance.transform.position = playerTransform.position;

                posInstance = enemy.transform.position + (attackDirection * offSet.x);
                targetVecter = attackDirection;
                break;
            case AttacksSkill.SpawnSkillPrefabsType.MouseWorld:
                //attackInstance = Instantiate(skillPrefabs, mousePosition, Quaternion.identity);
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefabs, mousePosition);
                //attackInstance.transform.position = mousePosition;

                float skillFarTrue = (offSet.x >= 0) ? Mathf.Clamp(Vector3.Distance(mousePosition, enemy.transform.position), 0, offSet.x) : Vector3.Distance(mousePosition, enemy.transform.position);
                posInstance = enemy.transform.position + (directionToMouse * skillFarTrue);
                targetVecter = mousePosition - enemy.transform.position;
                break;
        }

        // set position
        attackInstance.transform.position = posInstance + hightPos;

        // คำนวณการหมุน (Rotation)
        targetVecter.y = 0f;
        if (targetVecter != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetVecter);
            attackInstance.transform.rotation = targetRotation;
        }

        if (attackInstance.TryGetComponent(out IHitBox iHurtBox))
        {
            iHurtBox._targetLayer = LayerMask.GetMask("Player");
            iHurtBox._ownerHit = enemy;
            iHurtBox._damage = damage;
            iHurtBox._knockbackDirection = directionToMouse;
            iHurtBox._knockbackForce = knockbackForce;
            iHurtBox.PerformAttack();
        }
        if (attackInstance.TryGetComponent(out ISpeed iSpeed))
        {
            iSpeed._speed = speed;
        }
    }
}