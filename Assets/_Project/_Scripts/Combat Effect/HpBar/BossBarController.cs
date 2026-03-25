public class BossBarController : BaseHpBarController
{
    public BossBarUI bossBarUI;

    protected override void Awake()
    {
        base.Awake();

    }

    protected virtual void Start()
    {
        if (HpBarManager.Instance != null) bossBarUI = HpBarManager.Instance.bossBarUI;
    }
    protected override void ShowUI(string enemyName)
    {
        if (bossBarUI != null)
        {
            bossBarUI.SetData(enemyHealth, enemyName);
        }
    }

    protected override void HideUI()
    {
        if (bossBarUI != null)
        {
            bossBarUI.DisableBar();
        }
    }
}