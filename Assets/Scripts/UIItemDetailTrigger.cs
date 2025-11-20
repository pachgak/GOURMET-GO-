using Inventory.Model;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIItemDetailTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private ItemDetailPromptController detailPrompt; // ลากตัว Prompt Controller มาใส่
    
    private ItemSO _currentItem;

    // ฟังก์ชันสำหรับให้ CookingStationController ส่งข้อมูลมาให้
    public void SetItemData(ItemSO item)
    {
        _currentItem = item;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_currentItem == null) return;

        // สั่งเปิด Tooltip
        detailPrompt.Toggle(true);
        detailPrompt.SetDescription(_currentItem.ItemImage, _currentItem.ItemName, _currentItem.Description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // สั่งปิด Tooltip
        detailPrompt.Toggle(false);
    }

    // กันเหนียว: ถ้า Object ถูกปิดไปขณะเมาส์ชี้อยู่ ให้ปิด Tooltip ด้วย
    private void OnDisable()
    {
        if (detailPrompt != null)
        {
            detailPrompt.Toggle(false);
        }
    }
}