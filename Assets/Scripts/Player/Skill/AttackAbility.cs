using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "New Attack Skill", menuName = "Skills/AbilitySkill/Attack Skill")]
public class AttackAbility : AbilitySkill
{

    [Header("Attack on Skill")]
    public GameObject skillPrefabs;
    public SpawnSkillPrefabsType spawnSkillPrefabsType;
    public float skillFar = 0f;
    public float damage;
    public float knockbackForce;

    [Header("Dash on Skill")]
    public bool haveDash;
    public float dashSpeed = 0f; // ความเร็วของการพุ่ง
    public float dashTime = 0f; // ระยะเวลาการพุ่ง
    public float skillSpawnDelay = 0f;

    public bool reverdDiraction = false;

    public enum SpawnSkillPrefabsType
    {
        PlayerParent , PlayerWorld , MouseWorld
    }

    public override void Use(GameObject player, Vector3 mousePosition)
    {
        // ดึงคอมโพเนนต์ PlayerMovement จาก GameObject ของผู้เล่น
        if (haveDash && player.TryGetComponent(out PlayerMovement playerMovement))
        {
            // คำนวณทิศทางการพุ่ง
            Vector3 directionDesh = (mousePosition - player.transform.position).normalized;
            if (reverdDiraction) directionDesh *= -1;

            // เรียกใช้เมธอดการพุ่งในสคริปต์ PlayerMovement
            // นี่คือการที่ AttackSkill "ร้องขอ" การเคลื่อนที่
            Coroutine skillDelayCoroutine = playerMovement.StartCoroutine(SpawnSkillAfterDelay(player, mousePosition));

            playerMovement.OnSkillDash?.Invoke(directionDesh, dashSpeed, dashTime , skillDelayCoroutine);
        }
        else InstallAttackHit(skillPrefabs, player.transform, mousePosition, damage, knockbackForce);
    }

    private IEnumerator SpawnSkillAfterDelay(GameObject player, Vector3 mousePosition)
    {
        // รอเป็นเวลาตามที่กำหนด
        yield return new WaitForSeconds(skillSpawnDelay);

        // เมื่อครบเวลาแล้ว ให้สร้าง Skill Prefab ขึ้นมา
        InstallAttackHit(skillPrefabs, player.transform, mousePosition, damage, knockbackForce);
    }

    private void InstallAttackHit(GameObject skillPrefabs, Transform playerTransform, Vector3 mousePosition, float damage, float knockbackForce)
    {
        GameObject attackInstance = null;
        Vector3 directionToMouse = (mousePosition - playerTransform.position).normalized;


        // สร้าง GameObject ของการโจมตี
        switch (spawnSkillPrefabsType)
        {
            case SpawnSkillPrefabsType.PlayerParent:
                attackInstance = Instantiate(skillPrefabs, playerTransform);
                attackInstance.transform.position = playerTransform.position + (directionToMouse * skillFar);
                break;
            case SpawnSkillPrefabsType.PlayerWorld:
                attackInstance = Instantiate(skillPrefabs, playerTransform.position, Quaternion.identity);
                attackInstance.transform.position = playerTransform.position + (directionToMouse * skillFar);
                break;
            case SpawnSkillPrefabsType.MouseWorld:
                attackInstance = Instantiate(skillPrefabs, mousePosition, Quaternion.identity);
                float skillFarTrue = (skillFar <= 0) ? Mathf.Clamp(Vector3.Distance(mousePosition, playerTransform.position), 0, skillFar) : Vector3.Distance(mousePosition, playerTransform.position);
                attackInstance.transform.position = playerTransform.position + (directionToMouse * skillFarTrue);
                break;
        }
        
        // คำนวณการหมุน (Rotation)
        Vector3 targetVecter = mousePosition - playerTransform.position;
        targetVecter.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(targetVecter);
        attackInstance.transform.rotation = targetRotation;

        if (attackInstance.TryGetComponent(out IHurtBox iHurtBox))
        {
            iHurtBox._damage = damage;
            iHurtBox._knockbackDirection = directionToMouse;
            iHurtBox._knockbackForce = knockbackForce;
        }
    }
}