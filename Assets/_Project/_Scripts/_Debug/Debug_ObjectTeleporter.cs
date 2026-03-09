using UnityEngine;

public class Debug_ObjectTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("Object ที่ต้องการให้วาร์ป")]
    public Transform targetObject;

    [Tooltip("พิกัดตำแหน่งเป้าหมายที่ต้องการให้วาร์ปไป")]
    public Vector3 targetPosition;

    /// <summary>
    /// ฟังก์ชันสำหรับเรียกใช้วาร์ป (สามารถนำไปผูกกับ UI Button หรือเงื่อนไขอื่นๆ ได้)
    /// </summary>
    [ContextMenu("Teleport")]
    public void Teleport()
    {
        if (targetObject != null)
        {
            // ทำการวาร์ปโดยการเปลี่ยนค่า position
            targetObject.position = targetPosition;
            //Debug.Log($"Warped {targetObject.name} to {targetPosition}");
        }
        else
        {
            Debug.LogWarning("ไม่สามารถวาร์ปได้ กรุณาใส่ Target Object ใน Inspector ด้วยครับ");
        }
    }

    /// <summary>
    /// ฟังก์ชัน ContextMenu สำหรับเซ็ตค่า targetPosition ให้เท่ากับตำแหน่งปัจจุบันของ targetObject
    /// </summary>
    [ContextMenu("Set Target Position to Object Position")]
    private void GetTargetObjectPosition()
    {
        if (targetObject != null)
        {
            // ดึงค่าตำแหน่งปัจจุบันของ targetObject มาเก็บไว้ใน targetPosition
            targetPosition = targetObject.position;
            //Debug.Log($"Set targetPosition to {targetObject.name}'s current position: {targetPosition}");
        }
        else
        {
            Debug.LogWarning("กรุณาใส่ Target Object ก่อนคลิกใช้งานคำสั่งนี้นะครับ");
        }
    }

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

}
