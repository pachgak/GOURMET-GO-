using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "New AttacksDash Skill", menuName = "Skills/Skill/AttacksDash Skill")]
public class AttacksSkill : SkillS
{
    //[Space(20)]
    //[Header("===== Modify Skill ================================================================")]
    public SpawnSkillPrefabsType spawnSkillPrefabsType;
    public SkillSetp[] skillSetp;

    [System.Serializable]
    public class SkillSetp
    {
        [Header("Time")]
        public float playAtTime;

        [Header("Attack on Skill")]
        public GameObject skillPrefabs;
        public float skillFar = 0f;
        public float skillHight = 0f;
        public float damage;
        public float knockbackForce;

        [Header("Bullte Prefabs")]
        public float speed;

        [Header("Dash on Skill")]
        public bool haveDash;
        public float dashSpeed = 0f; // ความเร็วของการพุ่ง
        public float dashTime = 0f; // ระยะเวลาการพุ่ง
    }

    public enum SpawnSkillPrefabsType
    {
        PlayerParent , PlayerWorld , MouseWorld
    }

    public override Coroutine Use(GameObject player, Vector3 mousePosition)
    {
        Coroutine setplaySkill = PlayerSkillController.instance.StartCoroutine(Setplay(player, mousePosition));
        return setplaySkill;
    }

    private IEnumerator Setplay(GameObject player, Vector3 mousePosition)
    {
        for (int i = 0; i < skillSetp.Length; i++)
        {
            if (i == 0) yield return new WaitForSeconds(skillSetp[i].playAtTime);
            else yield return new WaitForSeconds(skillSetp[i].playAtTime - skillSetp[i-1].playAtTime);

            float dashSpeed = skillSetp[i].dashSpeed;
            float dashTime = skillSetp[i].dashTime;
            GameObject skillPrefabs = skillSetp[i].skillPrefabs;
            bool haveDash = skillSetp[i].haveDash;
            float damage = skillSetp[i].damage;
            float knockbackForce = skillSetp[i].knockbackForce;
            float speed = skillSetp[i].speed;
            float skillFar = skillSetp[i].skillFar;

            if (haveDash && PlayerMovement.instance.TryGetComponent(out PlayerMovement playerMovement))
            {
                // คำนวณทิศทางการพุ่ง
                Vector3 directionDesh = (mousePosition - player.transform.position).normalized;
                Vector3 attackDirection = (mousePosition - player.transform.position).normalized;

                playerMovement.OnSkillDash?.Invoke(directionDesh, dashSpeed, dashTime, null);
            }
            else if (skillPrefabs != null) InstallAttackHit(skillPrefabs, player.transform, mousePosition, (mousePosition - player.transform.position).normalized, skillFar, damage, knockbackForce, speed);
        }

        Debug.Log("Setplay End");
        //EndSkilling();
    }

    private void InstallAttackHit(GameObject skillPrefabs, Transform playerTransform, Vector3 mousePosition, Vector3 attackDirection , float skillFar ,  float damage, float knockbackForce,float speed)
    {
        GameObject attackInstance = null;
        Vector3 directionToMouse = (mousePosition - playerTransform.position).normalized;
        Vector3 posInstance = Vector3.zero;
        Vector3 targetVecter = Vector3.zero;
        
        // สร้าง GameObject ของการโจมตี
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

        // คำนวณการหมุน (Rotation)
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
    }
}