using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Inventory.Model;

public class RecipesItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TMP_Text nameText;
    public Image borderHighlight; // เอาไว้เปิด/ปิด กรอบตอนโดนเลือก
    public Image blackAlpha;

    public CookingRecipeSO RecipeData { get; private set; }

    // ส่ง Event กลับไปหา Controller ว่าตัวนี้โดนคลิก
    public event Action<RecipesItemUI> OnItemClicked;

    public void Setup(CookingRecipeSO recipe)
    {
        RecipeData = recipe;
        icon.sprite = recipe.resultItem.ItemImage;
        nameText.text = recipe.resultItem.ItemName;
        Deselect();
    }

    public void SetAvailability(bool canAfford)
    {
        if (blackAlpha != null)
        {
            // ถ้าวัตถุดิบพอ (canAfford = true) ให้ปิดภาพดำ (enabled = false)
            // ถ้าวัตถุดิบไม่พอ (canAfford = false) ให้เปิดภาพดำ (enabled = true)
            blackAlpha.enabled = !canAfford;
        }
    }

    public void Select() { if (borderHighlight != null) borderHighlight.enabled = true; }
    public void Deselect() { if (borderHighlight != null) borderHighlight.enabled = false; }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnItemClicked?.Invoke(this); // บอก Controller ว่าฉันโดนเลือกแล้วนะ!
    }

    // เผื่ออนาคตอยากทำเอฟเฟกต์ตอนเมาส์ชี้
    public void OnPointerEnter(PointerEventData eventData) { /* ย่อขยายภาพนิดหน่อย */ }
    public void OnPointerExit(PointerEventData eventData) { /* กลับขนาดเดิม */ }
}