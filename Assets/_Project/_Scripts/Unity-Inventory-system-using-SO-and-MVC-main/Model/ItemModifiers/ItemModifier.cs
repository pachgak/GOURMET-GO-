using System;
using UnityEngine;

// --- 1. คลาสแม่สุด ---
[System.Serializable]
public abstract class ItemModifier
{
    public abstract bool AffectCharacter(GameObject character);

    public virtual string GetDescription() => "";

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

    // *** เขียน Override อธิบายตัวเอง ***
    public override string GetDescription()
    {
        if (healthData == null) return "";
        // ใช้ Rich Text ให้ตัวเลขสีเขียวได้ด้วย!
        return $"- ฟื้นฟูพลังชีวิต <color=green>+{healthData.healAmount}</color> หน่วย";
    }
}

[System.Serializable]
public class GetSkillModifier : ItemModifier
{
    public PlayerSkillSO playerAttackSkill;
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

    // *** เขียน Override อธิบายตัวเอง ***
    public override string GetDescription()
    {
        if (playerAttackSkill == null) return "";
        return $"- ได้รับสกิล: <color=#00BFFF>{playerAttackSkill.skillName}</color> (x{amount})";
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

    // *** เขียน Override อธิบายตัวเอง ***
    // *** เขียน Override อธิบายตัวเอง ***
    public override string GetDescription()
    {
        if (buffToApply == null) return "";
        
        // 1. ข้อความหลัก (ชื่อบัพ และ เวลา)
        string baseDesc = $"- ได้รับบัพ: <color=orange>{buffToApply.buffName}</color> ({buffToApply.duration} วิ)";
        
        // 2. ดึงข้อความย่อยจาก BuffSO มาต่อท้าย
        string effectsDesc = buffToApply.GetEffectsDescription();
        
        return baseDesc + effectsDesc;
    }
}