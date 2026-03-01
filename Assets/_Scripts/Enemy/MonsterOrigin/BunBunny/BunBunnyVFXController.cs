using UnityEngine;

public class BunBunnyVFXController : MonoBehaviour
{
    [Header("VFX Settings")]
    public GameObject dustVfxPrefab;
    public Transform feetRoot;

    // *** เพิ่มตัวแปร Offset ตรงนี้ ***
    public Vector3 offset = Vector3.zero;

    private BaseEnemyAI _aiController;
    private EnemyHealth _enemyHealth;
    private GameObject _currentDustVfx;

    private void Awake()
    {
        _aiController = GetComponent<BaseEnemyAI>();
        _enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (_aiController != null) _aiController.OnStateChange += HandleStateChange;
        if (_enemyHealth != null) _enemyHealth.OnDie += HandleDeath;
    }

    private void OnDisable()
    {
        if (_aiController != null) _aiController.OnStateChange -= HandleStateChange;
        if (_enemyHealth != null) _enemyHealth.OnDie -= HandleDeath;

        RemoveVFX();
    }

    private void HandleStateChange(BaseEnemyAI.EnemyState newState)
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;

        if (newState == BaseEnemyAI.EnemyState.Chase)
        {
            if (_currentDustVfx == null && dustVfxPrefab != null)
            {
                Transform parentTransform = (feetRoot != null) ? feetRoot : transform;
                _currentDustVfx = ObjectPoolingManager.Instance.Spawn(dustVfxPrefab, parentTransform);

                // *** เปลี่ยนจาก Vector3.zero เป็น offset ***
                _currentDustVfx.transform.localPosition = offset;
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
        if (_currentDustVfx != null)
        {
            ObjectPoolingManager.Instance.Respawn(_currentDustVfx);
            _currentDustVfx = null;
        }
    }
}