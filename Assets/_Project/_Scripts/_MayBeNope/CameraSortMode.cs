using UnityEngine;

[ExecuteAlways] // เพื่อให้สคริปต์ทำงานและแสดงผลในหน้า Scene View ได้ทันที
[RequireComponent(typeof(Camera))]
public class CameraSortMode : MonoBehaviour
{
    void OnEnable()
    {
        Camera cam = GetComponent<Camera>();

        // บังคับให้เรียงตาม Custom Axis
        cam.transparencySortMode = TransparencySortMode.CustomAxis;

        // กำหนดให้เรียงตามแกน Z
        cam.transparencySortAxis = new Vector3(0, 0, 1);

        // หมายเหตุ: หากภาพยังเรียงกลับด้าน (หน้าไปหลัง/หลังมาหน้า) ให้ลองเปลี่ยนค่า Z เป็น -1 ครับ
    }
}