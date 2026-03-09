using Inventory.Model;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIItemDetailTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ไม่ต้อง serialize private แล้ว เพราะจะ set ผ่าน code
    private ItemDetailPromptController _detailPrompt;
    private ItemSO _currentItem;

    // ฟังก์ชันสำหรับรับข้อมูล Item
    public void SetItemData(ItemSO item)
    {
        _currentItem = item;
    }

    // *** ฟังก์ชันใหม่: สำหรับรับ Reference ของ Prompt Controller ***
    public void SetPromptController(ItemDetailPromptController controller)
    {
        _detailPrompt = controller;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_currentItem == null || _detailPrompt == null) return;

        _detailPrompt.Toggle(true);
        _detailPrompt.SetDescription(_currentItem.ItemImage, _currentItem.ItemName, _currentItem.Description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_detailPrompt != null)
        {
            _detailPrompt.Toggle(false);
        }
    }

    private void OnDisable()
    {
        if (_detailPrompt != null) _detailPrompt.Toggle(false);
    }
}