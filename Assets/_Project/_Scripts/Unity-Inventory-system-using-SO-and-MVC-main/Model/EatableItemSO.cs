using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "New EatableItem", menuName = "Inventory/Item/EatableItemSO")]
    public class EatableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        public List<ModifierData> modifiersData = new List<ModifierData>();

        public string ActionName => "Consume";

        [field: SerializeField]
        public AudioClip actionSFX {get; private set;}

        public bool PerformAction(GameObject character
            //,List<ItemParameter> itemParameter = null
            )
        {
            Debug.Log($"PerformAction : {ItemName}");
            if(modifiersData.Count <= 0) return false;

            foreach (ModifierData data in modifiersData)
            {
                data.statModifierSO.AffectCharacter(character, data.value);
            }
            return true;
        }
    }


}