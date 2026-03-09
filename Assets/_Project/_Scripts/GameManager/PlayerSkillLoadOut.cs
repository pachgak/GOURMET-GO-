using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillLoadOut : MonoBehaviour
{
    public PlayerSkill playerSkill;

    public AttacksSkill[] skillListLoadout;
    public List<SkillDataList> data;

    [System.Serializable]
    public class SkillDataList
    {
        public AttacksSkill skill;
        public int exp;
    }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
