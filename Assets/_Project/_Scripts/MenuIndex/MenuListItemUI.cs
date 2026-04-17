using System; // <--- ต้องใส่ System เพื่อใช้ Action
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // <--- ต้องใส่เพื่อใช้ IPointerClickHandler
using Inventory.Model;
public class MenuListItemUI : MonoBehaviour, IPointerClickHandler
{
    public List<GameObject> lockIcons;
    public Image menuImage;
    public GameObject emptyItemImage;
    // ลบ Button ทิ้งไปเลย!

    private int _myIndex;
    private bool _isInteractable = false; // เอาไว้เช็คว่าช่องนี้กดได้ไหม

    // สร้าง Event ไว้ให้ Manager มาดักฟัง
    public event Action<int> OnItemClicked;

    // ฟังก์ชันตั้งค่าตอนสร้างปุ่ม
    public void Setup(CookingRecipeSO recipe, int index, bool isFinished)
    {
        _myIndex = index;

        if (recipe == null)
        {
            EmptyMenu();
        }
        else
        {
            menuImage.sprite = recipe.resultItem.ItemImage;
            if (isFinished) FinishedMenu();
            else LockMenu();
        }
    }

    private void LockMenu()
    {
        foreach (var lockObj in lockIcons) lockObj.SetActive(true);
        menuImage.gameObject.SetActive(true);
        //menuImage.color = Color.black;
        emptyItemImage.SetActive(false);
        _isInteractable = true; // ล็อคอยู่ก็ให้กดดูเงาได้
    }

    private void FinishedMenu()
    {
        foreach (var lockObj in lockIcons) lockObj.SetActive(false);
        menuImage.gameObject.SetActive(true);
        //menuImage.color = Color.white;
        emptyItemImage.SetActive(false);
        _isInteractable = true;
    }

    private void EmptyMenu()
    {
        foreach (var lockObj in lockIcons) lockObj.SetActive(false);
        menuImage.gameObject.SetActive(false);
        emptyItemImage.SetActive(true);
        _isInteractable = false; // ช่องว่าง ไม่ต้องให้กด
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติเมื่อผู้เล่นเอาเมาส์มาคลิกที่ UI ตัวนี้
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isInteractable)
        {
            // ตะโกนบอก Manager พร้อมส่งเลข Index ของตัวเองไปให้
            OnItemClicked?.Invoke(_myIndex);
        }
    }
}