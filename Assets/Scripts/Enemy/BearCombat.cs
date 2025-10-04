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

    protected override IEnumerator AttackLogic(bool forceSkill3Sequence)
    {
        
        Debug.Log("AttackLogic Bear");

        // 1. ตรวจสอบเงื่อนไขการใช้สกิล 3 ก่อน
        if (forceSkill3Sequence)
        {
            Debug.Log("AI สั่ง: บังคับใช้ Skill 3 ทันที!");
            TriggerSkillUesd(2);
            yield return enemySkills[2].UseSkill(this.gameObject, _aiController.playerTarget);
        }
        else
        {
            // 2. ถ้า AI ไม่ได้สั่งบังคับ ให้สุ่ม 1 ใน 3
            //SkillType firstSkill = (SkillType)UnityEngine.Random.Range(0, 3);
            int firstSkillIndex = UnityEngine.Random.Range(0, 3);
            Debug.Log($"firstSkill UseSkill No.{firstSkillIndex+1}");
            TriggerSkillUesd(firstSkillIndex);
            yield return enemySkills[firstSkillIndex].UseSkill(this.gameObject, _aiController.playerTarget);

            if (firstSkillIndex == 2)
            {
                Debug.Log($"Skill3 UseSkill");
                forceSkill3Sequence = true;
            }
        }

        Debug.Log($"End firstSkill");

        // 3. ใช้สกิลที่สุ่มได้คือ Skill 3 ให้ทำต่อ
        if (forceSkill3Sequence)
        {
            int randomSkillIndex = UnityEngine.Random.Range(0, 2);
            Debug.Log($"Next UseSkill No.{randomSkillIndex + 1}");
            TriggerSkillUesd(randomSkillIndex);
            yield return enemySkills[randomSkillIndex].UseSkill(this.gameObject, _aiController.playerTarget);
        }
        Debug.Log($"=========== Skill Loop ==============");
    }
}
