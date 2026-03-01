using UnityEngine;

public class SkyChickenVFXController : MonoBehaviour
{
    [Header("VFX Settings")]
    public GameObject featherVfxPrefab;
    public Transform bodyRoot;

    // *** เพิ่มตัวแปร Offset ตรงนี้ ***
    public Vector3 offset = Vector3.zero;

    private LatchController _latchController;
    private EnemyHealth _enemyHealth;
    private GameObject _currentFeatherVfx;

    private void Awake()
    {
        _latchController = GetComponent<LatchController>();
        _enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (_latchController != null) _latchController.OnLatchStateChanged += HandleLatchStateChanged;
        if (_enemyHealth != null) _enemyHealth.OnDie += HandleDeath;
    }

    private void OnDisable()
    {
        if (_latchController != null) _latchController.OnLatchStateChanged -= HandleLatchStateChanged;
        if (_enemyHealth != null) _enemyHealth.OnDie -= HandleDeath;

        RemoveVFX();
    }

    private void HandleLatchStateChanged(bool isLatched)
    {
        if (isLatched)
        {
            if (_currentFeatherVfx == null && featherVfxPrefab != null)
            {
                Transform parentTransform = (bodyRoot != null) ? bodyRoot : transform;
                _currentFeatherVfx = ObjectPoolingManager.Instance.Spawn(featherVfxPrefab, parentTransform);

                // *** เปลี่ยนจาก Vector3.zero เป็น offset ***
                _currentFeatherVfx.transform.localPosition = offset;
            }
        }
        else
        {
            RemoveVFX();
        }
    }

    private void HandleDeath()
    {
        RemoveVFX();
    }

    private void RemoveVFX()
    {
        if (_currentFeatherVfx != null)
        {
            ObjectPoolingManager.Instance.Respawn(_currentFeatherVfx);
            _currentFeatherVfx = null;
        }
    }
}