using UnityEngine;

[System.Serializable]
public abstract class SpawnModifier
{
    // *** แก้ตรงนี้: เพิ่ม GameObject target เข้ามาใน Parameter ด้วย ***
    public abstract void Apply(GameObject user, GameObject target, GameObject spawnedObject, float speedMultiplier);
}

[System.Serializable]
public class SpeedModifier : SpawnModifier
{
    public float baseSpeed = 10f;
    public bool applySpeedMultiplier = false;

    // *** อัปเดต Parameter ***
    public override void Apply(GameObject user, GameObject target, GameObject spawnedObject, float speedMultiplier)
    {
        if (spawnedObject.TryGetComponent(out ISpeed iSpeed))
        {
            float finalSpeed = baseSpeed;
            if (applySpeedMultiplier) finalSpeed *= speedMultiplier;
            iSpeed._speed = finalSpeed;
        }
    }
}

[System.Serializable]
public class LifeTimeModifier : SpawnModifier
{
    public float duration = 5f;
    public bool applySpeedMultiplier = false;

    // *** อัปเดต Parameter ***
    public override void Apply(GameObject user, GameObject target, GameObject spawnedObject, float speedMultiplier)
    {
        if (spawnedObject.TryGetComponent(out ITimeDestroy timeDestroy))
        {
            float finalDuration = duration;
            if (applySpeedMultiplier && speedMultiplier > 0) finalDuration /= speedMultiplier;

            timeDestroy._lifeTime = finalDuration;
            timeDestroy.StartLifeTime();
        }
    }
}

[System.Serializable]
public class DelayTimeModifier : SpawnModifier
{
    public float delayTime = 1.0f;
    public bool applySpeedMultiplier = true;

    // *** อัปเดต Parameter ***
    public override void Apply(GameObject user, GameObject target, GameObject spawnedObject, float speedMultiplier)
    {
        // *** แก้ตรงนี้: เปลี่ยนจากการหา DelayedHitBox เป็นหา IDelayable แทน ***
        if (spawnedObject.TryGetComponent(out IDelayable delayable))
        {
            float finalDelay = delayTime;
            if (applySpeedMultiplier && speedMultiplier > 0) finalDelay /= speedMultiplier;

            // ส่งค่าไปให้ Interface ทำงาน
            delayable.SetDelayTime(finalDelay);
        }
    }
}

[System.Serializable]
public class DurationModifier : SpawnModifier
{
    public float delayTime = 1.0f;
    public bool applySpeedMultiplier = true;

    // *** อัปเดต Parameter ***
    public override void Apply(GameObject user, GameObject target, GameObject spawnedObject, float speedMultiplier)
    {
        // *** แก้ตรงนี้: เปลี่ยนจากการหา DelayedHitBox เป็นหา IDelayable แทน ***
        if (spawnedObject.TryGetComponent(out IDurationable delayable))
        {
            float finalDelay = delayTime;
            if (applySpeedMultiplier && speedMultiplier > 0) finalDelay /= speedMultiplier;

            // ส่งค่าไปให้ Interface ทำงาน
            delayable.SetDurationTime(finalDelay);
        }
    }
}

// ----------------------------------------------------
// *** สร้างใหม่: HomingTargetModifier ***
// ----------------------------------------------------
[System.Serializable]
public class TargetHomingModifier : SpawnModifier
{
    [Tooltip("ถ้าตอนเสกออกมา ศัตรูมองไม่เห็น Target (ค่าเป็น null) ให้บังคับหา Player อัตโนมัติเลยไหม?")]
    public bool fallbackToPlayer = true;

    public override void Apply(GameObject user, GameObject target, GameObject spawnedObject, float speedMultiplier)
    {
        // เช็คว่าของที่เสกออกมา รองรับการล็อกเป้าหมายไหม
        if (spawnedObject.TryGetComponent(out ITargetable targetable))
        {
            Transform finalTarget = null;

            if (target != null)
            {
                finalTarget = target.transform;
            }
            else if (fallbackToPlayer)
            {
                // ถ้าสกิลถูกยิงออกไปตอนที่ AI หลุด Aggro ให้พยายามดึงตัว Player มาล็อกเป้าแทน
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    finalTarget = playerObj.transform;
                }
            }

            // ส่งเป้าหมายไปให้กระสุน
            targetable.SetTarget(finalTarget);
        }
    }
}