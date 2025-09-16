using UnityEngine;
using System.Linq;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("ขนาดครึ่งหนึ่งของกล่องในการตรวจจับ")]
    public Vector3 overlapBoxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    [Tooltip("Layer ที่มีวัตถุที่สามารถโต้ตอบได้")]
    public LayerMask interactableLayer;

    private Vector3 _directionToMouse;
    private IInteractable _currentInteractable;

    private void Update()
    {
        // คำนวณทิศทางจากผู้เล่นไปยังเมาส์
        Vector3 mousePositionOnPlane = GetMousePositionOnPlane();
        _directionToMouse = (mousePositionOnPlane - transform.position).normalized;

        // ตรวจสอบวัตถุ Interactable ที่อยู่ภายในกล่อง
        CheckForInteractable();

        // เมื่อผู้เล่นกดปุ่ม 'E' และมีวัตถุที่โต้ตอบได้ถูกตรวจจับ
        if (Input.GetKeyDown(KeyCode.E) && _currentInteractable != null)
        {
            _currentInteractable._OnInteract?.Invoke();
            //Debug.Log($"Interacted with {_currentInteractable.gameObje}");
        }
    }

    private void CheckForInteractable()
    {
        // คำนวณตำแหน่งกึ่งกลางของกล่อง OverlapBox
        Vector3 boxCenter = transform.position + _directionToMouse * (overlapBoxHalfExtents.z);

        // ใช้ Physics.OverlapBox เพื่อหา Collider ที่อยู่ในบริเวณ
        Collider[] hitColliders = Physics.OverlapBox(
            boxCenter,
            overlapBoxHalfExtents,
            Quaternion.LookRotation(_directionToMouse),
            interactableLayer
        );

        if (hitColliders.Length > 0)
        {
            // ใช้ LINQ เพื่อหา Collider ตัวที่ใกล้ที่สุดและมีคอมโพเนนต์ Interactable
            _currentInteractable = hitColliders
                .Where(c => c.GetComponent<IInteractable>() != null)
                .OrderBy(c => Vector3.Distance(transform.position, c.transform.position))
                .FirstOrDefault()?.GetComponent<IInteractable>();
        }
        else
        {
            _currentInteractable = null;
        }
    }

    // ฟังก์ชันช่วยในการคำนวณตำแหน่งเมาส์ใน World Space
    private Vector3 GetMousePositionOnPlane()
    {
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float hitDist;
        if (playerPlane.Raycast(ray, out hitDist))
        {
            return ray.GetPoint(hitDist);
        }
        return Vector3.zero;
    }

    // ใช้สำหรับ Debug เพื่อแสดงกล่องตรวจจับใน Scene view
    private void OnDrawGizmos()
    {
        // คำนวณตำแหน่งกึ่งกลางของกล่องสำหรับ Gizmos
        Vector3 boxCenter = transform.position + _directionToMouse * (overlapBoxHalfExtents.z);

        Gizmos.color = _currentInteractable != null ? Color.green : Color.yellow;
        // วาด Wire Cube เพื่อแสดงขอบเขตการตรวจจับ
        Gizmos.DrawWireCube(boxCenter, overlapBoxHalfExtents * 2);

        // วาดเส้นไปยังวัตถุที่ถูกเลือก
        if (_currentInteractable != null)
        {
            Gizmos.color = Color.green;
            //Gizmos.DrawLine(transform.position, _currentInteractable.transform.position);
        }
    }
}