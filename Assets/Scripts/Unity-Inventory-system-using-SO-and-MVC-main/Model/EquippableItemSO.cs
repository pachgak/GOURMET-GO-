using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    //[CreateAssetMenu(fileName = "New EquippableItemSO", menuName = "Inventory/Item/EquippableItemSO")]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        public string ActionName => "Equip";

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemParameter
 = null)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if (weaponSystem != null)
            {
                weaponSystem.SetWeapon(this, itemParameter
 == null ? 
                    DefaultParametersList : itemParameter
);
                return true;
            }
            return false;
        }
    }
}