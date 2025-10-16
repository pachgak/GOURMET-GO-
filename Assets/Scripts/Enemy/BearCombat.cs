using System.Collections;
using UnityEngine;

public class BearCombat : BaseEnemyCombat
{
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Update()
    {
        if (attackTimer > 0 && _attackSequenceCoroutine == null)
        {
            attackTimer -= Time.deltaTime;
            
            if (attackTimer <= 0)
            {
                _agent.isStopped = false;
            }
            else
            {
                _agent.isStopped = true;
            }
        }

        if (_aiController.currentState == BaseEnemyAI.EnemyState.Attack && attackTimer <= 0 && _attackSequenceCoroutine == null)
        {
            HandleStartAttackSequence(false);
            attackTimer = attackCooldown;
        }

        if (_aiController.currentState == BaseEnemyAI.EnemyState.Attack && _attackSequenceCoroutine == null) _aiController.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);
    }

    protected override IEnumerator AttackLogic(bool forceSkill3Sequence)
    {
       

        // 1. ตรวจสอบเงื่อนไขการใช้สกิล 3 ก่อน
        if (forceSkill3Sequence)
        {
            TriggerSkillUesd(2);
            yield return enemySkills[2].UseSkill(this.gameObject, _aiController.playerTarget);
        }
        else
        {
            // 2. ถ้า AI ไม่ได้สั่งบังคับ ให้สุ่ม 1 ใน 3
            //SkillType firstSkill = (SkillType)UnityEngine.Random.Range(0, 3);
            int firstSkillIndex = UnityEngine.Random.Range(0, 3);
            TriggerSkillUesd(firstSkillIndex);
            yield return enemySkills[firstSkillIndex].UseSkill(this.gameObject, _aiController.playerTarget);

            if (firstSkillIndex == 2)
            {
                forceSkill3Sequence = true;
            }
        }

        // 3. ใช้สกิลที่สุ่มได้คือ Skill 3 ให้ทำต่อ
        if (forceSkill3Sequence)
        {
            int randomSkillIndex = UnityEngine.Random.Range(0, 2);
            TriggerSkillUesd(randomSkillIndex);
            yield return enemySkills[randomSkillIndex].UseSkill(this.gameObject, _aiController.playerTarget);
        }
    }
}
