using System.Collections;
using UnityEngine;

public class AutoInteractTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ลาก InteractableBase ที่ต้องการให้ทำงานอัตโนมัติมาใส่\n(ถ้าปล่อยว่างไว้ ระบบจะดึงจาก GameObject เดียวกันอัตโนมัติ)")]
    public InteractableBase targetInteractable;

    [Tooltip("เวลาหน่วง (วินาที) ก่อนที่จะเริ่มบังคับ Interact")]
    public float delayInSeconds = 0f;

    private void Start()
    {
        // 1. ถ้าไม่ได้ลากใส่ช่องไว้ ให้พยายามหาจาก Object ตัวเอง
        if (targetInteractable == null)
        {
            targetInteractable = GetComponent<InteractableBase>();
        }

        // 2. ถ้าหาเจอ ให้เริ่มรัน Coroutine สำหรับหน่วงเวลา
        if (targetInteractable != null)
        {
            StartCoroutine(ExecuteInteractWithDelay());
        }
        else
        {
            Debug.LogWarning("[AutoInteractTrigger] หา InteractableBase ไม่เจอ! ลืมแปะสคริปต์หรือเปล่า?");
        }
    }

    private IEnumerator ExecuteInteractWithDelay()
    {
        // 3. ถ้าระยะเวลาหน่วงมากกว่า 0 ให้รอก่อน
        if (delayInSeconds > 0f)
        {
            yield return new WaitForSeconds(delayInSeconds);
        }

        // 4. สั่งบังคับให้ทำงาน เสมือนว่าผู้เล่นเดินมากดปุ่มด้วยตัวเอง!
        targetInteractable.Interact();

        // (Option) ถ้าเป็น Interact แบบต้องกดค้าง (hasDuration = true) 
        // คำสั่ง Interact() จะไปกระตุ้น onProgress = true ให้เอง
        // และ Update ของ InteractableBase จะจัดการนับเวลาต่อให้จนจบครับ
    }
}