using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GetSkillModifier", menuName = "Inventory/Modifier/GetSkill")]
public class GetSkillModifierSO : ItemModifierSO
{
    public PlayerSkillSO playerSkill;
    public override bool AffectCharacter(GameObject character, float val)
    {
        PlayerSkill playerSkillController = character.GetComponent<PlayerSkill>();
        if (playerSkillController != null)
        {
           bool addSkillComplet = playerSkillController.AddSkill(playerSkill, (int)val);
            return true;
        }
        return false;
    }
}
