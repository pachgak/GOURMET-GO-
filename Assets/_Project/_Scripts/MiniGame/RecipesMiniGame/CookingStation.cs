using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

public class CookingStation : MonoBehaviour
{
    [Header("Station Settings")]
    public string stationName;
    public Sprite toolIcon;
    public List<CookingRecipeSO> recipesForThisStation;
    public MiniGameBase miniGameType;

    [Header("Inventory Access")]
    [Tooltip("ใส่ได้ทั้งกระเป๋าผู้เล่น และตู้เย็นเมนูจะเช็คของรวมกันให้")]
    public List<InventorySO> inventoriesToCheck;

    public void OpenStationUI()
    {
        // ส่งตัวเอง (this) ไปให้ UI Manager จัดการต่อ
        if (RecipeMiniGameUIManager.Instance != null)
        {
            RecipeMiniGameUIManager.Instance.Open(this);
        }
    }
}