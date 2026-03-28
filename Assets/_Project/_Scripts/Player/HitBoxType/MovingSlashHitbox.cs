using UnityEngine;

public class MovingSlashHitbox : MonoBehaviour
{
    [Header("Slash Settings")]
    public float moveDuration = 0.15f;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _timer;
    private bool _isMoving = false;

    // *** แก้ตรงนี้: รับค่า duration เข้ามาด้วย ***
    public void Setup(Vector3 start, Vector3 end, float duration)
    {
        _startPos = start;
        _endPos = end;
        moveDuration = duration; // เอาค่าที่ส่งมาจาก Controller มาทับค่าเดิม!
        _timer = 0f;
        _isMoving = true;
        transform.position = start;
        transform.LookAt(end);
    }

    private void Update()
    {
        if (!_isMoving) return;

        _timer += Time.deltaTime;
        float percent = _timer / moveDuration;

        transform.position = Vector3.Lerp(_startPos, _endPos, percent);

        if (percent >= 1f)
        {
            _isMoving = false;
            if (ObjectPoolingManager.Instance != null)
            {
                ObjectPoolingManager.Instance.Respawn(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}