using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackSkill;
using static AttacksSkill;
using static BearCombat;

//// Attribute นี้จะทำให้เราสร้าง ScriptableObject จากเมนู Assets ได้
[CreateAssetMenu(fileName = "New Enemys Skill", menuName = "Enemys/Enemys Skill")]
public class EnemySkillSO : ScriptableObject
{
    public string skillName;

    public AttacksSkill.SpawnSkillPrefabsType spawnSkillPrefabsType;
    public AttacksSkill.SkillSetp[] _skillSetp;

    public IEnumerator UseSkill(GameObject enemy, Transform target)
    {
        yield return Setplay(enemy, target);
    }

    private IEnumerator Setplay(GameObject enemy, Transform target)
    {
        Vector3 targetPosition = target.position;

        for (int i = 0; i < _skillSetp.Length; i++)
        {
            if (i == 0) yield return new WaitForSeconds(_skillSetp[i].playAtTime);
            else yield return new WaitForSeconds(_skillSetp[i].playAtTime - _skillSetp[i - 1].playAtTime);

            float dashSpeed = _skillSetp[i].dashSpeed;
            float dashTime = _skillSetp[i].dashTime;
            GameObject skillPrefabs = _skillSetp[i].skillPrefabs;
            bool haveDash = _skillSetp[i].haveDash;
            float damage = _skillSetp[i].damage;
            float knockbackForce = _skillSetp[i].knockbackForce;
            float speed = _skillSetp[i].speed;
            float skillFar = _skillSetp[i].skillFar;

            if (haveDash && enemy.TryGetComponent(out BaseEnemyMovement enemyMovement))
            {
                // คำนวณทิศทางการพุ่ง
                Vector3 directionDesh = (targetPosition - enemy.transform.position).normalized;
                Vector3 attackDirection = (targetPosition - enemy.transform.position).normalized;

                //
                enemyMovement.SkillDash(directionDesh, dashSpeed, dashTime);
            }
            else if (skillPrefabs != null) InstallAttackHit(skillPrefabs, enemy, targetPosition, (targetPosition - enemy.transform.position).normalized, skillFar, damage, knockbackForce, speed);
        }
        //EndSkilling();
    }

    private void InstallAttackHit(GameObject skillPrefabs, GameObject enemy, Vector3 mousePosition, Vector3 attackDirection, float skillFar, float damage, float knockbackForce, float speed)
    {
        GameObject attackInstance = null;
        Vector3 directionToMouse = (mousePosition - enemy.transform.position).normalized;
        Vector3 posInstance = Vector3.zero;
        Vector3 targetVecter = Vector3.zero;

        // สร้าง GameObject ของการโจมตี
        switch (spawnSkillPrefabsType)
        {
            case AttacksSkill.SpawnSkillPrefabsType.PlayerParent:
                //attackInstance = Instantiate(skillPrefabs, playerTransform);
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefabs, enemy.transform);
                //attackInstance.transform.parent = playerTransform;

                posInstance = enemy.transform.position + (attackDirection * skillFar);
                targetVecter = attackDirection;
                break;
            case AttacksSkill.SpawnSkillPrefabsType.PlayerWorld:
                //attackInstance = Instantiate(skillPrefabs, playerTransform.position, Quaternion.identity);
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefabs, enemy.transform.position);
                //attackInstance.transform.position = playerTransform.position;

                posInstance = enemy.transform.position + (attackDirection * skillFar);
                targetVecter = attackDirection;
                break;
            case AttacksSkill.SpawnSkillPrefabsType.MouseWorld:
                //attackInstance = Instantiate(skillPrefabs, mousePosition, Quaternion.identity);
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefabs , mousePosition);
                //attackInstance.transform.position = mousePosition;

                float skillFarTrue = (skillFar >= 0) ? Mathf.Clamp(Vector3.Distance(mousePosition, enemy.transform.position), 0, skillFar) : Vector3.Distance(mousePosition, enemy.transform.position);
                posInstance = enemy.transform.position + (directionToMouse * skillFarTrue);
                targetVecter = mousePosition - enemy.transform.position;
                break;
        }
        // set position
        attackInstance.transform.position = posInstance;

        // คำนวณการหมุน (Rotation)
        targetVecter.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(targetVecter);
        attackInstance.transform.rotation = targetRotation;

        if (attackInstance.TryGetComponent(out IHurtBox iHurtBox))
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