using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    [Tooltip("ลาก Grid มาใส่ หรือปล่อยว่างไว้เพื่อดึงจาก SnapToGridManager อัตโนมัติ")]
    public Grid targetGrid;

    void Start()
    {
        Snap();
    }

    [ContextMenu("Snap to Grid Now")]
    public void Snap()
    {
        // 1. ตรวจสอบว่ามี Grid หรือยัง ถ้าไม่มีให้ดึงจาก Manager
        if (targetGrid == null)
        {
            if (SnapToGridManager.instance != null)
            {
                targetGrid = SnapToGridManager.instance.targetGrid;
            }

            if (targetGrid == null)
            {
                //Debug.LogWarning($"{gameObject.name}: ไม่พบ Grid! กรุณาใส่ Grid ใน SnapToGridManager หรือที่ตัวสคริปต์เอง");
                return;
            }
        }

        // 2. แปลงตำแหน่งโลก ไปเป็นพิกัดช่องตาราง
        Vector3Int cellPosition = targetGrid.WorldToCell(transform.position);

        // 3. หาจุดกึ่งกลางของช่องตารางนั้นในโลก
        Vector3 snappedPosition = targetGrid.GetCellCenterWorld(cellPosition);

        // 4. ล็อกแกน Y ไว้ให้อยู่ตำแหน่งเดิม
        snappedPosition.y = transform.position.y;

        // 5. อัปเดตตำแหน่งของ Object
        transform.position = snappedPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Grid gridToUse = targetGrid;

        // ถ้ายังไม่มีเป้าหมาย ให้พยายามหาจาก Manager
        if (gridToUse == null)
        {
            // กรณีที่ 1: กำลังกด Play อยู่ (มี Instance แน่นอน)
            if (SnapToGridManager.instance != null)
            {
                gridToUse = SnapToGridManager.instance.targetGrid;
            }
            // กรณีที่ 2: อยู่ในโหมด Editor จัดฉาก (ยังไม่ได้กด Play, Instance จะยังเป็น null)
            else
            {
                SnapToGridManager manager = FindAnyObjectByType<SnapToGridManager>();
                if (manager != null)
                {
                    gridToUse = manager.targetGrid;
                }
            }
        }

        // ถ้าหาไม่เจอจริงๆ ให้หยุดวาด
        if (gridToUse == null) return;

        // --- เริ่มคำนวณและวาด Gizmos ---
        Vector3Int cellPosition = gridToUse.WorldToCell(transform.position);
        Vector3 targetPosition = gridToUse.GetCellCenterWorld(cellPosition);
        targetPosition.y = transform.position.y;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, targetPosition);

        Gizmos.color = Color.green;

        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(targetPosition, gridToUse.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, gridToUse.cellSize);
        Gizmos.matrix = oldGizmosMatrix;
    }
}