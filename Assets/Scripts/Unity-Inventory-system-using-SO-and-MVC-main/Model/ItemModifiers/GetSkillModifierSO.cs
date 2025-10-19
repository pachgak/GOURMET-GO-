using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GetSkillModifier", menuName = "Inventory/Modifier/GetSkill")]
public class GetSkillModifierSO : ItemModifierSO
{
    public AttacksSkill skill;
    public override bool AffectCharacter(GameObject character, float val)
    {
        PlayerSkillController playerSkillController = character.GetComponent<PlayerSkillController>();
        if (playerSkillController != null)
        {
           bool addSkillComplet = playerSkillController.AddSkill(skill, (int)val);
            return true;
        }
        return false;
    }
}
