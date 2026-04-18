using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoEZoneHitbox : BaseHitBox
{
    public float tickRate = 0.5f; // โดนดาเมจทุกๆ 0.5 วิ

    private Dictionary<Collider, float> _nextDamageTime = new Dictionary<Collider, float>();

    Collider _colider;

    private void Awake()
    {
        _colider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        _nextDamageTime.Clear();
    }
    private void ReturnObjectToPool()
    {
        _colider.enabled = false;
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay");
        // เช็ค Layer แบบ Bitwise
        if (((1 << other.gameObject.layer) & targetLayer.value) != 0)
        {
            Debug.Log($"Leyer {other.name}");
            // เช็คว่าถึงเวลาโดนดาเมจรอบใหม่หรือยัง
            if (!_nextDamageTime.ContainsKey(other) || Time.time >= _nextDamageTime[other])
            {
                Debug.Log($"_nextDamageTime");
                // ทำดาเมจ
                if (other.TryGetComponent(out ITakeDamage takeDamage))
                {
                    takeDamage.TakeDamage(damage);
                    // Update เวลาครั้งถัดไป
                    _nextDamageTime[other] = Time.time + tickRate;

                    Debug.Log($"Damage {damage}");
                }
            }
        }
    }

    public override void PerformAttack()
    {
        _colider.enabled = true;
    }

    public void DisableAttack()
    {
        _colider.enabled = false;
    }

}