using System.Collections.Generic;
using System.Text;
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

        // *** Override Method ที่เราสร้างไว้ ***
        public override string GetDescription()
        {
            StringBuilder sb = new StringBuilder();

            // 1. ดึงข้อความ Description ปกติมาใส่ก่อน
            sb.Append(base.GetDescription());

            // 2. เช็คว่ามี Modifier ไหม ถ้ามีให้เอามาต่อท้าย
            if (modifiers != null && modifiers.Count > 0)
            {
                sb.AppendLine(); // ขึ้นบรรทัดใหม่
                sb.AppendLine();
                sb.AppendLine("<color=#FFD700>คุณสมบัติไอเทม:</color>"); // พาดหัวสีทอง

                foreach (ItemModifier modifier in modifiers)
                {
                    if (modifier != null)
                    {
                        string modDesc = modifier.GetDescription();
                        if (!string.IsNullOrEmpty(modDesc)) // ถ้ามีคำอธิบาย ค่อยเอามาต่อ
                        {
                            sb.AppendLine(modDesc);
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}