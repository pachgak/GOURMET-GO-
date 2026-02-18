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


//[System.Serializable]
//public class LifeTimeModifier : SpawnModifier
//{
//    public float duration = 5f;

//    public override void Apply(GameObject user, GameObject spawnedObject, float speedMultiplier)
//    {
//        // สมมติว่าคุณมี Interface หรือ Component ที่จัดการเรื่องเวลาตาย
//        // เช่น ITimeDestroy หรือ AutoDestroy
//        if (spawnedObject.TryGetComponent(out ITimeDestroy timeDestroy))
//        {
//            timeDestroy._lifeTime = duration;
//            timeDestroy.StartLifeTime();
//        }
//        // หรือถ้าไม่มี Interface ก็สั่ง Coroutine ตรงนี้ได้ (แต่วิธี Interface ดีกว่า)
//    }
//}