using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemModifierSO : ScriptableObject
{
    [field: SerializeField]
    public string ModifierName { get; set; }

    [field: SerializeField]
    [field: TextArea]
    public string Description { get; set; }

    [field: SerializeField]
    public Sprite Image { get; set; }

    public abstract bool AffectCharacter(GameObject character, float val);
}
