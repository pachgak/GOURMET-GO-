using System;
using UnityEngine;

// --- 1. คลาสแม่สุด ---
[System.Serializable]
public abstract class ItemModifier
{
    public abstract bool AffectCharacter(GameObject character);
}

// --- 2. ตัวอย่าง Modifier ทั่วไป ---
[System.Serializable]
public class HealthModifier : ItemModifier
{
    [Tooltip("ลากไฟล์ HealthSO ที่ต้องการมาใส่ตรงนี้")]
    public HealthSO healthData;

    public override bool AffectCharacter(GameObject character)
    {
        // เช็คก่อนว่าลากไฟล์มาใส่หรือยัง
        if (healthData == null) return false;

        if (character.TryGetComponent(out PlayerHealth health))
        {
            // ดึงค่า healAmount จากไฟล์ SO มาใช้
            health.addHp(healthData.healAmount);
            return true;
        }
        return false;
    }
}

[System.Serializable]
public class GetSkillModifier : ItemModifier
{
    public AttacksSkill playerAttackSkill;
    public int amount = 1;
    public override bool AffectCharacter(GameObject character)
    {
        if (character.TryGetComponent(out PlayerLoadoutSkill loadout))
        {
            loadout.loadoutData.AddItem(playerAttackSkill, amount);
            return true;
        }
        return false;
    }
}

// --- 3. พระเอกของเรา: ตัวส่งบัพ ---
[System.Serializable]
public class ApplyBuffItemModifier : ItemModifier
{
    [Tooltip("ลากไฟล์ BuffSO ที่ต้องการมาใส่ตรงนี้")]
    public BuffSO buffToApply;

    public override bool AffectCharacter(GameObject character)
    {
        if (buffToApply == null) return false;

        if (character.TryGetComponent(out PlayerBuffManager buffManager))
        {
            buffManager.AddBuff(buffToApply);
            return true;
        }
        return false;
    }
}