using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    public abstract class ItemSO : ScriptableObject
    {
        [field: SerializeField]
        public string Name { get; set; }

        [field: SerializeField]
        [field: TextArea]
        public string Description { get; set; }

        [field: SerializeField]
        public Sprite ItemImage { get; set; }

        [field: SerializeField]
        public bool IsStackable { get; set; }

        public int ID => GetInstanceID();

        [field: SerializeField]
        public int MaxStackSize { get; set; } = 1;

        //������� Dubility
        [HideInInspector]
        public List<ItemParameter> DefaultParametersList { get; set; }

    }

    [Serializable]
    public struct ItemParameter // : IEquatable<ItemParameter>
    {
        public ItemParameterSO itemParameterSO;
        public float value;

        //public bool Equals(ItemParameter other)
        //{
        //    return other.itemParameter == itemParameter;
        //}
    }

    public interface IDestroyableItem
    {

    }

    public interface IItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        bool PerformAction(GameObject character, List<ItemParameter> itemParameter
);
    }

    [Serializable]
    public class ModifierData
    {
        public ItemModifierSO statModifierSO;
        public float value;
    }
}

