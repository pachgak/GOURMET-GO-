using UnityEngine;


[System.Serializable]
public abstract class SpawnModifier
{
    // ฟังก์ชันนี้จะถูกเรียกหลังจาก Spawn เสร็จแล้ว
    // user = คนยิง, spawnedObject = ของที่เสกออกมา, speedMultiplier = ตัวคูณความเร็วจาก Combat
    public abstract void Apply(GameObject user, GameObject spawnedObject, float speedMultiplier);
}

[System.Serializable]
public class SpeedModifier : SpawnModifier
{
    public float baseSpeed = 10f;
    public bool applySpeedMultiplier = false; // อยากให้คูณความเร็วจาก Combat ไหม?

    public override void Apply(GameObject user, GameObject spawnedObject, float speedMultiplier)
    {
        if (spawnedObject.TryGetComponent(out ISpeed iSpeed))
        {
            float finalSpeed = baseSpeed;
            if (applySpeedMultiplier)
            {
                finalSpeed *= speedMultiplier;
            }
            iSpeed._speed = finalSpeed;
        }
    }
}


[System.Serializable]
public class LifeTimeModifier : SpawnModifier
{
    [Tooltip("เวลาที่จะอยู่บนฉากก่อนหายไป")]
    public float duration = 5f;
    [Tooltip("ถ้าความเร็วโจมตี (Speed Multiplier) มากขึ้น อยากให้หายไปไวขึ้นด้วยไหม?")]
    public bool applySpeedMultiplier = false; 

    public override void Apply(GameObject user, GameObject spawnedObject, float speedMultiplier)
    {
        if (spawnedObject.TryGetComponent(out ITimeDestroy timeDestroy))
        {
            float finalDuration = duration;
            if (applySpeedMultiplier && speedMultiplier > 0)
            {
                // ถ้าตีไวขึ้น เวลาที่อยู่บนพื้นก็จะลดลง (เหมือน DashAction)
                finalDuration /= speedMultiplier; 
            }

            timeDestroy._lifeTime = finalDuration;
            timeDestroy.StartLifeTime(); // สั่งให้นับเวลาใหม่ด้วยค่าล่าสุด
        }
    }
}

// ----------------------------------------------------
// 2. DelayTimeModifier: สำหรับตั้งเวลาชาร์จของ DelayedHitBox
// ----------------------------------------------------
[System.Serializable]
public class DelayTimeModifier : SpawnModifier
{
    [Tooltip("เวลาหน่วงก่อนที่ Hitbox ตัวจริงจะทำงาน")]
    public float delayTime = 1.0f;
    [Tooltip("ถ้าความเร็วโจมตี (Speed Multiplier) มากขึ้น อยากให้หน่วงเวลาน้อยลงไหม?")]
    public bool applySpeedMultiplier = true;

    public override void Apply(GameObject user, GameObject spawnedObject, float speedMultiplier)
    {
        if (spawnedObject.TryGetComponent(out DelayedHitBox delayedHitBox))
        {
            float finalDelay = delayTime;
            if (applySpeedMultiplier && speedMultiplier > 0)
            {
                // ถ้าบัฟตีไวขึ้น ท่าชาร์จก็ควรจะชาร์จเร็วขึ้น (ลด delay)
                finalDelay /= speedMultiplier;
            }

            delayedHitBox.SetDelayTime(finalDelay);
        }
    }
}