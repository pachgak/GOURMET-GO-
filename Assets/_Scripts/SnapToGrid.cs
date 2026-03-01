using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    [Tooltip("ลาก Grid ที่เป็นตัวแม่ของ Tilemap มาใส่ช่องนี้ (ถ้าปล่อยว่าง สคริปต์จะหา Grid ในฉากให้อัตโนมัติ)")]
    public Grid targetGrid;

    // ทำงานอัตโนมัติเมื่อกด Play
    void Start()
    {
        Snap();
    }

    // [ContextMenu] ช่วยให้เราคลิกขวาที่ชื่อสคริปต์ใน Inspector แล้วกดสั่งทำงานได้เลยในโหมด Editor
    [ContextMenu("Snap to Grid Now")]
    public void Snap()
    {
        // 1. ตรวจสอบว่ามี Grid หรือยัง ถ้าไม่มีให้หาใน Scene
        if (targetGrid == null)
        {
            targetGrid = FindAnyObjectByType<Grid>();

            if (targetGrid == null)
            {
                Debug.LogWarning("ไม่พบ Grid ใน Scene! กรุณาสร้างหรือลาก Grid มาใส่ที่สคริปต์");
                return;
            }
        }

        // 2. แปลงตำแหน่งโลก (World Position) ปัจจุบัน ไปเป็นพิกัดช่องตาราง (Cell Position)
        Vector3Int cellPosition = targetGrid.WorldToCell(transform.position);

        // 3. หาจุดกึ่งกลางของช่องตารางนั้นในโลก (World Space)
        Vector3 snappedPosition = targetGrid.GetCellCenterWorld(cellPosition);

        // 4. ล็อกแกน Y ไว้ให้อยู่ตำแหน่งเดิม (ตามโค้ดของคุณ)
        snappedPosition.y = transform.position.y;

        // 5. อัปเดตตำแหน่งของ Object ให้ไปอยู่ตรงกึ่งกลาง Grid
        transform.position = snappedPosition;
    }

    // ฟังก์ชันใหม่: วาดเส้นและจุดเป้าหมายในหน้าต่าง Scene เมื่อเลือก Object นี้
    private void OnDrawGizmosSelected()
    {
        // สร้างตัวแปรชั่วคราวเพื่ออ้างอิง Grid ในโหมด Editor
        Grid gridToUse = targetGrid;
        if (gridToUse == null)
        {
            gridToUse = FindAnyObjectByType<Grid>();
        }

        // ถ้าหา Grid ไม่เจอเลย ให้ข้ามการวาดไป เพื่อป้องกัน Error
        if (gridToUse == null) return;

        // คำนวณหาตำแหน่งที่มันควรจะไป Snap
        Vector3Int cellPosition = gridToUse.WorldToCell(transform.position);
        Vector3 targetPosition = gridToUse.GetCellCenterWorld(cellPosition);

        // ล็อกแกน Y ตามโค้ดในเมธอด Snap ของคุณ
        targetPosition.y = transform.position.y;

        // --- เริ่มวาด Gizmos ---

        // ตั้งค่าสีเป็นสีเหลือง
        Gizmos.color = Color.yellow;

        // วาดเส้นจากตำแหน่งปัจจุบันของ Object ไปยังจุดศูนย์กลางของ Grid ที่จะไป Snap
        Gizmos.DrawLine(transform.position, targetPosition);

        // ตั้งค่าสีใหม่ให้กล่องเป้าหมาย (สีเขียว)
        Gizmos.color = Color.green;

        // --- เทคนิคหมุน Gizmo ตาม Grid ---
        // 1. จำค่า Matrix ปกติของ Scene เอาไว้ก่อน
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        // 2. เปลี่ยนองศาและตำแหน่งของ Gizmos ให้ตรงกับเป้าหมาย และการหมุน (Rotation) ของ Grid
        Gizmos.matrix = Matrix4x4.TRS(targetPosition, gridToUse.transform.rotation, Vector3.one);

        // 3. วาดกล่องที่จุดศูนย์กลาง (Vector3.zero เพราะเราเลื่อน Matrix ไปที่เป้าหมายแล้ว) ด้วยขนาดของช่อง Grid
        Gizmos.DrawWireCube(Vector3.zero, gridToUse.cellSize);

        // 4. คืนค่า Matrix กลับเป็นปกติ เพื่อไม่ให้กระทบกับการวาด Gizmos ตัวอื่นๆ ในฉาก
        Gizmos.matrix = oldGizmosMatrix;
    }
}