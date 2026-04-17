using System;
using System.Collections.Generic; // <--- อย่าลืมใส่สำหรับ List
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Inventory.Model;

public class RecipesItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TMP_Text nameText;
    public Image borderHighlight;
    public Image blackAlpha;

    // *** เพิ่ม List ไอคอนล็อค ***
    public List<GameObject> lockIcons;

    public CookingRecipeSO RecipeData { get; private set; }
    public event Action<RecipesItemUI> OnItemClicked;

    // *** รับค่า bool isFinished มาจาก Manager ***
    public void Setup(CookingRecipeSO recipe, bool isFinished)
    {
        RecipeData = recipe;
        icon.sprite = recipe.resultItem.ItemImage;

        if (isFinished) Finished();
        else LockIcon();

        Deselect();
    }

    private void LockIcon()
    {
        nameText.text = RecipeData.resultItem.ItemName;  //"???"; // ซ่อนชื่อถ้ายังไม่เคยทำ
        foreach (var lockObj in lockIcons) lockObj.SetActive(true);
        // icon.color = Color.black; // (ถ้าอยากให้ภาพดำเปิดคอมเมนต์นี้)
    }

    private void Finished()
    {
        nameText.text = RecipeData.resultItem.ItemName; // โชว์ชื่อ
        foreach (var lockObj in lockIcons) lockObj.SetActive(false);
        // icon.color = Color.white;
    }

    public void SetAvailability(bool canAfford)
    {
        if (blackAlpha != null)
        {
            blackAlpha.enabled = !canAfford;
        }
    }

    public void Select() { if (borderHighlight != null) borderHighlight.enabled = true; }
    public void Deselect() { if (borderHighlight != null) borderHighlight.enabled = false; }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnItemClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData) { }
}