using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "New EatableItem", menuName = "Inventory/Item/EatableItemSO")]
    public class EatableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeReference, SubclassSelector]
        public List<ItemModifier> modifiers = new List<ItemModifier>();

        public string ActionName => "Consume";

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character)
        {
            if (modifiers.Count <= 0) return false;

            foreach (ItemModifier modifier in modifiers)
            {
                if(modifier != null) modifier.AffectCharacter(character);
            }
            return true;
        }
    }
}