using UnityEngine;
using System.Linq;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("��Ҵ����˹�觢ͧ���ͧ㹡�õ�Ǩ�Ѻ")]
    public Vector3 overlapBoxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    [Tooltip("Layer ������ѵ�ط������ö��ͺ��")]
    public LayerMask interactableLayer;

    private Vector3 _directionToMouse;
    private IInteractable _currentInteractable;

    private void Update()
    {
        // �ӹǳ��ȷҧ�ҡ��������ѧ�����
        Vector3 mousePositionOnPlane = GetMousePositionOnPlane();
        _directionToMouse = (mousePositionOnPlane - transform.position).normalized;

        // ��Ǩ�ͺ�ѵ�� Interactable ����������㹡��ͧ
        CheckForInteractable();

        // ����ͼ����蹡����� 'E' ������ѵ�ط����ͺ��١��Ǩ�Ѻ
        if (Input.GetKeyDown(KeyCode.E) && _currentInteractable != null)
        {
            _currentInteractable._OnInteract?.Invoke();
            //Debug.Log($"Interacted with {_currentInteractable.gameObje}");
        }
    }

    private void CheckForInteractable()
    {
        // �ӹǳ���˹觡�觡�ҧ�ͧ���ͧ OverlapBox
        Vector3 boxCenter = transform.position + _directionToMouse * (overlapBoxHalfExtents.z);

        // �� Physics.OverlapBox ������ Collider �������㹺���ǳ
        Collider[] hitColliders = Physics.OverlapBox(
            boxCenter,
            overlapBoxHalfExtents,
            Quaternion.LookRotation(_directionToMouse),
            interactableLayer
        );

        if (hitColliders.Length > 0)
        {
            // �� LINQ ������ Collider ��Ƿ��������ش����դ���๹�� Interactable
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

    // �ѧ��ѹ����㹡�äӹǳ���˹������� World Space
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

    // ������Ѻ Debug �����ʴ����ͧ��Ǩ�Ѻ� Scene view
    private void OnDrawGizmos()
    {
        // �ӹǳ���˹觡�觡�ҧ�ͧ���ͧ����Ѻ Gizmos
        Vector3 boxCenter = transform.position + _directionToMouse * (overlapBoxHalfExtents.z);

        Gizmos.color = _currentInteractable != null ? Color.green : Color.yellow;
        // �Ҵ Wire Cube �����ʴ��ͺࢵ��õ�Ǩ�Ѻ
        Gizmos.DrawWireCube(boxCenter, overlapBoxHalfExtents * 2);

        // �Ҵ�����ѧ�ѵ�ط��١���͡
        if (_currentInteractable != null)
        {
            Gizmos.color = Color.green;
            //Gizmos.DrawLine(transform.position, _currentInteractable.transform.position);
        }
    }
}