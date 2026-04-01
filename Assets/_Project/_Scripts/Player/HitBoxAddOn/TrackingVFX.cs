using UnityEngine;

public class TrackingVFX : MonoBehaviour, ITargetable , IDurationable
{
    [Tooltip("ระยะเวลาที่จะวิ่งตามเป้าหมาย")]
    public float trackingDuration = 1.0f;
    [Tooltip("ความเร็วในการเลื่อนตาม (Lerp) ยิ่งน้อยยิ่งตามช้า ยิ่งมากยิ่งตามติดหนึบ")]
    public float followSpeed = 10f;

    [SerializeField] private Transform target;
    [SerializeField] private float timer;
    [SerializeField] private bool isTracking = false;

    private void OnEnable()
    {
        timer = trackingDuration;
        isTracking = true;
    }

    public void SetTarget(Transform targetTransform)
    {
        Debug.Log($"SetTarget() : {targetTransform.name}");

        this.target = targetTransform;
    }

    private void Update()
    {
        if (!isTracking || target == null) return;

        timer -= Time.deltaTime;
        if (timer > 0)
        {
            // เลื่อนตามผู้เล่นแบบ Smooth
            Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        }
        else
        {
            // หมดเวลาตาม -> หยุดนิ่ง
            isTracking = false;
        }
    }

    public void SetDurationTime(float duration)
    {
        Debug.Log($"SetDurationTime() : {duration}");

        this.trackingDuration = duration;
    }
}