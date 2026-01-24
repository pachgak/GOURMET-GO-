using UnityEngine;
using Inventory.Model;

[CreateAssetMenu(fileName = "New GeneralItem", menuName = "Inventory/Item/GeneralItemSO")]
public class GeneralItemSO : ItemSO, IDestroyableItem
{
    // ไม่ต้องมี IItemAction เพราะมันไม่มี Action ให้ทำ
    // ไอเทมนี้จะใช้แค่เก็บค่าพื้นฐาน (Name, Description, Stack, Image)
}