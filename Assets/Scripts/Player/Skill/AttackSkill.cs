using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "New AttackDash Skill", menuName = "Skills/Skill/AttackDash Skill")]
public class AttackSkill : SkillS
{
    [Space(20)]
    [Header("===== Modify Skill ================================================================")]

    [Header("Attack on Skill")]
    public GameObject skillPrefabs;
    public SpawnSkillPrefabsType spawnSkillPrefabsType;
    public float skillFar = 0f;
    public float damage;
    public float knockbackForce;

    [Header("Bullte Prefabs")]
    public float speed;

    [Header("Dash on Skill")]
    public bool haveDash;
    public float dashSpeed = 0f; // �������Ǣͧ��þ��
    public float dashTime = 0f; // �������ҡ�þ��
    public float skillSpawnDelay = 0f;

    public enum SpawnSkillPrefabsType
    {
        PlayerParent , PlayerWorld , MouseWorld
    }

    public override Coroutine Use(GameObject player, Vector3 mousePosition)
    {
        // �֧����๹�� PlayerMovement �ҡ GameObject �ͧ������
        if (haveDash && PlayerMovement.instance.TryGetComponent(out PlayerMovement playerMovement))
        {
            // �ӹǳ��ȷҧ��þ��
            Vector3 directionDesh = (mousePosition - player.transform.position).normalized;
            Vector3 attackDirection = (mousePosition - player.transform.position).normalized;

            // ���¡�����ʹ��þ���ʤ�Ի�� PlayerMovement
            // ����͡�÷�� AttackSkill "��ͧ��" �������͹���
            Coroutine skillDelayCoroutine = playerMovement.StartCoroutine(SpawnSkillAfterDelay(player, mousePosition , attackDirection));

            playerMovement.OnSkillDash?.Invoke(directionDesh, dashSpeed, dashTime , skillDelayCoroutine);
        }
        else if(skillPrefabs != null) InstallAttackHit(skillPrefabs, player.transform, mousePosition, (mousePosition - player.transform.position).normalized, damage, knockbackForce);

        return null;
    }

    private IEnumerator SpawnSkillAfterDelay(GameObject player, Vector3 mousePosition , Vector3 attackDirection)
    {
        // �������ҵ������˹�
        yield return new WaitForSeconds(skillSpawnDelay);

        // ����ͤú�������� ������ҧ Skill Prefab �����
        if (skillPrefabs != null) InstallAttackHit(skillPrefabs, player.transform, mousePosition, attackDirection, damage, knockbackForce);
    }

    private void InstallAttackHit(GameObject skillPrefabs, Transform playerTransform, Vector3 mousePosition, Vector3 attackDirection ,  float damage, float knockbackForce)
    {
        GameObject attackInstance = null;
        Vector3 directionToMouse = (mousePosition - playerTransform.position).normalized;
        Vector3 posInstance = Vector3.zero;
        Vector3 targetVecter = Vector3.zero;
        
        // ���ҧ GameObject �ͧ�������
        switch (spawnSkillPrefabsType)
        {
            case SpawnSkillPrefabsType.PlayerParent:
                attackInstance = Instantiate(skillPrefabs, playerTransform);
                posInstance = playerTransform.position + (attackDirection * skillFar);
                targetVecter = attackDirection;
                break;
            case SpawnSkillPrefabsType.PlayerWorld:
                attackInstance = Instantiate(skillPrefabs, playerTransform.position, Quaternion.identity);
                posInstance = playerTransform.position + (attackDirection * skillFar);
                targetVecter = attackDirection;
                break;
            case SpawnSkillPrefabsType.MouseWorld:
                attackInstance = Instantiate(skillPrefabs, mousePosition, Quaternion.identity);
                float skillFarTrue = (skillFar >= 0) ? Mathf.Clamp(Vector3.Distance(mousePosition, playerTransform.position), 0, skillFar) : Vector3.Distance(mousePosition, playerTransform.position);
                posInstance = playerTransform.position + (directionToMouse * skillFarTrue);
                targetVecter = mousePosition - playerTransform.position;
                break;
        }

        attackInstance.transform.position = posInstance;

        // �ӹǳ�����ع (Rotation)
        targetVecter.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(targetVecter);
        attackInstance.transform.rotation = targetRotation;

        if (attackInstance.TryGetComponent(out IHurtBox iHurtBox))
        {
            iHurtBox._damage = damage;
            iHurtBox._knockbackDirection = directionToMouse;
            iHurtBox._knockbackForce = knockbackForce;
        }
        if (attackInstance.TryGetComponent(out ISpeed iSpeed))
        {
            iSpeed._speed = speed;
        }

        EndSkilling();
    }
}