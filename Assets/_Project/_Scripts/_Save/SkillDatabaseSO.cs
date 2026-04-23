using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkillDatabase", menuName = "Skills/Skill Database")]
public class SkillDatabaseSO : ScriptableObject
{
    public List<PlayerSkillSO> allSkills = new List<PlayerSkillSO>();

    public PlayerSkillSO GetSkillByID(string id)
    {
        return allSkills.Find(skill => skill != null && skill.ID == id);
    }
}