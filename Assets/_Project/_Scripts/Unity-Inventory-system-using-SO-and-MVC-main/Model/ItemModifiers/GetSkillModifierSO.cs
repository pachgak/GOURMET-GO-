using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GetSkillModifier", menuName = "Inventory/Modifier/GetSkill")]
public class GetSkillModifierSO : ItemModifierSO
{
    public PlayerSkillSO playerSkill;
    public AttacksSkill playerAttackSkill;
    public override bool AffectCharacter(GameObject character, float val)
    {
        PlayerLoadoutSkill playerLoadoutSkill = character.GetComponent<PlayerLoadoutSkill>();
        if (playerLoadoutSkill != null)
        {
            //playerLoadoutSkill.loadoutData.AddItem(playerAttackSkill, ((int)val));
            playerLoadoutSkill.loadoutData.AddItem(playerAttackSkill, 1);
            return true;
        }
        return false;

        //PlayerSkill playerSkillController = character.GetComponent<PlayerSkill>();
        //if (playerSkillController != null)
        //{
        //   bool addSkillComplet = playerSkillController.AddSkill(playerSkill, (int)val);
        //    return true;
        //}
        //return false;
    }
}
