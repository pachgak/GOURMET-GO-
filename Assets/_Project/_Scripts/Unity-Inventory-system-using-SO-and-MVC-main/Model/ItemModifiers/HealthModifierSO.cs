using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New HealthModifier", menuName = "Inventory/Modifier/HealthSO")]
public class HealthModifierSO : ItemModifierSO
{
    public override bool AffectCharacter(GameObject character, float val) // , int lvlQuality
    {
        Debug.Log($"{character.name} : Health {val}");
        PlayerHealth health = character.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.addHp((int)val);
            return true;
        }
        return false;
    }
}
