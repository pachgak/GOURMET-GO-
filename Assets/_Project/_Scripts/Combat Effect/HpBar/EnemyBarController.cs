using UnityEngine;

public class EnemyBarController : BaseHpBarController
{
    private EnemyBarUI enemyBarUIPrefab;
    private Canvas canvasWorldParant;
    public Vector3 offset;

    private EnemyBarUI currentEnemyBarUI; // เก็บ UI ตัวปัจจุบันที่ดึงมาจาก Pool

    protected EnemyHealth _healthTarget;

    // เพิ่มบรรทัดนี้เข้าไป เพื่อให้ภายนอกดึงค่าไปเช็คได้ว่ากำลังโชว์เลือดใครอยู่
    public EnemyHealth CurrentHealthTarget => _healthTarget;

    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void Start()
    {
        if (HpBarManager.Instance != null)
        {
            enemyBarUIPrefab = HpBarManager.Instance.enemyBarUIPrefab;
            canvasWorldParant = HpBarManager.Instance.canvasWorldParent;
        }
    }

    protected override void Update()
    {
        base.Update(); // รันระบบ Timer ของคลาสแม่

        // ถ้ามี UI แสดงอยู่ ให้มันตามตำแหน่งของศัตรู
        if (currentEnemyBarUI != null && currentEnemyBarUI.gameObject.activeSelf)
        {
            currentEnemyBarUI.transform.position = transform.position + offset;
        }
    }

    protected override void ShowUI(string enemyName)
    {
        // ถ้ายังไม่มี UI หรือ UI ถูกซ่อนไปแล้ว ให้เสกมาใหม่
        if (currentEnemyBarUI == null || !currentEnemyBarUI.gameObject.activeSelf)
        {
            GameObject enemyBarClone = ObjectPoolingManager.Instance.Spawn(enemyBarUIPrefab.gameObject, canvasWorldParant.transform);
            currentEnemyBarUI = enemyBarClone.GetComponent<EnemyBarUI>();
        }

        currentEnemyBarUI.SetData(enemyHealth, enemyName);
    }

    protected override void HideUI()
    {
        if (currentEnemyBarUI != null)
        {
            currentEnemyBarUI.DisableBar();
            currentEnemyBarUI = null; // เคลียร์ Reference ทิ้ง
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + offset, 0.1f);
    }
}