using UnityEngine;

[System.Serializable]
public abstract class BuffEffect
{
    public abstract void ApplyEffect(GameObject target);
    public abstract void RemoveEffect(GameObject target);
    public virtual void TickEffect(GameObject target) { }

    // *** 1. แก้คลาสแม่ให้รับค่า BuffSO ***
    public virtual string GetDescription(BuffSO parentBuff) => "";
}

[System.Serializable]
public class MoveSpeedBuffEffect : BuffEffect
{
    public float amount = 0.2f; // 0.2 คือเพิ่ม 20%
    public override void ApplyEffect(GameObject target)
    {
        if (target.TryGetComponent(out PlayerStats stats)) stats.moveSpeed.AddModifier(amount);
    }
    public override void RemoveEffect(GameObject target)
    {
        if (target.TryGetComponent(out PlayerStats stats)) stats.moveSpeed.RemoveModifier(amount);
    }

    // *** Override อธิบายตัวเอง (แปลง amount 0.2 เป็น 20%) ***
    public override string GetDescription(BuffSO parentBuff)
    {
        return $"<color=green>+ ความเร็ว {amount * 100}%</color>";
    }
}

[System.Serializable]
public class AttackPowerBuffEffect : BuffEffect
{
    public float amount = 0.1f;
    public override void ApplyEffect(GameObject target)
    {
        if (target.TryGetComponent(out PlayerStats stats)) stats.attackPower.AddModifier(amount);
    }
    public override void RemoveEffect(GameObject target)
    {
        if (target.TryGetComponent(out PlayerStats stats)) stats.attackPower.RemoveModifier(amount);
    }

    public override string GetDescription(BuffSO parentBuff)
    {
        return $"<color=red>+ พลังโจมตี {amount * 100}%</color>";
    }
}

[System.Serializable]
public class DashBuffEffect : BuffEffect
{
    public float amount = 0.1f;
    public override void ApplyEffect(GameObject target)
    {
        if (target.TryGetComponent(out PlayerStats stats)) stats.dashRang.AddModifier(amount);
    }
    public override void RemoveEffect(GameObject target)
    {
        if (target.TryGetComponent(out PlayerStats stats)) stats.dashRang.RemoveModifier(amount);
    }

    public override string GetDescription(BuffSO parentBuff)
    {
        return $"<color=cyan>+ ระยะพุ่งตัว {amount * 100}%</color>";
    }
}

[System.Serializable]
public class MaxHealthBuffEffect : BuffEffect
{
    public float amount = 0.2f;
    public override void ApplyEffect(GameObject target)
    {
        if (target.TryGetComponent(out PlayerStats stats))
        {
            stats.maxHealth.AddModifier(amount);
            stats.UpdateMaxHealth();
        }
    }
    public override void RemoveEffect(GameObject target)
    {
        if (target.TryGetComponent(out PlayerStats stats))
        {
            stats.maxHealth.RemoveModifier(amount);
            stats.UpdateMaxHealth();
        }
    }

    public override string GetDescription(BuffSO parentBuff)
    {
        return $"<color=green>+ พลังชีวิตสูงสุด {amount * 100}%</color>";
    }
}

[System.Serializable]
public class RegenHPBuffEffect : BuffEffect
{
    public float healAmount = 20f;
    public override void ApplyEffect(GameObject target) { }
    public override void RemoveEffect(GameObject target) { }
    public override void TickEffect(GameObject target)
    {
        if (target.TryGetComponent(out PlayerHealth health)) health.addHp(healAmount);
    }
    public override string GetDescription(BuffSO parentBuff)
    {
        // ถ้าบัพนี้มีการติ๊กเปิด Tick Effect ไว้ ให้เอาเวลามาโชว์
        if (parentBuff != null && parentBuff.hasTickEffect)
        {
            return $"<color=green>+ ฟื้นฟู {healAmount} HP ทุกๆ {parentBuff.tickInterval} วินาที</color>";
        }

        // กันเหนียว เผื่อลืมติ๊กเปิด Tick Effect
        return $"<color=green>+ ฟื้นฟู {healAmount} HP</color>";
    }
}