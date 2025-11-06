using UnityEngine;

public class SettingPlayerControllerManager : MonoBehaviour
{
    public static SettingPlayerControllerManager instance;

    public AttackDiractionType meleeAttackDiraction;
    public AttackDiractionType skillDiraction;
    public AttackDiractionType dashDiraction;

    public enum AttackDiractionType
    {
        mouse, movement
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

    }

}


