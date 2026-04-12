using UnityEngine;

[System.Serializable]
public abstract class BuffEffect
{
    public abstract void ApplyEffect(GameObject target);
    public abstract void RemoveEffect(GameObject target);
    public virtual void TickEffect(GameObject target) { }
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
}