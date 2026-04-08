using UnityEngine;

public class DamagePromptController : MonoBehaviour
{
    public GameObject damagePromptPrefab;
    public Vector3 offSet;
    public ITakeDamage takeDamage;

    public void Awake()
    {
        takeDamage = GetComponent<ITakeDamage>();
    }

    private void OnEnable()
    {
        takeDamage.OnTakeDamage += HandleInstantiateEffect;
    }
    private void OnDisable()
    {
        takeDamage.OnTakeDamage -= HandleInstantiateEffect;
    }

    public void HandleInstantiateEffect(float damage, GameObject customHitVFX = null)
    {
        Vector3 effectPos = transform.position + offSet;

        if (damagePromptPrefab != null)
        {
            //GameObject cloneDamagePrompt = Instantiate(damagePromptPrefab, transform);
            GameObject cloneDamagePrompt = ObjectPoolingManager.Instance.Spawn(damagePromptPrefab);
            cloneDamagePrompt.transform.position = effectPos;
            if (cloneDamagePrompt.TryGetComponent(out DamagePromptText damagePrompt))
            {
                damagePrompt.SetDamageText(damage, offSet, transform);
            }
        }


    }

    private void OnDrawGizmosSelected()
    {
        // กำหนดสีของ Gizmos (เปลี่ยนสีได้ตามต้องการ)
        Gizmos.color = Color.red;

        // วาดจุด (Sphere) ขนาดเล็กที่ตำแหน่งของ Object + offSet
        Gizmos.DrawSphere(transform.position + offSet, 0.15f);
    }
}
